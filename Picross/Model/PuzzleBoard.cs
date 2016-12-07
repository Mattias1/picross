using System.Drawing;
using Picross.Solvers;
using Picross.UI;
using Picross.Helpers;

namespace Picross.Model
{
    class PuzzleBoard
    {
        public enum CheckResult { Mistake, AllRightSoFar, Finished };
        public enum SolveResult { EditorModeConflict, NoSolution, MultipleSolutions, NoLogicSolution, UniqueOrLogicSolution };

        // Members
        private Puzzle puzzleObject;
        private Puzzle backUpOriginalPuzzle;

        private bool editorMode;

        private PuzzlePainter painter;

        // Getters and setters
        private Puzzle puzzle {
            get { return this.puzzleObject; }
            set {
                this.puzzleObject = value;
                if (this.painter == null)
                    this.painter = new PuzzlePainter(value, this.backUpOriginalPuzzle);
                else
                    this.painter.SetPuzzleObjects(value, this.backUpOriginalPuzzle);
            }
        }
        public bool EditorMode {
            get { return this.editorMode; }
            set {
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

        public void MouseClick(Point mouse, Field value) {
            Point p = this.Mouse2Point(mouse, this.painter.CalculateSquareSize());
            doMouseClick(p, value);
        }
        public void MouseClick(Point oldMouse, Point newMouse, Field value) {
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

        public override string ToString() {
            return (this.backUpOriginalPuzzle ?? this.puzzle).ToString();
        }

        // Helper methods
        public Point Mouse2Point(Point mouse, int squareSize) {
            // Get the array index corresponding to the mouse coordinate
            return Mouse2Point(mouse, squareSize, this.painter);
        }
        public static Point Mouse2Point(Point mouse, int squareSize, PuzzlePainter painter) {
            // Get the array index corresponding to the mouse coordinate
            int numeratorX = mouse.X - painter.Offset.X - painter.InnerOffset.X;
            int numeratorY = mouse.Y - painter.Offset.Y - painter.InnerOffset.Y;
            if (numeratorX < 0)
                numeratorX -= squareSize;
            if (numeratorY < 0)
                numeratorY -= squareSize;
            return new Point(numeratorX / squareSize, numeratorY / squareSize);
        }

        private void doMouseClick(Point p, Field value) {
            // Set the puzzle value at point p
            if (this.puzzle.IsInRange(p)) {
                if (this.puzzle[p] == value)
                    this.puzzle[p] = Field.Unknown;
                else
                    this.puzzle[p] = value;
            }

            // Or autoblank all columns
            else if (Settings.Get.UseAutoBlanker && !this.EditorMode) {
                if (this.puzzle.IsInRangeX(p.X)) {
                    bool[] autoblanks = AutoBlanker.GetCol(this.puzzle, this.backUpOriginalPuzzle, p.X);
                    for (int y = 0; y < autoblanks.Length; y++)
                        if (autoblanks[y])
                            this.puzzle[p.X, y] = Field.Empty;
                }
                else if (this.puzzle.IsInRangeY(p.Y)) {
                    bool[] autoblanks = AutoBlanker.GetRow(this.puzzle, this.backUpOriginalPuzzle, p.Y);
                    for (int x = 0; x < autoblanks.Length; x++)
                        if (autoblanks[x])
                            this.puzzle[x, p.Y] = Field.Empty;
                }
            }
        }

        // Solve methods
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

        public SolveResult Solve() {
            // Check if the original puzzle is not empty (if one accidentally started designing in play mode)
            if (!this.EditorMode && (this.backUpOriginalPuzzle == null || this.backUpOriginalPuzzle.IsEmpty())) {
                return SolveResult.EditorModeConflict;
            }

            // Solve or check for uniqueness
            bool result;
            if (this.EditorMode) {
                Puzzle solvePuzzle = this.puzzle.EmptyClone();
                if (Settings.Get.Solver.IsOneOf(Settings.SolverSetting.Smart, Settings.SolverSetting.OnlyLogic)) {
                    result = LogicalSolver.Solve(solvePuzzle, this.puzzle);
                    if (result)
                        return SolveResult.UniqueOrLogicSolution;
                }

                if (Settings.Get.Solver.IsOneOf(Settings.SolverSetting.Smart, Settings.SolverSetting.OnlyBacktracking)) {
                    var backtrackResult = BacktrackSolver.CheckUniqueness(solvePuzzle);
                    if (backtrackResult == SolveResult.UniqueOrLogicSolution && Settings.Get.Solver == Settings.SolverSetting.Smart)
                        return SolveResult.NoLogicSolution;

                    return backtrackResult;
                }
            }
            else {
                if (Settings.Get.Solver.IsOneOf(Settings.SolverSetting.Smart, Settings.SolverSetting.OnlyLogic)) {
                    result = LogicalSolver.Solve(this.puzzle, this.backUpOriginalPuzzle);
                    if (result)
                        return SolveResult.UniqueOrLogicSolution;
                }
                if (Settings.Get.Solver == Settings.SolverSetting.OnlyBacktracking) {
                    result = BacktrackSolver.Solve(this.puzzle, this.backUpOriginalPuzzle);
                    if (result)
                        return SolveResult.UniqueOrLogicSolution;
                }
            }

            return SolveResult.NoSolution;
        }
    }
}
