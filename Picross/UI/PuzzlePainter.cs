using System;
using System.Drawing;
using System.Windows.Forms;
using Picross.Helpers;
using Picross.Model;
using Picross.Solvers;

namespace Picross.UI
{
    class PuzzlePainter
    {
        private const int MINIMUM_SQUARE_SIZE = 5;

        private Puzzle puzzle;
        private Puzzle puzzleForNumbers;
        private Color[] colors;
        private Font numberFont;

        public Point Offset;
        public Point InnerOffset { get; set; }
        private Point size;
        private Point maxSize;

        public Point Size {
            get { return this.size; }
            set {
                this.maxSize = value;
                int squareSize = Math.Max(MINIMUM_SQUARE_SIZE, Math.Min((value.X - this.InnerOffset.X) / this.puzzle.Width, (value.Y - this.InnerOffset.Y) / this.puzzle.Height));
                this.size = new Point(this.InnerOffset.X + squareSize * this.puzzle.Width, this.InnerOffset.Y + squareSize * this.puzzle.Height);
            }
        }

        public PuzzlePainter(Puzzle puzzle, Puzzle backupPuzzle) {
            this.SetPuzzleObjects(puzzle, backupPuzzle);
            this.numberFont = new Font("Arial", 12);

            this.Offset = new Point(10, 10);
            this.InnerOffset = new Point(120, 100);
            this.Size = new Point(20 * this.puzzle.Width, 20 * this.puzzle.Height);

            Settings s = Settings.Get;
            this.colors = new Color[5] {
                s.GetColor(Field.Decoration), s.GetColor(Field.Empty), s.GetColor(Field.Unknown), s.GetColor(Field.Black), s.GetColor(Field.Red)
            };
        }

        public void SetPuzzleObjects(Puzzle puzzle, Puzzle backupPuzzle) {
            this.puzzle = puzzle;
            this.puzzleForNumbers = backupPuzzle ?? puzzle;
        }

        public Color GetColor(Field type) {
            return this.colors[type.Index];
        }
        public bool SetColor(Field type, Color color) {
            // Don't set a colour that is already used
            for (int i = 0; i < this.colors.Length; i++) {
                if (this.colors[i].R == color.R && this.colors[i].G == color.G && this.colors[i].B == color.B)
                    return false;
            }
            // Set the colour
            this.colors[type.Index] = color;
            return true;
        }

        public Field GetType(Color color) {
            foreach (Field f in Field.All) {
                Color c = this.colors[f.Index];
                if (c.R == color.R && c.G == color.G && c.B == color.B)
                    return f;
            }

            return Field.Unknown;
        }

        public void Draw(Graphics graphics, Point mouse) {
            this.Draw(graphics, mouse, Field.Black);
        }
        public void Draw(Graphics graphics, Point mouse, Field selectedColour) {
            // Draw it to a bitmap
            int squareSize;
            adjustToNumberSizes(out squareSize);

            Point hover = PuzzleBoard.Mouse2Point(mouse, squareSize, this);
            Bitmap bmp = this.drawToBitmap(squareSize, hover, selectedColour, true, Settings.Get.DarkerBackground);

            // Draw that bitmap to form
            graphics.DrawImage(bmp, this.Offset);
        }

        public Bitmap ToBitmap(bool solution) {
            return this.drawToBitmap(32, new Point(-1, -1), Field.Black, solution);
        }

        public int CalculateSquareSize() {
            return (this.Size.X - this.InnerOffset.X) / this.puzzle.Width;
        }

        private bool adjustToNumberSizes(out int squareSize) {
            bool needsResizing = false;

            // Update InnerOffset width
            for (int y = 0; y < this.puzzle.Height; y++) {
                string nrs = this.puzzleForNumbers.GetRowNumbers(y);
                int nrsWidth = TextRenderer.MeasureText(nrs, this.numberFont).Width;
                if (nrsWidth > this.InnerOffset.X) {
                    this.InnerOffset = new Point(nrsWidth + 6, this.InnerOffset.Y);
                    needsResizing = true;
                }
            }
            // Update InnerOffset height
            for (int x = 0; x < this.puzzle.Width; x++) {
                string nrs = this.puzzleForNumbers.GetColNumbers(x);
                int nrsHeight = TextRenderer.MeasureText(nrs, this.numberFont).Height;
                if (nrsHeight > this.InnerOffset.Y) {
                    this.InnerOffset = new Point(this.InnerOffset.X, nrsHeight + 8);
                    needsResizing = true;
                }
            }

            // Update the Size and squaresize
            if (needsResizing)
                this.Size = this.maxSize;
            squareSize = this.CalculateSquareSize();

            return needsResizing;
        }

        private Bitmap drawToBitmap(int squareSize, Point hover, Field selectedColour, bool fillSquares = true, bool darkerBackground = false) {
            Graphics g;
            Bitmap bmp = this.initBitmap(squareSize, darkerBackground, out g);

            this.drawNumbers(squareSize, g);
            this.drawSquares(squareSize, fillSquares, darkerBackground, g);
            this.drawHover(squareSize, hover, selectedColour, g);
            if (this.puzzle != this.puzzleForNumbers)
                this.drawAutoblanksHover(squareSize, hover, g);
            this.drawLines(squareSize, bmp, g);
            this.drawMinimap(fillSquares, g);

            return bmp;
        }

        private Bitmap initBitmap(int squareSize, bool darkerBackground, out Graphics g) {
            Bitmap bmp = new Bitmap(this.InnerOffset.X + squareSize * this.puzzle.Width + 1, this.InnerOffset.Y + squareSize * this.puzzle.Height + 1);
            g = Graphics.FromImage(bmp);

            g.Clear(darkerBackground ? Color.LightGray : this.GetColor(Field.Unknown));

            return bmp;
        }

        private void drawNumbers(int squareSize, Graphics g) {
            int yExtra = (squareSize - TextRenderer.MeasureText("1", this.numberFont).Height) / 2 + 1;
            for (int y = 0; y < this.puzzle.Height; y++) {
                string nrs = this.puzzleForNumbers.GetRowNumbers(y);
                int nrsWidth = TextRenderer.MeasureText(nrs, this.numberFont).Width;
                g.DrawString(nrs, this.numberFont, Brushes.Black, this.InnerOffset.X - nrsWidth - 4, this.InnerOffset.Y + y * squareSize + yExtra);
            }
            for (int x = 0; x < this.puzzle.Width; x++) {
                string nrs = this.puzzleForNumbers.GetColNumbers(x);
                Size nrsSize = TextRenderer.MeasureText(nrs, this.numberFont);
                g.DrawString(nrs, this.numberFont, Brushes.Black, this.InnerOffset.X + x * squareSize + (squareSize - nrsSize.Width) / 2, this.InnerOffset.Y - nrsSize.Height - 4);
            }
        }

        private void drawSquares(int squareSize, bool fillSquares, bool darkerBackground, Graphics g) {
            if (fillSquares) {
                for (int y = 0; y < this.puzzle.Height; y++)
                    for (int x = 0; x < this.puzzle.Width; x++)
                        g.FillRectangle(this.getBrush(this.puzzle[x, y], darkerBackground), this.InnerOffset.X + squareSize * x, this.InnerOffset.Y + squareSize * y, squareSize, squareSize);
            }
        }

        private void drawHover(int squareSize, Point hover, Field selectedColour, Graphics g) {
            if (this.puzzle.IsInRange(hover)) {
                Color hoverColor = MathHelper.Lerp(this.GetColor(selectedColour), Color.White, 0.5f);
                g.FillRectangle(new SolidBrush(hoverColor), this.InnerOffset.X + squareSize * hover.X, this.InnerOffset.Y + squareSize * hover.Y, squareSize, squareSize);
            }
        }

        private void drawAutoblanksHover(int squareSize, Point hover, Graphics g) {
            if (!Settings.Get.UseAutoBlanker) {
                return;
            }

            Color hoverColor = MathHelper.Lerp(this.GetColor(Field.Empty), Color.White, 0.5f);
            bool xOk = this.puzzle.IsInRangeX(hover.X);
            bool yOk = this.puzzle.IsInRangeY(hover.Y);

            if (xOk && !yOk) {
                bool[] autoblanks = AutoBlanker.GetCol(this.puzzle, this.puzzleForNumbers, hover.X);
                for (int y = 0; y < autoblanks.Length; y++) {
                    if (autoblanks[y])
                        g.FillRectangle(new SolidBrush(hoverColor), this.InnerOffset.X + squareSize * hover.X, this.InnerOffset.Y + squareSize * y, squareSize, squareSize);
                }
            }

            else if (!xOk && yOk) {
                bool[] autoblanks = AutoBlanker.GetRow(this.puzzle, this.puzzleForNumbers, hover.Y);
                for (int x = 0; x < autoblanks.Length; x++) {
                    if (autoblanks[x])
                        g.FillRectangle(new SolidBrush(hoverColor), this.InnerOffset.X + squareSize * x, this.InnerOffset.Y + squareSize * hover.Y, squareSize, squareSize);
                }
            }
        }

        private void drawLines(int squareSize, Bitmap bmp, Graphics g) {
            // Draw the horizontal and vertical lines
            for (int y = 0; y < this.puzzle.Height + 1; y++)
                g.DrawLine(this.getPen(y, this.puzzle.Height), 1, this.InnerOffset.Y + squareSize * y, bmp.Width - 1, this.InnerOffset.Y + squareSize * y);
            for (int x = 0; x < this.puzzle.Width + 1; x++)
                g.DrawLine(this.getPen(x, this.puzzle.Width), this.InnerOffset.X + squareSize * x, 1, this.InnerOffset.X + squareSize * x, bmp.Height - 1);

            // Override grey lines
            for (int y = 0; y < this.puzzle.Height; y += 5)
                g.DrawLine(this.getPen(y, this.puzzle.Height), 1, this.InnerOffset.Y + squareSize * y, bmp.Width - 1, this.InnerOffset.Y + squareSize * y);
            for (int x = 0; x < this.puzzle.Width; x += 5)
                g.DrawLine(this.getPen(x, this.puzzle.Width), this.InnerOffset.X + squareSize * x, 1, this.InnerOffset.X + squareSize * x, bmp.Height - 1);
            g.DrawLine(this.getPen(this.puzzle.Height, this.puzzle.Height), 1, this.InnerOffset.Y + squareSize * this.puzzle.Height, bmp.Width - 1, this.InnerOffset.Y + squareSize * this.puzzle.Height);

            // Draw the lines to close the number boxes
            g.DrawLine(this.getPen(0, 0), 1, this.InnerOffset.Y, 1, bmp.Height - 1);
            g.DrawLine(this.getPen(0, 0), this.InnerOffset.X, 1, bmp.Width - 1, 1);
        }

        private void drawMinimap(bool fillSquares, Graphics g) {
            if (fillSquares) {
                int squareSize = 2;
                Point offset = new Point((this.InnerOffset.X - squareSize * this.puzzle.Width) / 2, (this.InnerOffset.Y - squareSize * this.puzzle.Height) / 2);
                for (int y = 0; y < this.puzzle.Height; y++) {
                    for (int x = 0; x < this.puzzle.Width; x++)
                        if (this.puzzle[x, y] == Field.Black || this.puzzle[x, y] == Field.Red)
                            g.FillRectangle(new SolidBrush(Color.Black), offset.X + squareSize * x, offset.Y + squareSize * y, squareSize, squareSize);
                }
            }
        }

        private Brush getBrush(Field type, bool darkerBackground) {
            // Get the brush with the right colour (used for drawing squares)
            if (darkerBackground && type == Field.Unknown)
                return Brushes.LightGray;
            return new SolidBrush(this.GetColor(type));
        }

        private Pen getPen(int i, int last) {
            // Get the pen with the right colour (used for drawing lines)
            if (i % 5 == 0 || i == last)
                return new Pen(Color.Black, 2);
            return new Pen(Color.Gray, 2);
        }
    }
}
