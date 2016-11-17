using System.Collections.Generic;

namespace Picross
{
    class AutoBlanker : Solver
    {
        private AutoBlanker(Puzzle puzzle, Puzzle puzzleForNumbers)
            : base(puzzle, puzzleForNumbers) { }

        public static bool[] GetRow(Puzzle puzzle, Puzzle puzzleForNumbers, int y) {
            AutoBlanker solver = new AutoBlanker(null, puzzleForNumbers);

            return solver.GetRow_Mirror(puzzle, puzzleForNumbers, y, solver.Rows[y], false);
        }

        public static bool[] GetCol(Puzzle puzzle, Puzzle puzzleForNumbers, int x) {
            AutoBlanker solver = new AutoBlanker(null, puzzleForNumbers);

            return solver.GetRow_Mirror(puzzle, puzzleForNumbers, x, solver.Cols[x], true);
        }

        private bool[] GetRow_Mirror(Puzzle puzzle, Puzzle puzzleForNumbers, int y, List<int> row, bool mirror) {
            bool[] result = new bool[puzzle.GetWidth(mirror)];

            // If the whole row is invalid already before we start, we don't show anything.
            this.Puzzle = puzzle.Clone();
            if (!this.canFindValidRowConfiguration_Mirror(0, y, row, mirror))
                return result;

            // This has some performance problems, you are (have the risk of) bruteforcing all solutions #width times, rather than once.
            // (Every time it tries to find a configuration for the same row remember).
            for (int x = 0; x < puzzle.GetWidth(mirror); x++) {
                if (puzzle[x, y, mirror] == Field.Unknown) {
                    this.Puzzle = puzzle.Clone();

                    this.Puzzle[x, y, mirror] = Field.Black;
                    if (!this.canFindValidRowConfiguration_Mirror(0, y, row, mirror))
                        result[x] = true;
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
    }
}
