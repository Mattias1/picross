using System;
using System.Drawing;
using System.Collections.Generic;

namespace Picross
{
    class Puzzle
    {
        // The types
        public const int Decoration = -2;   // An empty square with a decoration colour
        public const int Empty = -1;        // A square you are sure it's empty
        public const int Unknown = 0;       // A square you don't know anything about
        public const int Black = 1;         // A black square
        public const int Red = 2;           // A red square

        // Members
        private int[,] puzzle;
        private int[,] original;

        private bool editorMode;

        private PuzzlePainter painter;

        // Getters and setters
        public int this[int x, int y] {
            get { return this.puzzle[x, y]; }
            set { this.puzzle[x, y] = value; }
        }
        public int this[Point p] {
            get { return this[p.X, p.Y]; }
            set { this[p.X, p.Y] = value; }
        }

        public int Width {
            get { return this.puzzle.GetLength(0); }
        }
        public int Height {
            get { return this.puzzle.GetLength(1); }
        }

        public bool EditorMode {
            get { return this.editorMode; }
            set {
                this.editorMode = value;
                if (value) {
                    if (this.isEmpty(this.original))
                        this.original = this.puzzle;
                    else
                        this.puzzle = this.original;
                }
                else {
                    this.puzzle = new int[this.Width, this.Height];
                }
            }
        }

        // Methods for communication with the outside world
        public Puzzle(int w, int h, bool editormode) {
            this.original = new int[w, h];
            this.puzzle = this.original;
            this.EditorMode = editormode; // This will also set this.puzzle when in playmode.
        }

        public void SetPainterReference(PuzzlePainter painter) {
            this.painter = painter;
        }

        public void Clear() {
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++)
                    this[x, y] = Unknown;
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

        public void Move(Point move) {
            // Fill the new puzzle
            int[,] pzl = new int[this.Width, this.Height];
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++) {
                    Point to = new Point(x + move.X, y + move.Y);
                    if (IsInRange(to))
                        pzl[to.X, to.Y] = this[x, y];
                }

            // Override the old puzzle
            this.original = this.puzzle = pzl;
        }

        public void ChangeSize(Point size) {
            // Fill the new puzzle
            int[,] pzl = new int[size.X, size.Y];
            for (int y = 0; y < size.Y; y++)
                for (int x = 0; x < size.X; x++) {
                    if (IsInRange(x, y))
                        pzl[x, y] = this[x, y];
                }

            // Override the old puzzle
            this.original = this.puzzle = pzl;
        }

        public override string ToString() {
            // Save the original puzzle to string
            // Json start
            string puzzle = "[\n";
            for (int y = 0; y < this.Height; y++) {
                // Add the outer array beginning
                puzzle += "[";
                for (int x = 0; x < this.Width; x++) {
                    // Add the inner array
                    puzzle += this.original[x, y].ToString();
                    if (x != this.Width - 1)
                        puzzle += ",";
                }
                // Add the outer array ending
                puzzle += "]";
                if (y != this.Height - 1)
                    puzzle += ",";
                puzzle += "\n";
            }
            // Json end
            puzzle += "]";
            return puzzle;
        }

        public static Puzzle FromString(string puzzle) {
            // Remove whitespace, [ and ] and get the height of the puzzle on the fly
            puzzle = puzzle.Replace(" ", "").Replace("\n", "");
            int height = puzzle.Length - puzzle.Replace("],", "]").Length + 1;
            puzzle = puzzle.Replace("[", "").Replace("]", "");

            // Put everything in a 1D string array, and get the width of the puzzle on the fly
            string[] numbers = puzzle.Split(',');
            int width = numbers.Length / height;

            // Create the puzzle
            Puzzle result = new Puzzle(width, height, Settings.Get.EditorMode);
            int i = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++) {
                    int nr = int.Parse(numbers[i++]);
                    if (nr == Black || nr == Red)
                        result.original[x, y] = nr;
                }
            return result;
        }

        public bool IsInRange(Point p) {
            return this.IsInRange(p.X, p.Y);
        }
        public bool IsInRange(int x, int y) {
            return 0 <= x && x < this.Width && 0 <= y && y < this.Height;
        }

        public int MouseMoved(Point lastMouse, Point nextMouse) {
            // 0: not moved
            // 1: moved horizontally
            // 2: moved vertically
            // 3: moved both
            if (lastMouse.X == nextMouse.X && lastMouse.Y == nextMouse.Y)
                return 0;
            // Check if both mouse coordinates map to the same point, if not, the mouse moved significantly.
            int squareSize = (this.painter.Size.X - this.painter.InnerOffset.X) / this.Width;
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

        // Helper methods
        public Point Mouse2Point(Point mouse, int squareSize) {
            // Get the array index corresponding to the mouse coordinate
            return new Point((mouse.X - this.painter.Offset.X - this.painter.InnerOffset.X) / squareSize, (mouse.Y - this.painter.Offset.Y - this.painter.InnerOffset.Y) / squareSize);
        }

        public string GetRowNumbers(int y) {
            List<int> nrs = this.getRowNumberList(y);
            return string.Join<int>(" ", nrs);
        }
        public string GetColNumbers(int x) {
            List<int> nrs = this.getColNumberList(x);
            return string.Join<int>("\n", nrs);
        }

        private void doMouseClick(Point p, int value) {
            // Do a mouseclick at the puzzle coordinate system's p.
            if (this.IsInRange(p)) {
                if (this[p] == value)
                    this[p] = Unknown;
                else
                    this[p] = value;
            }
        }

        private List<int> getRowNumberList(int y) {
            // Initialize
            List<int> nrs = new List<int>();
            int counter = 0;

            // Count all numbers
            for (int x = 0; x < this.Width; x++) {
                switch (this.original[x, y]) {
                case Black:
                    counter++;
                    break;
                case Red:
                    counter++;
                    break;
                default:
                    if (counter != 0) {
                        nrs.Add(counter);
                        counter = 0;
                    }
                    break;
                }
            }
            if (counter != 0) {
                nrs.Add(counter);
                counter = 0;
            }
            return nrs;
        }
        private List<int> getColNumberList(int x) {
            // Initialize
            List<int> nrs = new List<int>();
            int counter = 0;

            // Count all numbers
            for (int y = 0; y < this.Height; y++) {
                switch (this.original[x, y]) {
                case Black:
                    counter++;
                    break;
                case Red:
                    counter++;
                    break;
                default:
                    if (counter != 0) {
                        nrs.Add(counter);
                        counter = 0;
                    }
                    break;
                }
            }
            if (counter != 0) {
                nrs.Add(counter);
                counter = 0;
            }
            return nrs;
        }

        private bool isEmpty(int[,] pzl) {
            // Check if the array is an empty array
            for (int y = 0; y < pzl.GetLength(1); y++)
                for (int x = 0; x < pzl.GetLength(0); x++)
                    if (pzl[x, y] != 0)
                        return false;
            return true;
        }

        // Solve methods
        public int Check(bool strict) {
            // Return 0 if mistake found, 2 if puzzle is finished and 1 if no mistake found but not yet finished. 
            bool finished = true;
            for (int y = 0; y < this.Height; y++)
                for (int x = 0; x < this.Width; x++) {
                    // Mistake
                    if ((this[x, y] > 0 && this.original[x, y] <= 0))
                        return 0;
                    // Strict mistake (filled in a blank spot while it should be filled).
                    if (strict && this[x, y] < 0 && this.original[x, y] > 0)
                        return 0;
                    // Not yet finished
                    if (this[x, y] <= 0 && this.original[x, y] > 0)
                        finished = false;
                }
            return finished ? 2 : 1;
        }

        public bool Solve(bool setPuzzle) {
            // Only allow replacing the puzzle in play mode
            if (this.EditorMode)
                setPuzzle = false;

            // Initialize
            List<int>[] cols = new List<int>[this.Width];
            List<int>[] rows = new List<int>[this.Height];
            for (int x = 0; x < this.Width; x++)
                cols[x] = this.getColNumberList(x);
            for (int y = 0; y < this.Height; y++)
                rows[y] = this.getRowNumberList(y);
            int[,] pzl = new int[this.Width, this.Height];

            // Check if the original puzzle is not empty (one accidentally started designing in play mode)
            bool originalIsEmpty = true;
            for (int y = 0; y < this.Height; y++)
                if (rows[y].Count > 0) {
                    originalIsEmpty = false;
                    break;
                }
            if (originalIsEmpty)
                return true;

            // Start backtracking
            int nrOfSolutions = setPuzzle ? -1 : 0;
            if (this.backTracking(pzl, cols, rows, 0, 0, ref nrOfSolutions)) {
                if (setPuzzle)
                    this.puzzle = pzl;
                return true;
            }
            return false;
        }

        private bool backTracking(int[,] pzl, List<int>[] cols, List<int>[] rows, int x, int y, ref int uniqueness) {
            // Termination criterium
            if (uniqueness > 1)
                return false;
            if (y == this.Height || y == -1) {
                if (uniqueness == -1)   // Don't check on uniqeness, so we can return.
                    return true;
                uniqueness++;
                return false;           // Don't return true right now, as we wan't to continue searching.
            }

            // Try all values
            pzl[x, y] = Black;
            if (checkXY(pzl, cols, rows, x, y))
                if (backTracking(pzl, cols, rows, nextX(x), nextY(x, y), ref uniqueness))
                    return true;
            pzl[x, y] = Empty;
            if (checkXY(pzl, cols, rows, x, y))
                if (backTracking(pzl, cols, rows, nextX(x), nextY(x, y), ref uniqueness))
                    return true;

            // None of the values worked, so erase this one and start backtracking (unless you are back at the root and have a unique solution).
            // pzl[x, y] = Unknown;
            if (x == 0 && y == 0)
                return uniqueness == 1;
            return x == 0 && y == 0 && uniqueness == 1;
        }

        private int nextX(int x) {
            return ++x == this.Width ? 0 : x;
        }
        private int nextY(int x, int y) {
            return x == this.Width - 1 ? y + 1 : y;
        }

        private bool checkXY(int[,] pzl, List<int>[] cols, List<int>[] rows, int x, int y) {
            // Vertical
            int counter = 0;
            int listCounter = 0;
            int sum = -1;
            for (int i = 0; i <= y; i++) {
                switch (pzl[x, i]) {
                case Black:
                    // Count Black pixels
                    counter++;
                    break;
                case Red:
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
            if (sum >= this.Height - y)
                return false;

            // Horizontal
            counter = 0;
            listCounter = 0;
            sum = -1;
            for (int i = 0; i <= x; i++) {
                switch (pzl[i, y]) {
                case Black:
                    // Count Black pixels
                    counter++;
                    break;
                case Red:
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
            if (sum >= this.Width - x)
                return false;
            return true;
        }
    }
}
