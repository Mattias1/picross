using Picross.Model;

namespace Picross.Solvers
{
    class BacktrackSolver : SolverBase
    {
        private BacktrackSolver(Puzzle puzzle, Puzzle puzzleForNumbers)
            : base(puzzle, puzzleForNumbers) { }

        public static PuzzleBoard.SolveResult Solve(Puzzle puzzle, Puzzle puzzleForNumbers) {
            var solver = new BacktrackSolver(puzzle, puzzleForNumbers);

            int nrOfSolutions = -1;
            return solver.backTracking(0, 0, ref nrOfSolutions);
        }

        public static PuzzleBoard.SolveResult CheckUniqueness(Puzzle puzzle) {
            BacktrackSolver solver = new BacktrackSolver(puzzle.Clone(), puzzle);

            int nrOfSolutions = 0;
            return solver.backTracking(0, 0, ref nrOfSolutions);
        }

        private PuzzleBoard.SolveResult backTracking(int x, int y, ref int uniqueness) {
            // Termination criterium
            if (uniqueness > 1)
                return PuzzleBoard.SolveResult.MultipleSolutions;
            if (y == this.Puzzle.Height || y == -1) {
                if (uniqueness == -1)   // Don't check on uniqeness, so we can return.
                    return PuzzleBoard.SolveResult.UniqueOrLogicSolution;
                uniqueness++;
                return PuzzleBoard.SolveResult.NoSolutionFound; // Don't return true right now, as we want to continue searching.
            }

            // Try all values
            this.Puzzle[x, y] = Field.Black;
            if (CheckXYSoFar(x, y)) {
                var result = backTracking(nextX(x), nextY(x, y), ref uniqueness);
                if (result == PuzzleBoard.SolveResult.UniqueOrLogicSolution)
                    return result;
            }
            this.Puzzle[x, y] = Field.Empty;
            if (CheckXYSoFar(x, y)) {
                var result = backTracking(nextX(x), nextY(x, y), ref uniqueness);
                if (result == PuzzleBoard.SolveResult.UniqueOrLogicSolution)
                    return result;
            }

            // None of the values worked, so start backtracking (unless you are back at the root and have a unique solution).
            if (x == 0 && y == 0 && uniqueness == 1)
                return PuzzleBoard.SolveResult.UniqueOrLogicSolution;
            return PuzzleBoard.SolveResult.NoSolutionFound;
        }

        private int nextX(int x) {
            return ++x == this.Puzzle.Width ? 0 : x;
        }
        private int nextY(int x, int y) {
            return x == this.Puzzle.Width - 1 ? y + 1 : y;
        }

        private bool CheckXYSoFar(int x, int y) {
            // Check whether or not the puzzle up untill (x, y) is valid, and can be made valid for the fields after (x, y).
            return CheckHorizontalSoFar(x, y) && CheckVerticalSoFar(x, y);
        }
    }
}
