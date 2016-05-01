using System.Drawing;

namespace Picross
{
    class PuzzlePainter
    {
        private Puzzle puzzle;
        private Color[] colors; // Array is 0 based, so const values + 2.

        public Point InnerOffset
        {
            get { return this.puzzle.InnerOffset; }
            set { this.puzzle.InnerOffset = value; }
        }

        public PuzzlePainter(Puzzle puzzle) {
            this.puzzle = puzzle;
            Settings s = Settings.Get;
            this.colors = new Color[5] {
                s.GetColor(Puzzle.Decoration), s.GetColor(Puzzle.Empty), s.GetColor(Puzzle.Unknown), s.GetColor(Puzzle.Black), s.GetColor(Puzzle.Red)
            };
        }

        public Color GetColor(int type) {
            return this.colors[type + 2];
        }
        public bool SetColor(int type, Color color) {
            // Don't set a colour that is already used
            for (int i = 0; i < this.colors.Length; i++)
                if (this.colors[i].R == color.R && this.colors[i].G == color.G && this.colors[i].B == color.B)
                    return false;
            // Set the colour
            this.colors[type + 2] = color;
            return true;
        }

        public void Draw(Graphics graphics, Point mouse, int selectedColour = Puzzle.Black) {
            // Draw it to a bitmap
            int squareSize = (this.puzzle.Size.X - this.InnerOffset.X) / this.puzzle.Width;
            Bitmap bmp = this.drawToBitmap(squareSize, mouse, selectedColour, true, Settings.Get.DarkerBackground);

            // Draw that bitmap to form
            graphics.DrawImage(bmp, this.puzzle.Offset);
        }

        public Bitmap ToBitmap(bool solution) {
            return this.drawToBitmap(32, new Point(-1, -1), Puzzle.Black, solution);
        }

        private Bitmap drawToBitmap(int squareSize, Point mouse, int selectedColour, bool fillSquares = true, bool darkerBackground = false) {
            // Prepare the bitmap
            Bitmap bmp = new Bitmap(this.InnerOffset.X + squareSize * this.puzzle.Width + 1, this.InnerOffset.Y + squareSize * this.puzzle.Height + 1);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(darkerBackground ? Color.LightGray : this.GetColor(Puzzle.Unknown));

            // Draw the square colours
            if (fillSquares) {
                for (int y = 0; y < this.puzzle.Height; y++)
                    for (int x = 0; x < this.puzzle.Width; x++)
                        g.FillRectangle(this.GetBrush(this.puzzle[x, y], darkerBackground), this.InnerOffset.X + squareSize * x, this.InnerOffset.Y + squareSize * y, squareSize, squareSize);
            }

            // Draw the hover
            Point hover = this.puzzle.Mouse2Point(mouse, squareSize);
            if (this.puzzle.IsInRange(hover)) {
                Color hoverColor = GameMath.Lerp(this.GetColor(selectedColour), Color.White, 0.5f);
                g.FillRectangle(new SolidBrush(hoverColor), this.InnerOffset.X + squareSize * hover.X, this.InnerOffset.Y + squareSize * hover.Y, squareSize, squareSize);
            }

            // Draw the horizontal (row) and vertical (col) numbers (also check if they are not too long, if so, change the inneroffset)
            Font f = new Font("Arial", 12);
            int yExtra = (squareSize - (int)g.MeasureString("1", f).Height) / 2 + 1;
            for (int y = 0; y < this.puzzle.Height; y++) {
                string nrs = this.puzzle.GetRowNumbers(y);
                g.DrawString(nrs, f, Brushes.Black, this.InnerOffset.X - g.MeasureString(nrs, f).Width - 4, this.InnerOffset.Y + y * squareSize + yExtra);
                if (g.MeasureString(nrs, f).Width > this.InnerOffset.X)
                    this.InnerOffset = new Point((int)g.MeasureString(nrs, f).Width + 4, this.InnerOffset.Y);
            }
            for (int x = 0; x < this.puzzle.Width; x++) {
                string nrs = this.puzzle.GetColNumbers(x);
                g.DrawString(nrs, f, Brushes.Black, this.InnerOffset.X + x * squareSize + (squareSize - (int)g.MeasureString(nrs, f).Width) / 2, this.InnerOffset.Y - g.MeasureString(nrs, f).Height);
                if (g.MeasureString(nrs, f).Height > this.InnerOffset.Y)
                    this.InnerOffset = new Point(this.InnerOffset.X, (int)g.MeasureString(nrs, f).Height + 1);
            }

            // Draw the horizontal and vertical lines
            for (int y = 0; y < this.puzzle.Height + 1; y++)
                g.DrawLine(this.GetPen(y, this.puzzle.Height), 1, this.InnerOffset.Y + squareSize * y, bmp.Width - 1, this.InnerOffset.Y + squareSize * y);
            for (int x = 0; x < this.puzzle.Width + 1; x++)
                g.DrawLine(this.GetPen(x, this.puzzle.Width), this.InnerOffset.X + squareSize * x, 1, this.InnerOffset.X + squareSize * x, bmp.Height - 1);

            // Override grey lines
            for (int y = 0; y < this.puzzle.Height; y += 5)
                g.DrawLine(this.GetPen(y, this.puzzle.Height), 1, this.InnerOffset.Y + squareSize * y, bmp.Width - 1, this.InnerOffset.Y + squareSize * y);
            for (int x = 0; x < this.puzzle.Width; x += 5)
                g.DrawLine(this.GetPen(x, this.puzzle.Width), this.InnerOffset.X + squareSize * x, 1, this.InnerOffset.X + squareSize * x, bmp.Height - 1);
            g.DrawLine(this.GetPen(this.puzzle.Height, this.puzzle.Height), 1, this.InnerOffset.Y + squareSize * this.puzzle.Height, bmp.Width - 1, this.InnerOffset.Y + squareSize * this.puzzle.Height);

            // Draw the lines to close the number boxes
            g.DrawLine(this.GetPen(0, 0), 1, this.InnerOffset.Y, 1, bmp.Height - 1);
            g.DrawLine(this.GetPen(0, 0), this.InnerOffset.X, 1, bmp.Width - 1, 1);

            // Draw a minimap
            if (fillSquares) {
                squareSize = 2;
                Point offset = new Point((this.InnerOffset.X - squareSize * this.puzzle.Width) / 2, (this.InnerOffset.Y - squareSize * this.puzzle.Height) / 2);
                for (int y = 0; y < this.puzzle.Height; y++)
                    for (int x = 0; x < this.puzzle.Width; x++)
                        if (this.puzzle[x, y] == Puzzle.Black || this.puzzle[x, y] == Puzzle.Red)
                            g.FillRectangle(new SolidBrush(Color.Black), offset.X + squareSize * x, offset.Y + squareSize * y, squareSize, squareSize);
            }

            // Return the bitmap
            return bmp;
        }

        private Brush GetBrush(int type, bool darkerBackground) {
            // Get the brush with the right colour (used for drawing squares)
            if (darkerBackground && type == Puzzle.Unknown)
                return Brushes.LightGray;
            return new SolidBrush(this.GetColor(type));
        }

        private Pen GetPen(int i, int last) {
            // Get the pen with the right colour (used for drawing lines)
            if (i % 5 == 0 || i == last)
                return new Pen(Color.Black, 2);
            return new Pen(Color.Gray, 2);
        }
    }
}
