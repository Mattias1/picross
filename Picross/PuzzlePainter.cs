﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Picross
{
    class PuzzlePainter
    {
        private Puzzle puzzle;
        private Color[] colors; // Array is 0 based, so const values + 2.
        private Font numberFont;

        public Point Offset;
        public Point InnerOffset { get; set; }
        private Point size;
        private Point maxSize;

        public Point Size
        {
            get { return this.size; }
            set
            {
                this.maxSize = value;
                int squareSize = Math.Max(5, Math.Min((value.X - this.InnerOffset.X) / this.puzzle.Width, (value.Y - this.InnerOffset.Y) / this.puzzle.Height)); // Minimum square size is 5
                this.size = new Point(this.InnerOffset.X + squareSize * this.puzzle.Width, this.InnerOffset.Y + squareSize * this.puzzle.Height);
            }
        }

        public PuzzlePainter(Puzzle puzzle) {
            this.puzzle = puzzle;
            this.numberFont = new Font("Arial", 12);

            this.Offset = new Point(10, 10);
            this.InnerOffset = new Point(120, 100);
            this.Size = new Point(20 * this.puzzle.Width, 20 * this.puzzle.Height);

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
            int squareSize;
            adjustToNumberSizes(out squareSize);
            Bitmap bmp = this.drawToBitmap(squareSize, mouse, selectedColour, true, Settings.Get.DarkerBackground);

            // Draw that bitmap to form
            graphics.DrawImage(bmp, this.Offset);
        }

        public Bitmap ToBitmap(bool solution) {
            return this.drawToBitmap(32, new Point(-1, -1), Puzzle.Black, solution);
        }

        public int CalculateSquareSize() {
            return (this.Size.X - this.InnerOffset.X) / this.puzzle.Width;
        }

        private bool adjustToNumberSizes(out int squareSize) {
            bool needsResizing = false;

            // Update InnerOffset width
            for (int y = 0; y < this.puzzle.Height; y++) {
                string nrs = this.puzzle.GetRowNumbers(y);
                int nrsWidth = TextRenderer.MeasureText(nrs, this.numberFont).Width;
                if (nrsWidth > this.InnerOffset.X) {
                    this.InnerOffset = new Point(nrsWidth + 6, this.InnerOffset.Y);
                    needsResizing = true;
                }
            }
            // Update InnerOffset height
            for (int x = 0; x < this.puzzle.Width; x++) {
                string nrs = this.puzzle.GetColNumbers(x);
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

        private Bitmap drawToBitmap(int squareSize, Point mouse, int selectedColour, bool fillSquares = true, bool darkerBackground = false) {
            Graphics g;
            Bitmap bmp = this.initBitmap(squareSize, darkerBackground, out g);

            this.drawNumbers(squareSize, g);
            this.drawSquares(squareSize, fillSquares, darkerBackground, g);
            this.drawHover(squareSize, mouse, selectedColour, g);
            this.drawLines(squareSize, bmp, g);
            this.drawMinimap(fillSquares, g);

            return bmp;
        }

        private Bitmap initBitmap(int squareSize, bool darkerBackground, out Graphics g) {
            Bitmap bmp = new Bitmap(this.InnerOffset.X + squareSize * this.puzzle.Width + 1, this.InnerOffset.Y + squareSize * this.puzzle.Height + 1);
            g = Graphics.FromImage(bmp);

            g.Clear(darkerBackground ? Color.LightGray : this.GetColor(Puzzle.Unknown));

            return bmp;
        }

        private void drawNumbers(int squareSize, Graphics g) {
            int yExtra = (squareSize - TextRenderer.MeasureText("1", this.numberFont).Height) / 2 + 1;
            for (int y = 0; y < this.puzzle.Height; y++) {
                string nrs = this.puzzle.GetRowNumbers(y);
                int nrsWidth = TextRenderer.MeasureText(nrs, this.numberFont).Width;
                g.DrawString(nrs, this.numberFont, Brushes.Black, this.InnerOffset.X - nrsWidth - 4, this.InnerOffset.Y + y * squareSize + yExtra);
            }
            for (int x = 0; x < this.puzzle.Width; x++) {
                string nrs = this.puzzle.GetColNumbers(x);
                Size nrsSize = TextRenderer.MeasureText(nrs, this.numberFont);
                g.DrawString(nrs, this.numberFont, Brushes.Black, this.InnerOffset.X + x * squareSize + (squareSize - nrsSize.Width) / 2, this.InnerOffset.Y - nrsSize.Height - 4);
            }
        }

        private void drawSquares(int squareSize, bool fillSquares, bool darkerBackground, Graphics g) {
            if (fillSquares) {
                for (int y = 0; y < this.puzzle.Height; y++)
                    for (int x = 0; x < this.puzzle.Width; x++)
                        g.FillRectangle(this.GetBrush(this.puzzle[x, y], darkerBackground), this.InnerOffset.X + squareSize * x, this.InnerOffset.Y + squareSize * y, squareSize, squareSize);
            }
        }

        private void drawHover(int squareSize, Point mouse, int selectedColour, Graphics g) {
            Point hover = this.puzzle.Mouse2Point(mouse, squareSize);
            if (this.puzzle.IsInRange(hover)) {
                Color hoverColor = GameMath.Lerp(this.GetColor(selectedColour), Color.White, 0.5f);
                g.FillRectangle(new SolidBrush(hoverColor), this.InnerOffset.X + squareSize * hover.X, this.InnerOffset.Y + squareSize * hover.Y, squareSize, squareSize);
            }
        }

        private void drawLines(int squareSize, Bitmap bmp, Graphics g) {
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
        }

        private void drawMinimap(bool fillSquares, Graphics g) {
            if (fillSquares) {
                int squareSize = 2;
                Point offset = new Point((this.InnerOffset.X - squareSize * this.puzzle.Width) / 2, (this.InnerOffset.Y - squareSize * this.puzzle.Height) / 2);
                for (int y = 0; y < this.puzzle.Height; y++)
                    for (int x = 0; x < this.puzzle.Width; x++)
                        if (this.puzzle[x, y] == Puzzle.Black || this.puzzle[x, y] == Puzzle.Red)
                            g.FillRectangle(new SolidBrush(Color.Black), offset.X + squareSize * x, offset.Y + squareSize * y, squareSize, squareSize);
            }
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