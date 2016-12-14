using System;
using System.Collections.Generic;
using Picross.Helpers;
using Picross.Model;

namespace Picross.Solvers
{
    class LogicalSolver : SolverBase
    {
        protected LogicalSolver(Puzzle puzzle, Puzzle puzzleForNumbers, ThreadHelper threadHelper)
            : base(puzzle, puzzleForNumbers, threadHelper) { }

        public static PuzzleSolver.SolveResult Solve(Puzzle puzzle, Puzzle puzzleForNumbers, ThreadHelper threadHelper) {
            var solver = new LogicalSolver(puzzle, puzzleForNumbers, threadHelper);

            bool result = solver.solveLogically(puzzle, puzzleForNumbers);

            if (threadHelper.Cancelling)
                return PuzzleSolver.SolveResult.Cancelled;

            return result ? PuzzleSolver.SolveResult.UniqueOrLogicSolution : PuzzleSolver.SolveResult.NoSolutionFound;
        }

        private bool solveLogically(Puzzle puzzle, Puzzle puzzleForNumbers) {
            loopAll(puzzle);

            if (this.ThreadHelper.Cancelling)
                return false;

            return isSolved(puzzle, puzzleForNumbers);
        }

        private bool isSolved(Puzzle puzzle, Puzzle original) {
            for (int y = 0; y < puzzle.Height; y++) {
                for (int x = 0; x < puzzle.Width; x++) {
                    if (puzzle[x, y].IsOn() != original[x, y].IsOn())
                        return false;
                }
            }
            return true;
        }


        // The following code acts like a state machine.
        // Each method represents a state. The return value represents whether or not it found a change.
        // We hop from state to state using recursion.
        private bool loopAll(Puzzle puzzle) {
            if (this.ThreadHelper.Cancelling) return false;

            if (loopRows(puzzle, Field.Black, Field.Empty)) return true;
            if (loopCols(puzzle, Field.Black, Field.Empty)) return true;

            if (loopRows(puzzle, Field.Empty, Field.Black)) return true;
            if (loopCols(puzzle, Field.Empty, Field.Black)) return true;

            return false;
        }

        private bool loopRows(Puzzle puzzle, Field search, Field opposite) {
            return loopRows_Mirror(puzzle, search, opposite, this.Rows, false);
        }
        private bool loopCols(Puzzle puzzle, Field search, Field opposite) {
            return loopRows_Mirror(puzzle, search, opposite, this.Cols, true);
        }

        private bool loopRows_Mirror(Puzzle puzzle, Field search, Field opposite, List<int>[] rows, bool mirror) {
            if (this.ThreadHelper.Cancelling)
                return false;

            for (int y = 0; y < this.Puzzle.GetHeight(mirror); y++) {
                FoundFields resultBlack = GetRow_Mirror(puzzle, search, opposite, y, rows[y], mirror);
                if (resultBlack.FoundChange) {
                    FoundFields resultWhite = GetRow_Mirror(puzzle, opposite, search, y, rows[y], mirror);
                    FoundFields merged = FoundFields.Merge(resultBlack, resultWhite);
                    return loopAll(puzzle);
                }
            }
            return false;
        }


        // Helper methods
        protected FoundFields GetRow(Puzzle puzzle, Field? searchField, Field oppositeField, int y) {
            return this.GetRow_Mirror(puzzle, searchField, oppositeField, y, this.Rows[y], false);
        }
        protected FoundFields GetCol(Puzzle puzzle, Field? searchField, Field oppositeField, int x) {
            return this.GetRow_Mirror(puzzle, searchField, oppositeField, x, this.Cols[x], true);
        }

        protected FoundFields GetRow_Mirror(Puzzle puzzle, Field? searchField, Field oppositeField, int y, List<int> row, bool mirror) {
            var result = new FoundFields(puzzle.GetWidth(mirror));

            // If the whole row is invalid already before we start, we don't show anything.
            this.Puzzle = puzzle.Clone();
            if (!this.canFindValidRowConfiguration_Mirror(0, y, row, mirror))
                return result;

            // This has some performance problems, you are (have the risk of) bruteforcing all solutions #width times, rather than once.
            // (Every time it tries to find a configuration for the same row remember).
            for (int x = 0; x < puzzle.GetWidth(mirror); x++) {
                if (puzzle[x, y, mirror] == Field.Unknown) {
                    this.Puzzle = puzzle.Clone();

                    this.Puzzle[x, y, mirror] = oppositeField;
                    if (!this.canFindValidRowConfiguration_Mirror(0, y, row, mirror)) {
                        result[x] = true;

                        if (searchField.HasValue)
                            puzzle[x, y, mirror] = searchField.Value;
                    }
                }
            }

            return result;
        }

        private bool canFindValidRowConfiguration_Mirror(int x, int y, List<int> row, bool mirror) {
            // Can I find a valid configuration of the cells for row y (using backtracking on x)
            // Termination criterium
            if (x >= this.Puzzle.GetWidth(mirror))
                return CheckHorizontalSoFar_Mirror(this.Puzzle.GetWidth(mirror) - 1, y, row, mirror);

            // Not allowed to modify this value
            if (this.Puzzle[x, y, mirror] != Field.Unknown)
                return canFindValidRowConfiguration_Mirror(x + 1, y, row, mirror);

            // Try all values
            this.Puzzle[x, y, mirror] = Field.Black;
            if (CheckHorizontalSoFar_Mirror(x, y, row, mirror))
                if (canFindValidRowConfiguration_Mirror(x + 1, y, row, mirror))
                    return true;

            this.Puzzle[x, y, mirror] = Field.Empty;
            if (CheckHorizontalSoFar_Mirror(x, y, row, mirror))
                if (canFindValidRowConfiguration_Mirror(x + 1, y, row, mirror))
                    return true;

            // None of the values worked, so start backtracking
            this.Puzzle[x, y, mirror] = Field.Unknown;
            return false;
        }


        protected class FoundFields
        {
            public bool FoundChange;
            public bool[] Result;

            public int Length => Result.Length;

            public bool this[int index] {
                get { return this.Result[index]; }
                set {
                    this.Result[index] = value;
                    this.FoundChange |= value;
                }
            }

            public FoundFields(int length) {
                this.Result = new bool[length];
            }

            public static FoundFields Merge(FoundFields one, FoundFields other) {
                if (one.Length != other.Length)
                    throw new ArgumentException("Lengths must be equal to merge found fields.");

                var result = new FoundFields(one.Length);
                if (one.FoundChange || other.FoundChange) {
                    for (int i = 0; i < other.Length; i++)
                        result[i] = one[i] || other[i];
                }

                return result;
            }
        }
    }
}
