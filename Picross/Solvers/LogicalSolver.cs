using System;
using System.Collections.Generic;

namespace Picross.Solvers
{
    class LogicalSolver : Solver
    {
        protected LogicalSolver(Puzzle puzzle, Puzzle puzzleForNumbers)
            : base(puzzle, puzzleForNumbers) { }

        new public static bool Solve(Puzzle puzzle, Puzzle puzzleForNumbers) {
            var solver = new LogicalSolver(puzzle, puzzleForNumbers);

            return solver.solveLogically(puzzle);
        }

        private bool solveLogically(Puzzle puzzle) {
            // The following code acts like a state machine. Each method represents a state. We hop from state to state using recursion.

            // Loop row
            loopRows(puzzle);
            // Loop col
            for (int x = 0; x < this.Puzzle.Width; x++) {
                // Todo
            }

            return false;
        }


        // State machine mehods
        private void loopRows(Puzzle puzzle) {
            for (int y = 0; y < this.Puzzle.Height; y++) {
                FoundFields resultBlack = GetRow(puzzle, Field.Black, Field.Empty, y);
                if (resultBlack.FoundChange) {
                    FoundFields resultWhite = GetRow(puzzle, Field.Empty, Field.Black, y);
                    FoundFields merged = resultBlack.Merge(resultWhite);
                    // Todo
                }
            }
        }

        private void loopCols() {
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
                    if (!this.canFindValidRowConfiguration_Mirror(0, y, row, mirror))
                        result[x] = true;

                    if (searchField.HasValue)
                        this.Puzzle[x, y, mirror] = searchField.Value;
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


        public class FoundFields
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

            public FoundFields Merge(FoundFields other) {
                if (this.Length != other.Length)
                    throw new ArgumentException("Lengths must be equal to merge found fields.");

                var result = new FoundFields(this.Length);
                if (this.FoundChange || other.FoundChange) {
                    for (int i = 0; i < other.Length; i++)
                        result[i] = this[i] || other[i];
                }

                return result;
            }
        }
    }
}
