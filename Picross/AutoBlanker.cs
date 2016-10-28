using System.Collections.Generic;

namespace Picross
{
    class AutoBlanker : Solver
    {
        private AutoBlanker(Puzzle puzzle, Puzzle puzzleForNumbers)
            : base(puzzle, puzzleForNumbers) { }

        public static bool[] GetRow(Puzzle puzzle, Puzzle puzzleForNumbers, int y) {
            AutoBlanker solver = new AutoBlanker(null, puzzleForNumbers);
            bool[] result = new bool[puzzle.Width];

            // If the whole row is invalid already before we start, we don't show anything.
            solver.Puzzle = puzzle.Clone();
            if (!solver.canFindValidRowConfiguration(0, y))
                return result;

            // This has some performance problems, you are (have the risk of) bruteforcing all solutions #width times, rather than once.
            // (Every time it tries to find a configuration for the same row remember).
            for (int x = 0; x < puzzle.Width; x++) {
                if (puzzle[x, y] == Puzzle.Unknown) {
                    solver.Puzzle = puzzle.Clone();

                    solver.Puzzle[x, y] = Puzzle.Black;
                    if (!solver.canFindValidRowConfiguration(0, y))
                        result[x] = true;
                }
            }

            return result;
        }

        public static bool[] GetCol(Puzzle puzzle, Puzzle puzzleForNumbers, int x) {
            AutoBlanker solver = new AutoBlanker(null, puzzleForNumbers);
            bool[] result = new bool[puzzle.Height];

            // If the whole column is invalid already before we start, we don't show anything.
            solver.Puzzle = puzzle.Clone();
            if (!solver.canFindValidColConfiguration(x, 0))
                return result;

            // This has some performance problems, you are (have the risk of) bruteforcing all solutions #height times, rather than once.
            // (Every time it tries to find a configuration for the same column remember).
            for (int y = 0; y < puzzle.Height; y++) {
                if (puzzle[x, y] == Puzzle.Unknown) {
                    solver.Puzzle = puzzle.Clone();

                    solver.Puzzle[x, y] = Puzzle.Black;
                    if (!solver.canFindValidColConfiguration(x, 0))
                        result[y] = true;
                }
            }

            return result;
        }

        private bool canFindValidRowConfiguration(int x, int y) {
            // Can I find a valid configuration of the cells for row y (using backtracking on x)
            // Termination criterium
            if (x >= this.Puzzle.Width)
                return CheckHorizontalSoFar(this.Puzzle.Width - 1, y);

            // Not allowed to modify this value
            if (this.Puzzle[x, y] != Puzzle.Unknown)
                return canFindValidRowConfiguration(x + 1, y);

            // Try all values
            this.Puzzle[x, y] = Puzzle.Black;
            if (CheckHorizontalSoFar(x, y))
                if (canFindValidRowConfiguration(x + 1, y))
                    return true;

            this.Puzzle[x, y] = Puzzle.Empty;
            if (CheckHorizontalSoFar(x, y))
                if (canFindValidRowConfiguration(x + 1, y))
                    return true;

            // None of the values worked, so start backtracking
            this.Puzzle[x, y] = Puzzle.Unknown;
            return false;
        }

        private bool canFindValidColConfiguration(int x, int y) {
            // Can I find a valid configuration of the cells for column x (using backtracking on y)
            // Termination criterium
            if (y >= this.Puzzle.Height)
                return CheckVerticalSoFar(x, this.Puzzle.Height - 1);

            // Not allowed to modify this value
            if (this.Puzzle[x, y] != Puzzle.Unknown)
                return canFindValidColConfiguration(x, y + 1);

            // Try all values
            this.Puzzle[x, y] = Puzzle.Black;
            if (CheckVerticalSoFar(x, y))
                if (canFindValidColConfiguration(x, y + 1))
                    return true;

            this.Puzzle[x, y] = Puzzle.Empty;
            if (CheckVerticalSoFar(x, y))
                if (canFindValidColConfiguration(x, y + 1))
                    return true;

            // None of the values worked, so start backtracking
            this.Puzzle[x, y] = Puzzle.Unknown;
            return false;
        }
    }
}
