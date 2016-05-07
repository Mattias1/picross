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

            // Initialize
            List<int>[] rows, cols;
            (this.backUpOriginalPuzzle ?? this.puzzle).ComputeRowAndColNumbers(out rows, out cols);
            Puzzle pzl = this.puzzle.Clone();

            // Check if the original puzzle is not empty (one accidentally started designing in play mode)
            if (rows.All(r => r.Count == 0))
                return true;

            // Start backtracking
            int nrOfSolutions = setPuzzle ? -1 : 0;
            if (this.backTracking(pzl, rows, cols, 0, 0, ref nrOfSolutions)) {
                if (setPuzzle)
                    this.puzzle = pzl;
                return true;
            }
            return false;
        }

        private bool backTracking(Puzzle pzl, List<int>[] rows, List<int>[] cols, int x, int y, ref int uniqueness) {
            // Termination criterium
            if (uniqueness > 1)
                return false;
            if (y == pzl.Height || y == -1) {
                if (uniqueness == -1)   // Don't check on uniqeness, so we can return.
                    return true;
                uniqueness++;
                return false;           // Don't return true right now, as we wan't to continue searching.
            }

            // Try all values
            pzl[x, y] = Puzzle.Black;
            if (checkXYSoFar(pzl, rows, cols, x, y))
                if (backTracking(pzl, rows, cols, nextX(x), nextY(x, y), ref uniqueness))
                    return true;
            pzl[x, y] = Puzzle.Empty;
            if (checkXYSoFar(pzl, rows, cols, x, y))
                if (backTracking(pzl, rows, cols, nextX(x), nextY(x, y), ref uniqueness))
                    return true;

            // None of the values worked, so start backtracking (unless you are back at the root and have a unique solution).
            if (x == 0 && y == 0)
                return uniqueness == 1;
            return false;
        }

        private int nextX(int x) {
            return ++x == this.puzzle.Width ? 0 : x;
        }
        private int nextY(int x, int y) {
            return x == this.puzzle.Width - 1 ? y + 1 : y;
        }

        private bool checkXYSoFar(Puzzle pzl, List<int>[] rows, List<int>[] cols, int x, int y) {
            // Vertical
            int counter = 0;
            int listCounter = 0;
            int sum = -1;
            for (int i = 0; i <= y; i++) {
                switch (pzl[x, i]) {
                case Puzzle.Black:
                    // Count Black pixels
                    counter++;
                    break;
                case Puzzle.Red:
                    throw new NotImplementedException();
                default:
                    // Check off the black pixels we've had so far
                    if (counter != 0) {
                        if (cols[x][listCounter] != counter)
                            return false;
                        listCounter++;
                        counter = 0;
                    }
                    break;
                }
            }
            if (counter != 0) {
                if (listCounter >= cols[x].Count)
                    return false;
                if (cols[x][listCounter] < counter)
                    return false;
                sum -= counter;
            }
            // Check if we have enough left to harbor the next pixels
            for (int i = listCounter; i < cols[x].Count; i++)
                sum += cols[x][i] + 1;
            if (sum >= this.puzzle.Height - y)
                return false;

            // Horizontal
            counter = 0;
            listCounter = 0;
            sum = -1;
            for (int i = 0; i <= x; i++) {
                switch (pzl[i, y]) {
                case Puzzle.Black:
                    // Count Black pixels
                    counter++;
                    break;
                case Puzzle.Red:
                    throw new NotImplementedException();
                default:
                    // Check off the black pixels we've had so far
                    if (counter != 0) {
                        if (rows[y][listCounter] != counter)
                            return false;
                        listCounter++;
                        counter = 0;
                    }
                    break;
                }
            }
            if (counter != 0) {
                if (listCounter >= rows[y].Count)
                    return false;
                if (rows[y][listCounter] < counter)
                    return false;
                sum -= counter;
            }
            // Check if we have enough left to harbor the next pixels
            for (int i = listCounter; i < rows[y].Count; i++)
                sum += rows[y][i] + 1;
            if (sum >= this.puzzle.Width - x)
                return false;
            return true;
        }

        // Auto blank methods
        public bool[] GetAutoBlanksCol(int x) {
            bool[] result = new bool[this.puzzle.Height];

            for (int y = 0; y < this.puzzle.Height; y++) {
                if (this.puzzle[x, y] == Puzzle.Unknown) {
                    this.puzzle[x, y] = Puzzle.Black;
                    if (this.canFindValidColConfiguration(x))
                        result[y] = true;
                    this.puzzle[x, y] = Puzzle.Unknown;
                }
            }

            return result;
        }

        private bool canFindValidColConfiguration(int x) {
            return false;
        }
    }
}
