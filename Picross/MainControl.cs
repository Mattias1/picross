using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using MattyControls;

namespace Picross
{
    class MainControl : MattyUserControl
    {
        PuzzleBoard puzzleBoard;
        Point mouse;
        bool mouseDown;
        string fileName;
        Btn btnEditorMode, btnNewPuzzle, btnLoad, btnSave, btnSolve, btnCheck, btnClear, btnColorBlack, btnColorEmpty, btnMove, btnSize;
        Cb cbStrictChecking, cbDarkerBackground;

        public MainControl() {
            // Set some members
            this.puzzleBoard = new PuzzleBoard(20, 15, Settings.Get.EditorMode);
            this.mouse = new Point(-1, -1);
            this.fileName = "";

            // Manage events
            this.manageEvents();

            // Add the controls
            this.addControls();
        }

        private void manageEvents() {
            // The paint event
            this.Paint += (o, e) => { this.puzzleBoard.Painter.Draw(e.Graphics, this.mouse); };

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
            this.btnColorBlack.BackColor = this.puzzleBoard.Painter.GetColor(Puzzle.Black);
            this.btnColorBlack.Size = new Size(this.btnColorBlack.Height, this.btnColorBlack.Height);
            this.btnColorBlack.MouseDown += this.colorBtnMouseDown;
            this.btnColorEmpty = new Btn("", this);
            this.btnColorEmpty.BackColor = this.puzzleBoard.Painter.GetColor(Puzzle.Empty);
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

            // The strictness checkbox
            this.cbStrictChecking = new Cb("Strict check", this);
            this.cbStrictChecking.Size = new Size(this.btnSize.Width + 10, this.cbStrictChecking.Height);
            this.cbStrictChecking.CheckedChanged += (o, e) => { Settings.Get.StrictChecking = this.cbStrictChecking.Checked; };
            if (this.puzzleBoard.EditorMode)
                this.cbStrictChecking.Hide();
            this.cbStrictChecking.Checked = Settings.Get.StrictChecking;

            // The background colour checkbox
            this.cbDarkerBackground = new Cb("Grey theme", this);
            this.cbDarkerBackground.Size = this.cbStrictChecking.Size;
            this.cbDarkerBackground.CheckedChanged += (o, e) => {
                Settings.Get.DarkerBackground = this.cbDarkerBackground.Checked;
                this.Draw();
            };
            this.cbDarkerBackground.Checked = Settings.Get.DarkerBackground;
        }

        private void editorModeClick(object o, EventArgs e) {
            this.puzzleBoard.EditorMode = !this.puzzleBoard.EditorMode;
            Settings.Get.EditorMode = this.puzzleBoard.EditorMode;
            if (this.puzzleBoard.EditorMode) {
                this.btnEditorMode.Text = "Mode: editor";
                this.btnMove.Show();
                this.btnSize.Show();
                this.btnCheck.Hide();
                this.cbStrictChecking.Hide();
            }
            else {
                this.btnEditorMode.Text = "Mode: play";
                this.btnMove.Hide();
                this.btnSize.Hide();
                this.btnCheck.Show();
                this.cbStrictChecking.Show();
            }
            this.OnResize();
        }

        private void newPuzzleClick(object o, EventArgs e) {
            SizeDialog dialog = new SizeDialog("Create a new picross puzzle.");
            if (dialog.ShowDialog() == DialogResult.OK) {
                this.puzzleBoard = new PuzzleBoard(dialog.ChosenSize.X, dialog.ChosenSize.Y, Settings.Get.EditorMode);
                this.OnResize();
            }
        }

        private void loadClick(object o, EventArgs e) {
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
                    MessageBox.Show("There was an error saving the puzzle.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            switch (this.puzzleBoard.Check(Settings.Get.StrictChecking)) {
            case 0:
                MessageBox.Show("You have one or more mistakes.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                break;
            case 1:
                MessageBox.Show("You have no mistakes.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            case 2:
                MessageBox.Show("You have solved the puzzle.", "Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            }
        }

        private void clearClick(object o, EventArgs e) {
            this.puzzleBoard.Clear();
            this.Draw();
        }

        private void solveClick(object o, EventArgs e) {
            this.Cursor = Cursors.WaitCursor;
            if (!this.puzzleBoard.Solve(!this.puzzleBoard.EditorMode))
                MessageBox.Show("This puzzle has no unique solution.", "Solve", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            else if (this.puzzleBoard.EditorMode)
                MessageBox.Show("This puzzle is valid.", "Solve", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (!this.puzzleBoard.EditorMode)
                this.Draw();
            this.Cursor = Cursors.Default;
        }

        private void colorBtnMouseDown(object o, MouseEventArgs e) {
            Btn btn = (Btn)o;
            if (e.Button == MouseButtons.Left) {
                if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Puzzle.Black))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Puzzle.Red);
                else if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Puzzle.Red))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Puzzle.Black);
                else if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Puzzle.Empty))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Puzzle.Decoration);
                else if (btn.BackColor == this.puzzleBoard.Painter.GetColor(Puzzle.Decoration))
                    btn.BackColor = this.puzzleBoard.Painter.GetColor(Puzzle.Empty);
            }
            else if (e.Button == MouseButtons.Right) {
                int type = 0;
                for (int i = -2; i < 3; i++)
                    if (btn.BackColor == this.puzzleBoard.Painter.GetColor(i)) {
                        type = i;
                        break;
                    }
                ColorDialog dialog = new ColorDialog();
                dialog.SolidColorOnly = true;
                dialog.Color = btn.BackColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                    this.changeColor(btn, type, dialog.Color);
            }
        }

        private bool changeColor(Btn btn, int type, Color color) {
            if (!this.puzzleBoard.Painter.SetColor(type, color)) {
                MessageBox.Show("This colour is already in use.", "Colour", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("There was an error loading the puzzle.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void Draw() {
            // Draw the puzzle, and check if the innerOffset is changed
            int selectedColour = this.btnColorBlack.BackColor == this.puzzleBoard.Painter.GetColor(Puzzle.Black) ? Puzzle.Black : Puzzle.Red;
            Point innerOffset = this.puzzleBoard.Painter.InnerOffset;

            this.puzzleBoard.Painter.Draw(this.CreateGraphics(), this.mouse, selectedColour);

            if (innerOffset != this.puzzleBoard.Painter.InnerOffset)
                this.OnResize();
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

            this.cbDarkerBackground.PositionBottomRightInside(this);
            this.cbStrictChecking.PositionAbove(cbDarkerBackground);

            // The puzzle location and size
            this.puzzleBoard.Painter.Size = new Point(this.btnNewPuzzle.Location.X - 30, this.ClientSize.Height - 20);
            this.Invalidate();
        }

        // -- Helpers --
        private int mouseButton2Type(MouseButtons buttons) {
            if (buttons == MouseButtons.Left) {
                if (this.btnColorBlack.BackColor == this.puzzleBoard.Painter.GetColor(Puzzle.Black))
                    return Puzzle.Black;
                else
                    return Puzzle.Red;
            }
            if (buttons == MouseButtons.Right) {
                if (this.btnColorEmpty.BackColor == this.puzzleBoard.Painter.GetColor(Puzzle.Empty))
                    return Puzzle.Empty;
                else
                    return Puzzle.Decoration;
            }
            return Puzzle.Unknown;
        }

        private bool moved(Point newMouse) {
            return this.puzzleBoard.MouseMoved(this.mouse, newMouse) > 0;
        }

        private void updateTitleBar(string fullFileName = null) {
            if (string.IsNullOrEmpty(fullFileName))
                ((Main)this.Parent).Text = "Picross";
            else
                ((Main)this.Parent).Text = "Picross - " + Path.GetFileName(fullFileName);
        }
    }
}
