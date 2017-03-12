using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MattyControls;
using Picross.Model;
using Picross.Helpers;
using Picross.Solvers;

namespace Picross.UI
{
    class MainControl : MattyUserControl
    {
        PuzzleBoard puzzleBoard;
        Point mouse;
        bool mouseDown;
        string fileName;
        Btn btnEditorMode, btnNewPuzzle, btnLoad, btnSave, btnSolve, btnCheck, btnClear, btnColorBlack, btnColorEmpty, btnMove, btnSize, btnSettings;
        StatusBarElements statusBar;
        ThreadHelper threadHelper;

        public MainControl(StatusBarElements statusBarElements) {
            // Set some members
            this.puzzleBoard = new PuzzleBoard(20, 15, Settings.Get.EditorMode);
            this.mouse = new Point(-1, -1);
            this.fileName = "";

            // Manage threadhelper
            this.threadHelper = new ThreadHelper();
            this.threadHelper.OnBeforeRun += updateButtonsForThreadHelper;
            this.threadHelper.OnAfterRun += updateButtonsForThreadHelper;

            // Manage events
            this.manageEvents();

            // Add the controls
            this.statusBar = statusBarElements;
            this.addControls();
            this.clearMessage();
        }

        private void manageEvents() {
            // The paint event
            this.Paint += (o, e) => { this.puzzleBoard.Painter.Draw(e.Graphics, this.mouse, this.getSelectedColour()); };

            // The mouse events
            this.MouseClick += (o, e) => {
                if (this.moved(e.Location))
                    this.puzzleBoard.MouseClick(e.Location, this.mouseButton2Type(e.Button));
            };
            this.MouseDown += (o, e) => {
                this.mouseDown = true;
                this.mouse = new Point(-1, -1);
            };
            this.MouseUp += (o, e) => { this.mouseDown = false; };
            this.MouseMove += (o, e) => {
                if (this.moved(e.Location)) {
                    if (this.mouseDown) {
                        // Draw single squares
                        this.puzzleBoard.MouseClick(this.mouse, e.Location, this.mouseButton2Type(e.Button));
                    }
                    this.mouse = e.Location;
                    this.Draw();
                }
            };
        }

        private void updateButtonsForThreadHelper(object o, EventArgs e) {
            bool show = !this.threadHelper.Running;

            this.btnEditorMode.Enabled = show;
            this.btnNewPuzzle.Enabled = show;
            this.btnLoad.Enabled = show;
            this.btnSave.Enabled = show;
            this.btnCheck.Enabled = show;
            this.btnClear.Enabled = show;
            this.btnMove.Enabled = show;
            this.btnSize.Enabled = show;

            this.btnSolve.Text = show ? "Solve" : "Cancel";
        }

        // -- Buttons --
        private void addControls() {
            // Switch editor and play mode
            this.btnEditorMode = new Btn(Settings.Get.EditorMode ? "Mode: editor" : "Mode: play", this);
            this.btnEditorMode.Click += this.editorModeClick;

            // Create a new puzzle
            this.btnNewPuzzle = new Btn("New puzzle", this);
            this.btnNewPuzzle.Click += this.newPuzzleClick;

            // Load a puzzle
            this.btnLoad = new Btn("Load", this);
            this.btnLoad.Click += this.loadClick;

            // Save a puzzle
            this.btnSave = new Btn("Save", this);
            this.btnSave.Click += this.saveClick;

            // Solve the puzzle
            this.btnSolve = new Btn("Solve", this);
            this.btnSolve.Click += this.solveClick;

            // Check if you have no mistakes so far
            this.btnCheck = new Btn("Check", this);
            if (this.puzzleBoard.EditorMode)
                this.btnCheck.Hide();
            this.btnCheck.Click += this.checkClick;

            // Clear
            this.btnClear = new Btn("Clear", this);
            this.btnClear.Click += this.clearClick;

            // The colour buttons
            this.btnColorBlack = new Btn("", this);
            this.btnColorBlack.BackColor = this.puzzleBoard.Painter.GetColor(Field.Black);
            this.btnColorBlack.Size = new Size(this.btnColorBlack.Height, this.btnColorBlack.Height);
            this.btnColorBlack.MouseDown += this.colorBtnMouseDown;
            this.btnColorEmpty = new Btn("", this);
            this.btnColorEmpty.BackColor = this.puzzleBoard.Painter.GetColor(Field.Empty);
            this.btnColorEmpty.Size = this.btnColorBlack.Size;
            this.btnColorEmpty.MouseDown += this.colorBtnMouseDown;

            // The move button
            this.btnMove = new Btn("Move", this);
            if (!this.puzzleBoard.EditorMode)
                this.btnMove.Hide();
            this.btnMove.Click += this.moveClick;

            // The size button
            this.btnSize = new Btn("Change size", this);
            if (!this.puzzleBoard.EditorMode)
                this.btnSize.Hide();
            this.btnSize.Click += this.sizeClick;

            // The settings button
            this.btnSettings = new Btn("Settings", this);
            this.btnSettings.Click += (o, e) => { this.ShowUserControl<SettingsControl>(); };
        }

        private void editorModeClick(object o, EventArgs e) {
            bool editorMode = !this.puzzleBoard.EditorMode;
            this.puzzleBoard.EditorMode = editorMode;
            Settings.Get.EditorMode = editorMode;

            this.btnEditorMode.Text = editorMode ? "Mode: editor" : "Mode: play";
            this.btnMove.Visible = editorMode;
            this.btnSize.Visible = editorMode;
            this.btnCheck.Visible = !editorMode;

            this.OnResize();

            this.clearMessage();
        }

        private void newPuzzleClick(object o, EventArgs e) {
            this.clearMessage();

            SizeDialog dialog = new SizeDialog("Create a new picross puzzle.");
            if (dialog.ShowDialog() == DialogResult.OK) {
                this.puzzleBoard = new PuzzleBoard(dialog.ChosenSize.X, dialog.ChosenSize.Y, Settings.Get.EditorMode);
                this.OnResize();
            }
        }

        private void loadClick(object o, EventArgs e) {
            this.clearMessage();

            // Create the open file dialog box
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open puzzle";
            dialog.FileName = this.fileName;
            if (Directory.Exists(Application.StartupPath + Path.DirectorySeparatorChar + "Puzzles"))
                dialog.InitialDirectory = Application.StartupPath + Path.DirectorySeparatorChar + "Puzzles";
            else
                dialog.InitialDirectory = Application.StartupPath;
            dialog.AddExtension = true;
            dialog.DefaultExt = ".pzl";
            dialog.Filter = "Picross puzzle|*.pzl|All files|*.*";
            DialogResult dialogResult = dialog.ShowDialog();

            // Check if the user clicked OK
            if (dialogResult == DialogResult.OK) {
                this.LoadPuzzleFromFile(dialog.FileName);
                this.fileName = Path.GetFileName(dialog.FileName);
            }
        }

        private void saveClick(object o, EventArgs e) {
            this.clearMessage();

            // Create the save file dialog box
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Save puzzle";
            dialog.FileName = this.fileName;
            if (!Directory.Exists(Application.StartupPath + Path.DirectorySeparatorChar + "Puzzles"))
                Directory.CreateDirectory(Application.StartupPath + Path.DirectorySeparatorChar + "Puzzles");
            dialog.InitialDirectory = Application.StartupPath + Path.DirectorySeparatorChar + "Puzzles";
            dialog.DefaultExt = ".pzl";
            dialog.Filter = "Picross puzzle|*.pzl|Portable Network Graphics|*.png|Joint Photographic Experts Group|*.jpg|All files|*.*";
            DialogResult dialogResult = dialog.ShowDialog();

            // Check if the user clicked OK
            if (dialogResult == DialogResult.OK) {
                try {
                    string ext = Path.GetExtension(dialog.FileName);
                    if (ext.ToLower() == ".png" || ext.ToLower() == ".jpg") {
                        // Export the puzzle as image
                        Bitmap bmp = this.puzzleBoard.Painter.ToBitmap(false);
                        Bitmap bmpSolution = this.puzzleBoard.Painter.ToBitmap(true);
                        bmp.Save(dialog.FileName);
                        bmpSolution.Save(Path.Combine(Path.GetDirectoryName(dialog.FileName), "Solution" + Path.GetFileName(dialog.FileName)));
                    }
                    else {
                        // Export the puzzle as a javascript array (.pzl)
                        using (StreamWriter writer = new StreamWriter(dialog.FileName)) {
                            // Write to the just created file
                            writer.WriteLine(this.puzzleBoard.ToString());
                        }

                        this.updateTitleBar(dialog.FileName);
                    }
                    this.fileName = Path.GetFileName(dialog.FileName);
                }
                catch {
                    this.showMessage("There was an error saving the puzzle.", "Saving", StatusIcon.Error);
                }
            }
        }

        private void sizeClick(object o, EventArgs e) {
            SizeDialog dialog = new SizeDialog("Change the size of this puzzle", this.puzzleBoard.PuzzleSize);
            if (dialog.ShowDialog() == DialogResult.OK) {
                this.puzzleBoard.ChangeSize(dialog.ChosenSize);
                this.OnResize();
            }
        }

        private void moveClick(object o, EventArgs e) {
            SizeDialog dialog = new SizeDialog("Move the puzzle" + Environment.NewLine + "(use negative values to move left or up)", Point.Empty, "Move:");
            dialog.Text = "Move puzzle";
            if (dialog.ShowDialog() == DialogResult.OK) {
                this.puzzleBoard.Move(dialog.ChosenSize);
                this.Draw();
            }
        }

        private void checkClick(object o, EventArgs e) {
            this.clearMessage();

            switch (this.puzzleBoard.Solver.Check(Settings.Get.StrictChecking)) {
            case PuzzleSolver.CheckResult.Mistake:
                this.showMessage("There are one or more mistakes.", "Checking", StatusIcon.Warning);
                break;
            case PuzzleSolver.CheckResult.AllRightSoFar:
                this.showMessage("There are no mistakes.", "Checking", StatusIcon.Success);
                break;
            case PuzzleSolver.CheckResult.Finished:
                this.showMessage("You have solved the puzzle.", "Checking", StatusIcon.Success);
                break;
            }
        }

        private void clearClick(object o, EventArgs e) {
            this.puzzleBoard.Clear();
            this.Draw();
        }

        private void solveClick(object o, EventArgs e) {
            this.clearMessage();
            this.statusBar.StatusLabel.Text = "Solving...";

            if (this.threadHelper.Running) {
                this.threadHelper.Cancel();
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            this.threadHelper.Run(
                this.puzzleBoard.Solver.Solve,
                (PuzzleSolverDto result) => {
                    if (!this.puzzleBoard.EditorMode)
                        this.Draw();

                    string errorMessage = this.solveResultErrorMessage(result.SolveResult);
                    string time = this.solveTimerMessage(result.ElapsedMilliseconds);
                    if (!string.IsNullOrEmpty(errorMessage))
                        this.showMessage(errorMessage, "Solving", StatusIcon.Warning, time);

                    if (result.SolveResult == PuzzleSolver.SolveResult.Cancelled)
                        this.showStatusbarMessage("Cancelled.", "Solving", StatusIcon.None, time);

                    if (result.SolveResult == PuzzleSolver.SolveResult.UniqueOrLogicSolution) {
                        if (this.puzzleBoard.EditorMode)
                            this.showMessage("This puzzle is valid.", "Solving", StatusIcon.Success, time);
                        else
                            this.showStatusbarMessage("This puzzle is valid.", "Solving", StatusIcon.Success, time);
                    }

                    this.Cursor = Cursors.Default;
                },
                () => {
                    this.Cursor = Cursors.Default;
                }
            );
        }

        private string solveResultErrorMessage(PuzzleSolver.SolveResult result) {
            switch (result) {
            case PuzzleSolver.SolveResult.NoSolutionFound:
                return "No unique solution found.";
            case PuzzleSolver.SolveResult.NoSolutionExists:
                return "This puzzle has no solution.";
            case PuzzleSolver.SolveResult.MultipleSolutions:
                return "This puzzle has multiple solutions.";
            case PuzzleSolver.SolveResult.NoLogicSolution:
                return "This puzzle has a unique solution, but can't be solved without guessing.";
            case PuzzleSolver.SolveResult.EditorModeConflict:
                return "Editor mode conflict.";
            default:
                return null;
            }
        }

        private string solveTimerMessage(List<long> elapsedMilliseconds) {
            return $"({string.Join(", ", elapsedMilliseconds.Select(ms => $"{ms}ms"))})";
        }

        private void colorBtnMouseDown(object o, MouseEventArgs e) {
            Btn btn = (Btn)o;
            if (e.Button == MouseButtons.Left) {
                if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Field.Black))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Field.Red);
                else if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Field.Red))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Field.Black);
                else if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Field.Empty))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Field.Decoration);
                else if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Field.Decoration))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Field.Empty);
            }
            else if (e.Button == MouseButtons.Right) {
                Field type = this.puzzleBoard.Painter.GetType(btn.BackColor);

                ColorDialog dialog = new ColorDialog();
                dialog.SolidColorOnly = true;
                dialog.Color = btn.BackColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                    this.changeColor(btn, type, dialog.Color);
            }
        }

        private bool changeColor(Btn btn, Field type, Color color) {
            if (!this.puzzleBoard.Painter.SetColor(type, color)) {
                this.showMessage("This colour is already in use.", "Colour", StatusIcon.Warning);
                return false;
            }

            if (btn != null)
                btn.BackColor = color;
            Settings.Get.SetColor(type, color);
            return true;
        }

        // -- Public methods --
        public bool LoadPuzzleFromFile(string filename) {
            // Create the streamReader
            try {
                using (StreamReader reader = new StreamReader(filename)) {
                    // Load the puzzle
                    this.puzzleBoard = PuzzleBoard.FromString(reader.ReadToEnd());
                    this.puzzleBoard.EditorMode = Settings.Get.EditorMode;
                    this.OnResize();

                    this.updateTitleBar(filename);
                    return true;
                }
            }
            catch {
                this.showMessage("There was an error loading the puzzle.", "Loading", StatusIcon.Error);
                return false;
            }
        }

        public void Draw() {
            // Draw the puzzle, and check if the innerOffset is changed
            Point oldInnerOffset = this.puzzleBoard.Painter.InnerOffset;

            this.puzzleBoard.Painter.Draw(this.CreateGraphics(), this.mouse, this.getSelectedColour());

            if (oldInnerOffset != this.puzzleBoard.Painter.InnerOffset)
                this.OnResize(); // This adjusts the board size, and invalidates the window
        }

        public override void OnResize() {
            // The button locations
            this.btnEditorMode.PositionTopRightInside(this, 20);
            this.btnNewPuzzle.PositionBelow(this.btnEditorMode);
            this.btnLoad.PositionBelow(this.btnNewPuzzle);
            this.btnSave.PositionBelow(this.btnLoad);
            this.btnSolve.PositionBelow(this.btnSave);
            this.btnCheck.PositionBelow(this.btnSolve);
            this.btnMove.PositionBelow(this.btnSolve);
            this.btnSize.PositionBelow(this.btnMove);
            this.btnClear.PositionBelow(this.puzzleBoard.EditorMode ? this.btnSize : this.btnCheck);
            this.btnColorBlack.PositionBelow(this.btnClear);
            this.btnColorEmpty.PositionRightOf(this.btnColorBlack, 5);

            this.btnSettings.PositionBottomRightInside(this, 20);
            this.btnSettings.MoveDown(10);

            // The puzzle location and size
            this.puzzleBoard.Painter.Size = new Point(this.btnNewPuzzle.Location.X - 30, this.ClientSize.Height - 20);
            this.Invalidate();
        }

        public override void OnShow() {
            this.Draw();
        }

        // -- Helpers --
        private Field getSelectedColour() {
            return this.btnColorBlack.BackColor == this.puzzleBoard.Painter.GetColor(Field.Black) ? Field.Black : Field.Red;
        }

        private Field mouseButton2Type(MouseButtons buttons) {
            if (buttons == MouseButtons.Left) {
                if (this.btnColorBlack.BackColor == this.puzzleBoard.Painter.GetColor(Field.Black))
                    return Field.Black;
                else
                    return Field.Red;
            }
            if (buttons == MouseButtons.Right) {
                if (this.btnColorEmpty.BackColor == this.puzzleBoard.Painter.GetColor(Field.Empty))
                    return Field.Empty;
                else
                    return Field.Decoration;
            }
            return Field.Unknown;
        }

        private bool moved(Point newMouse) {
            return this.puzzleBoard.MouseMoved(this.mouse, newMouse) > 0;
        }

        private void updateTitleBar(string fullFileName = null) {
            if (string.IsNullOrEmpty(fullFileName))
                this.ParentMattyForm.Text = "Picross";
            else
                this.ParentMattyForm.Text = "Picross - " + Path.GetFileName(fullFileName);
        }

        private void showStatusbarMessage(string message, string title, StatusIcon icon, string secondStatusMessage = null) {
            this.statusBar.StatusLabel.Text = $"{title}: {message}";
            this.statusBar.StatusLabel.Image = icon.ToBitmap();
            if (!string.IsNullOrEmpty(secondStatusMessage))
                this.statusBar.StatusLabel.Text += $"    {secondStatusMessage}";
        }

        private void showMessage(string message, string title, StatusIcon icon, string secondStatusMessage = null) {
            this.showStatusbarMessage(message, title, icon, secondStatusMessage);

            if (!Settings.Get.OnlyStatusBar)
                MessageBox.Show(message, title, MessageBoxButtons.OK, icon.ToMessageBoxIcon());
        }

        private void clearMessage() {
            this.statusBar.StatusLabel.Text = this.puzzleBoard.EditorMode ? "Mode: Editor" : "Mode: Play";
            this.statusBar.StatusLabel.Image = null;
        }
    }
}
