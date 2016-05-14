using System;
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
            this.Puzzle[x, y] = Puzzle.Black;
            if (CheckXYSoFar(x, y))
                if (backTracking(nextX(x), nextY(x, y), ref uniqueness))
                    return true;
            this.Puzzle[x, y] = Puzzle.Empty;
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
            return CheckHorizontalSoFar(x, y) && CheckVerticalSoFar(x, y);
        }
        protected bool CheckHorizontalSoFar(int x, int y) {
            List<int> row = this.Rows[y];
            int counter = 0;
            int listCounter = 0;
            int sum = -1;
            for (int i = 0; i <= x; i++) {
                switch (this.Puzzle[i, y]) {
                case Puzzle.Black:
                    // Count Black pixels
                    counter++;
                    break;
                case Puzzle.Red:
                    throw new NotImplementedException();
                default:
                    // Check off the black pixels we've had so far
                    if (counter != 0) {
                        if (listCounter >= row.Count || row[listCounter] != counter)
                            return false;
                        listCounter++;
                        counter = 0;
                    }
                    break;
                }
            }
            if (counter != 0) {
                if (listCounter >= row.Count)
                    return false;
                if (row[listCounter] < counter)
                    return false;
                sum -= counter;
            }
            // Check if we have enough left to harbor the next pixels
            for (int i = listCounter; i < row.Count; i++)
                sum += row[i] + 1;
            if (sum >= this.Puzzle.Width - x)
                return false;
            return true;
        }
        protected bool CheckVerticalSoFar(int x, int y) {
            List<int> col = this.Cols[x];
            int counter = 0;
            int listCounter = 0;
            int sum = -1;
            for (int i = 0; i <= y; i++) {
                switch (this.Puzzle[x, i]) {
                case Puzzle.Black:
                    // Count Black pixels
                    counter++;
                    break;
                case Puzzle.Red:
                    throw new NotImplementedException();
                default:
                    // Check off the black pixels we've had so far
                    if (counter != 0) {
                        if (listCounter >= col.Count || col[listCounter] != counter)
                            return false;
                        listCounter++;
                        counter = 0;
                    }
                    break;
                }
            }
            if (counter != 0) {
                if (listCounter >= col.Count)
                    return false;
                if (col[listCounter] < counter)
                    return false;
                sum -= counter;
            }
            // Check if we have enough left to harbor the next pixels
            for (int i = listCounter; i < col.Count; i++)
                sum += col[i] + 1;
            if (sum >= this.Puzzle.Height - y)
                return false;
            return true;
        }
    }
}
