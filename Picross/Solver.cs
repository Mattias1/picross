using System.Collections.Generic;

namespace Picross
{
    class Solver
    {
        protected Puzzle Puzzle;
        protected readonly List<int>[] Rows, Cols;

        protected Solver(Puzzle puzzle, Puzzle puzzleForNumbers) {
            this.Puzzle = puzzle;
            puzzleForNumbers.ComputeRowAndColNumbers(out this.Rows, out this.Cols);
        }

        public static bool Solve(Puzzle puzzle, Puzzle puzzleForNumbers) {
            Solver solver = new Solver(puzzle, puzzleForNumbers);

            int nrOfSolutions = -1;
            return solver.backTracking(0, 0, ref nrOfSolutions);
        }

        public static bool CheckUniqueness(Puzzle puzzle) {
            Solver solver = new Solver(puzzle.Clone(), puzzle);

            int nrOfSolutions = 0;
            return solver.backTracking(0, 0, ref nrOfSolutions);
        }

        private bool backTracking(int x, int y, ref int uniqueness) {
            // Termination criterium
            if (uniqueness > 1)
                return false;
            if (y == this.Puzzle.Height || y == -1) {
                if (uniqueness == -1)   // Don't check on uniqeness, so we can return.
                    return true;
                uniqueness++;
                return false;           // Don't return true right now, as we wan't to continue searching.
            }

            // Try all values
            this.Puzzle[x, y] = Field.Black;
            if (CheckXYSoFar(x, y))
                if (backTracking(nextX(x), nextY(x, y), ref uniqueness))
                    return true;
            this.Puzzle[x, y] = Field.Empty;
            if (CheckXYSoFar(x, y))
                if (backTracking(nextX(x), nextY(x, y), ref uniqueness))
                    return true;

            // None of the values worked, so start backtracking (unless you are back at the root and have a unique solution).
            if (x == 0 && y == 0)
                return uniqueness == 1;
            return false;
        }

        private int nextX(int x) {
            return ++x == this.Puzzle.Width ? 0 : x;
        }
        private int nextY(int x, int y) {
            return x == this.Puzzle.Width - 1 ? y + 1 : y;
        }

        protected bool CheckXYSoFar(int x, int y) {
            // Check whether or not the puzzle up untill (x, y) is valid, and can be made valid for the fields after (x, y).
            return CheckHorizontalSoFar(x, y) && CheckVerticalSoFar(x, y);
        }

        protected bool CheckHorizontalSoFar(int x, int y) {
            return CheckHorizontalSoFar_Mirror(x, y, this.Rows[y], false);
        }
        protected bool CheckVerticalSoFar(int x, int y) {
            return CheckHorizontalSoFar_Mirror(y, x, this.Cols[x], true);
        }

        protected bool CheckHorizontalSoFar_Mirror(int x, int y, List<int> row, bool mirror) {
            int groupSize = 0; // The number of black boxes next to each other (so far)
            int listIndex = 0; // The number of black-box-groups we have had so far

            // Check if the completed groups (untill x) are valid
            for (int i = 0; i <= x; i++) {
                // Count Black pixels (Red pixels count as Black for now)
                if (this.Puzzle[i, y, mirror].IsOn()) {
                    groupSize++;
                }
                // Check off the black pixels we've had so far
                else if (groupSize != 0) {
                    if (listIndex >= row.Count || groupSize != row[listIndex])
                        return false;
                    listIndex++;
                    groupSize = 0;
                }
            }

            // If there is an incomplete group left, check if it's (possible to make it) valid
            if (groupSize != 0) {
                if (listIndex >= row.Count || groupSize > row[listIndex])
                    return false;
            }

            // Check if we have enough left to harbor the next pixels
            int boxesStillNecessary = -groupSize - 1;
            for (int i = listIndex; i < row.Count; i++) {
                boxesStillNecessary += row[i] + 1;
            }

            if (boxesStillNecessary >= this.Puzzle.GetWidth(mirror) - x) {
                return false;
            }

            return true;
        }
    }
}
