using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Picross
{
    class PuzzleBoard
    {
        // Members
        private Puzzle puzzleObject;
        private Puzzle backUpOriginalPuzzle;

        private bool editorMode;

        private PuzzlePainter painter;

        // Getters and setters
        private Puzzle puzzle
        {
            get { return this.puzzleObject; }
            set
            {
                this.puzzleObject = value;
                if (this.painter == null)
                    this.painter = new PuzzlePainter(value, this.backUpOriginalPuzzle);
                else
                    this.painter.SetPuzzleObjects(value, this.backUpOriginalPuzzle);
            }
        }
        public bool EditorMode
        {
            get { return this.editorMode; }
            set
            {
                if (this.editorMode == value)
                    return;
                this.editorMode = value;
                if (value) {
                    if (this.backUpOriginalPuzzle != null && !this.backUpOriginalPuzzle.IsEmpty())
                        this.puzzleObject = this.backUpOriginalPuzzle;
                    this.backUpOriginalPuzzle = null;
                }
                else {
                    this.backUpOriginalPuzzle = this.puzzle;
                    this.puzzleObject = new Puzzle(this.backUpOriginalPuzzle.Width, this.backUpOriginalPuzzle.Height);
                }
                this.painter.SetPuzzleObjects(this.puzzle, this.backUpOriginalPuzzle);
            }
        }

        public Point PuzzleSize => this.puzzle.Size;

        public PuzzlePainter Painter => this.painter;

        // Methods for communication with the outside world
        public PuzzleBoard(int w, int h, bool editormode)
            : this(new Puzzle(w, h), editormode) { }
        public PuzzleBoard(Puzzle puzzle, bool editormode) {
            this.puzzle = puzzle;         // Also sets the painter
            this.backUpOriginalPuzzle = null;
            this.EditorMode = editormode; // This may override the backup puzzle and will set the painter (again)
        }

        public void MouseClick(Point mouse, int value) {
            Point p = this.Mouse2Point(mouse, this.painter.CalculateSquareSize());
            doMouseClick(p, value);
        }
        public void MouseClick(Point oldMouse, Point newMouse, int value) {
            Point from = this.Mouse2Point(oldMouse, this.painter.CalculateSquareSize());
            Point to = this.Mouse2Point(newMouse, this.painter.CalculateSquareSize());
            if (from.Y == to.Y) {
                while (to.X != from.X) {
                    doMouseClick(to, value);
                    to.X += (to.X < from.X) ? 1 : -1;
                }
            }
            else if (from.X == to.X) {
                while (to.Y != from.Y) {
                    doMouseClick(to, value);
                    to.Y += (to.Y < from.Y) ? 1 : -1;
                }
            }
            else {
                doMouseClick(to, value);
            }
        }

        public static PuzzleBoard FromString(string puzzleString) {
            Puzzle puzzle = Puzzle.FromString(puzzleString);

            return new PuzzleBoard(puzzle, true);
        }

        public int MouseMoved(Point lastMouse, Point nextMouse) {
            // 0: not moved
            // 1: moved horizontally
            // 2: moved vertically
            // 3: moved both
            if (lastMouse.X == nextMouse.X && lastMouse.Y == nextMouse.Y)
                return 0;
            // Check if both mouse coordinates map to the same point, if not, the mouse moved significantly.
            int squareSize = (this.painter.Size.X - this.painter.InnerOffset.X) / this.puzzle.Width;
            Point a = this.Mouse2Point(lastMouse, squareSize);
            Point b = this.Mouse2Point(nextMouse, squareSize);
            // Figure out how they moved
            if (a.X == b.X) {
                if (a.Y == b.Y)
                    return 0;
                return 2;
            }
            else {
                if (a.Y == b.Y)
                    return 1;
                return 3;
            }
        }

        public void Clear() {
            this.puzzle.Clear();
        }

        public void ChangeSize(Point size) {
            this.puzzle.ChangeSize(size);
        }

        public void Move(Point move) {
            this.puzzle.Move(move);
        }

        // Helper methods
        public Point Mouse2Point(Point mouse, int squareSize) {
            // Get the array index corresponding to the mouse coordinate
            return Mouse2Point(mouse, squareSize, this.painter);
        }
        public static Point Mouse2Point(Point mouse, int squareSize, PuzzlePainter painter) {
            // Get the array index corresponding to the mouse coordinate
            return new Point((mouse.X - painter.Offset.X - painter.InnerOffset.X) / squareSize, (mouse.Y - painter.Offset.Y - painter.InnerOffset.Y) / squareSize);
        }

        private void doMouseClick(Point p, int value) {
            // Do a mouseclick at the puzzle coordinate system's p.
            if (this.puzzle.IsInRange(p)) {
                if (this.puzzle[p] == value)
                    this.puzzle[p] = Puzzle.Unknown;
                else
                    this.puzzle[p] = value;
            }
        }

        // Solve methods
        public int Check(bool strict) {
            // Return 0 if mistake found, 2 if puzzle is finished and 1 if no mistake found but not yet finished. 
            bool finished = true;
            for (int y = 0; y < this.puzzle.Height; y++)
                for (int x = 0; x < this.puzzle.Width; x++) {
                    // Mistake
                    if ((this.puzzle[x, y] > 0 && this.backUpOriginalPuzzle[x, y] <= 0))
                        return 0;
                    // Strict mistake (filled in a blank spot while it should be filled).
                    if (strict && this.puzzle[x, y] < 0 && this.backUpOriginalPuzzle[x, y] > 0)
                        return 0;
                    // Not yet finished
                    if (this.puzzle[x, y] <= 0 && this.backUpOriginalPuzzle[x, y] > 0)
                        finished = false;
                }
            return finished ? 2 : 1;
        }

        public bool Solve(bool setPuzzle) {
            // Only allow replacing the puzzle in play mode
            if (this.EditorMode)
                setPuzzle = false;

            // Check if the original puzzle is not empty (if one accidentally started designing in play mode)
            if (setPuzzle && (this.backUpOriginalPuzzle == null || this.backUpOriginalPuzzle.IsEmpty()))
                return true;

            // Solve or check for uniqueness
            if (this.EditorMode)
                return Solver.CheckUniqueness(this.puzzle);
            else
                return Solver.Solve(this.puzzle, this.backUpOriginalPuzzle);
        }
    }
}
