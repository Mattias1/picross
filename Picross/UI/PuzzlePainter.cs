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
        private PuzzleBoard board;

        private Puzzle puzzle => board.Puzzle;
        private Puzzle puzzleForNumbers => board.BackUpOriginalPuzzle ?? board.Puzzle;

        private Color[] colors;
        private Font numberFont;

        private Point minInnerOffset;
        private Point maxSize;
        private Point size;

        public Point Offset;
        public Point InnerOffset { get; set; }
        public Point Size {
            get { return this.size; }
            set {
                this.maxSize = value;
                this.calculateSizeAndInnerOffset();
            }
        }

        private void calculateSizeAndInnerOffset() {
            const int MiN_SQUARE_SIZE = 5;
            const int MAX_EXTRA_SPACE = 20;

            int maxSquareWidth = (this.maxSize.X - this.minInnerOffset.X) / this.puzzle.Width;
            int maxSquareHeight = (this.maxSize.Y - this.minInnerOffset.Y) / this.puzzle.Height;
            int squareSize = Math.Max(MiN_SQUARE_SIZE, Math.Min(maxSquareWidth, maxSquareHeight));

            int extraWidth = Math.Min(MAX_EXTRA_SPACE, this.maxSize.X - this.minInnerOffset.X - this.puzzle.Width * squareSize);
            int extraHeight = Math.Min(MAX_EXTRA_SPACE, this.maxSize.Y - this.minInnerOffset.Y - this.puzzle.Height * squareSize);
            this.InnerOffset = new Point(this.minInnerOffset.X + extraWidth, this.minInnerOffset.Y + extraHeight);

            this.size = new Point(this.InnerOffset.X + squareSize * this.puzzle.Width + 1, this.InnerOffset.Y + squareSize * this.puzzle.Height + 1);
        }

        public PuzzlePainter(PuzzleBoard board) {
            this.numberFont = new Font("Arial", 12);
            this.board = board;

            this.Offset = new Point(10, 10);
            this.minInnerOffset = new Point(120, 100);
            this.Size = new Point(20 * this.puzzle.Width, 20 * this.puzzle.Height); // Dummy size

            Settings s = Settings.Get;
            this.colors = new Color[5] {
                s.GetColor(Field.Decoration), s.GetColor(Field.Empty), s.GetColor(Field.Unknown), s.GetColor(Field.Black), s.GetColor(Field.Red)
            };
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
                Color c = this.GetColor(f);
                if (c.R == color.R && c.G == color.G && c.B == color.B)
                    return f;
            }

            return Field.Unknown;
        }

        public int CalculateSquareSize() {
            return (this.size.X - this.InnerOffset.X) / this.puzzle.Width;
        }

        public void Draw(Graphics graphics, Point mouse, Field selectedColour) {
            // Draw it to a bitmap
            adjustToNumberSizes();
            int squareSize = this.CalculateSquareSize();

            Point hover = PuzzleBoard.Mouse2Point(mouse, squareSize, this);
            Bitmap bmp = this.drawToBitmap(squareSize, hover, selectedColour, true, Settings.Get.DarkerBackground);

            // Draw that bitmap to form
            graphics.DrawImage(bmp, this.Offset);
        }

        private bool adjustToNumberSizes() {
            bool needsResizing = false;

            // Update minimum InnerOffset width
            for (int y = 0; y < this.puzzle.Height; y++) {
                string nrs = this.puzzleForNumbers.GetRowNumbers(y);
                int nrsWidth = TextRenderer.MeasureText(nrs, this.numberFont).Width;
                if (nrsWidth > this.minInnerOffset.X) {
                    this.minInnerOffset = new Point(nrsWidth + 6, this.minInnerOffset.Y);
                    needsResizing = true;
                }
            }
            // Update minimum InnerOffset height
            for (int x = 0; x < this.puzzle.Width; x++) {
                string nrs = this.puzzleForNumbers.GetColNumbers(x);
                int nrsHeight = TextRenderer.MeasureText(nrs, this.numberFont).Height;
                if (nrsHeight > this.minInnerOffset.Y) {
                    this.minInnerOffset = new Point(this.minInnerOffset.X, nrsHeight + 8);
                    needsResizing = true;
                }
            }

            // Update the Size and squaresize
            if (needsResizing) {
                this.calculateSizeAndInnerOffset();
            }

            return needsResizing;
        }

        public Bitmap ToBitmap(bool solution) {
            return this.drawToBitmap(32, new Point(-1, -1), Field.Black, solution);
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
            Bitmap bmp = new Bitmap(this.size.X, this.size.Y);
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
                        this.fillRectangle(g, this.getBrushColor(this.puzzle[x, y], darkerBackground), squareSize, x, y);
            }
        }

        private void drawHover(int squareSize, Point hover, Field selectedColour, Graphics g) {
            if (this.puzzle.IsInRange(hover)) {
                Color hoverColor = MathHelper.Lerp(this.GetColor(selectedColour), Color.White, 0.5f);
                this.fillRectangle(g, hoverColor, squareSize, hover.X, hover.Y);
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
                        this.fillRectangle(g, hoverColor, squareSize, hover.X, y);
                }
            }

            else if (!xOk && yOk) {
                bool[] autoblanks = AutoBlanker.GetRow(this.puzzle, this.puzzleForNumbers, hover.Y);
                for (int x = 0; x < autoblanks.Length; x++) {
                    if (autoblanks[x])
                        this.fillRectangle(g, hoverColor, squareSize, x, hover.Y);
                }
            }
        }

        private void drawLines(int squareSize, Bitmap bmp, Graphics g) {
            // Draw the lines seperating (and enclosing) the fields
            for (int y = 0; y < this.puzzle.Height + 1; y++)
                drawVerticalLine(g, bmp, squareSize, y);
            for (int x = 0; x < this.puzzle.Width + 1; x++)
                drawHorizontalLine(g, bmp, squareSize, x);

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
                            this.fillRectangle(g, Color.Black, squareSize, x, y, offset);
                }
            }
        }

        private void fillRectangle(Graphics g, Color color, int squareSize, int x, int y, Point? innerOffset = null) {
            Point offset = innerOffset ?? this.InnerOffset;
            g.FillRectangle(new SolidBrush(color), offset.X + squareSize * x, offset.Y + squareSize * y, squareSize, squareSize);
        }

        private void drawVerticalLine(Graphics g, Bitmap bmp, int squareSize, int y) {
            g.DrawLine(this.getPen(y, this.puzzle.Height), 1, this.InnerOffset.Y + squareSize * y, bmp.Width - 1, this.InnerOffset.Y + squareSize * y);
        }

        private void drawHorizontalLine(Graphics g, Bitmap bmp, int squareSize, int x) {
            g.DrawLine(this.getPen(x, this.puzzle.Width), this.InnerOffset.X + squareSize * x, 1, this.InnerOffset.X + squareSize * x, bmp.Height - 1);
        }

        private Color getBrushColor(Field type, bool darkerBackground) {
            // Get the brush with the right colour (used for drawing squares)
            if (darkerBackground && type == Field.Unknown)
                return Color.LightGray;
            return this.GetColor(type);
        }

        private Pen getPen(int i, int last) {
            // Get the pen with the right colour (used for drawing lines)
            if (i % 5 == 0 || i == last)
                return new Pen(Color.Black, 2);
            return new Pen(Color.Gray, 2);
        }
    }
}
