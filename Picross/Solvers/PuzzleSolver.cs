using Picross.Helpers;
using Picross.Model;

namespace Picross.Solvers
{
    class PuzzleSolver
    {
        public enum CheckResult
        {
            Mistake, AllRightSoFar, Finished
        };
        public enum SolveResult
        {
            Cancelled, EditorModeConflict,
            NoSolutionFound, NoSolutionExists,
            MultipleSolutions, NoLogicSolution,
            UniqueOrLogicSolution
        };

        private PuzzleBoard board;

        private Puzzle puzzle => board.Puzzle;
        private Puzzle backUpOriginalPuzzle => board.BackUpOriginalPuzzle;

        public PuzzleSolver(PuzzleBoard board) {
            this.board = board;
        }

        public CheckResult Check(bool strict) {
            bool finished = true;
            for (int y = 0; y < this.puzzle.Height; y++) {
                for (int x = 0; x < this.puzzle.Width; x++) {
                    // Mistake
                    if ((this.puzzle[x, y].IsOn() && this.backUpOriginalPuzzle[x, y] != this.puzzle[x, y]))
                        return CheckResult.Mistake;

                    // Strict mistake (filled in a blank spot while it should be filled).
                    if (strict && this.puzzle[x, y].IsOff() && this.backUpOriginalPuzzle[x, y].IsOn())
                        return CheckResult.Mistake;

                    // Not yet finished
                    if (this.puzzle[x, y].IsNotOn() && this.backUpOriginalPuzzle[x, y].IsOn())
                        finished = false;
                }
            }

            return finished ? CheckResult.Finished : CheckResult.AllRightSoFar;
        }

        public SolveResult Solve(ThreadHelper threadHelper) {
            // Check if the original puzzle is not empty (if one accidentally started designing in play mode)
            if (!this.board.EditorMode && (this.backUpOriginalPuzzle == null || this.backUpOriginalPuzzle.IsEmpty())) {
                return SolveResult.EditorModeConflict;
            }

            // Solve or check for uniqueness
            if (this.board.EditorMode)
                return this.solveEditorMode(threadHelper);
            return this.solvePlayMode(threadHelper);
        }

        private SolveResult solveEditorMode(ThreadHelper threadHelper) {
            Puzzle solvePuzzle = this.puzzle.EmptyClone();
            if (Settings.Get.Solver.IsOneOf(Settings.SolverSetting.Smart, Settings.SolverSetting.OnlyLogic)) {
                var logicResult = LogicalSolver.Solve(solvePuzzle, this.puzzle, threadHelper);
                if (logicResult == SolveResult.UniqueOrLogicSolution)
                    return logicResult;
            }

            if (Settings.Get.Solver.IsOneOf(Settings.SolverSetting.Smart, Settings.SolverSetting.OnlyBacktracking)) {
                var backtrackResult = Settings.Get.Solver == Settings.SolverSetting.Smart
                    ? BacktrackSolver.Solve(solvePuzzle, this.puzzle, threadHelper)
                    : BacktrackSolver.CheckUniqueness(solvePuzzle, threadHelper);

                if (backtrackResult == SolveResult.UniqueOrLogicSolution && Settings.Get.Solver == Settings.SolverSetting.Smart)
                    return SolveResult.NoLogicSolution;

                return this.adjustNoSolutionResult(backtrackResult);
            }

            return this.adjustNoSolutionResult(SolveResult.NoSolutionFound);
        }

        private SolveResult adjustNoSolutionResult(SolveResult result) {
            if (Settings.Get.Solver == Settings.SolverSetting.Smart && result == SolveResult.NoSolutionFound)
                return SolveResult.NoSolutionExists;
            return result;
        }

        private SolveResult solvePlayMode(ThreadHelper threadHelper) {
            if (Settings.Get.Solver.IsOneOf(Settings.SolverSetting.Smart, Settings.SolverSetting.OnlyLogic)) {
                return LogicalSolver.Solve(this.puzzle, this.backUpOriginalPuzzle, threadHelper);
            }

            if (Settings.Get.Solver == Settings.SolverSetting.OnlyBacktracking) {
                return BacktrackSolver.Solve(this.puzzle, this.backUpOriginalPuzzle, threadHelper);
            }

            return SolveResult.NoSolutionFound;
        }
    }
}
