using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Picross
{
    class MainControl : SeaUserControl
    {
        Puzzle puzzle;
        PuzzlePainter puzzlePainter;
        Point mouse;
        bool mouseDown;
        string fileName;
        Btn btnEditorMode, btnNewPuzzle, btnLoad, btnSave, btnSolve, btnCheck, btnClear, btnColorBlack, btnColorEmpty, btnMove, btnSize;
        Cb cbStrictChecking, cbDarkerBackground;

        public MainControl() {
            // Set some members
            this.setNewPuzzleObject(new Puzzle(20, 15, Settings.Get.EditorMode));
            this.mouse = new Point(-1, -1);
            this.fileName = "";

            // Manage events
            this.manageEvents();

            // Add the controls
            this.addControls();
        }

        private void setNewPuzzleObject(Puzzle puzzle) {
            this.puzzle = puzzle;
            this.puzzlePainter = new PuzzlePainter(this.puzzle);
            this.puzzle.SetPainterReference(this.puzzlePainter);
        }

        private void manageEvents() {
            // The paint event
            this.Paint += (o, e) => { this.puzzlePainter.Draw(e.Graphics, this.mouse); };

            // The mouse events
            this.MouseClick += (o, e) => {
                if (this.moved(e.Location) > 0)
                    this.puzzle.MouseClick(e.Location, this.mouseButton2Type(e.Button));
            };
            this.MouseDown += (o, e) => {
                this.mouseDown = true;
                this.mouse = new Point(-1, -1);
            };
            this.MouseUp += (o, e) => { this.mouseDown = false; };
            this.MouseMove += (o, e) => {
                int moveChange = this.moved(e.Location);
                if (moveChange > 0) {
                    if (this.mouseDown) {
                        // Draw single squares
                        this.puzzle.MouseClick(this.mouse, e.Location, this.mouseButton2Type(e.Button));
                    }
                    this.mouse = e.Location;
                    this.Draw();
                }
            };
        }

        private void addControls() {
            // Switch editor and play mode
            this.btnEditorMode = new Btn(Settings.Get.EditorMode ? "Mode: editor" : "Mode: play", this);
            this.btnEditorMode.Click += (o, e) => {
                this.puzzle.EditorMode = !this.puzzle.EditorMode;
                Settings.Get.EditorMode = this.puzzle.EditorMode;
                if (this.puzzle.EditorMode) {
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
            };

            // Create a new puzzle
            this.btnNewPuzzle = new Btn("New puzzle", this);
            this.btnNewPuzzle.Click += (o, e) => {
                SizeDialog dialog = new SizeDialog("Create a new picross puzzle.");
                if (dialog.ShowDialog() == DialogResult.OK) {
                    this.setNewPuzzleObject(new Puzzle(dialog.ChosenSize.X, dialog.ChosenSize.Y, Settings.Get.EditorMode));
                    this.OnResize();
                }
            };

            // Load a puzzle
            this.btnLoad = new Btn("Load", this);
            this.btnLoad.Click += (o, e) => {
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
            };

            // Save a puzzle
            this.btnSave = new Btn("Save", this);
            this.btnSave.Click += (o, e) => {
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
                            Bitmap bmp = this.puzzlePainter.ToBitmap(false);
                            Bitmap bmpSolution = this.puzzlePainter.ToBitmap(true);
                            bmp.Save(dialog.FileName);
                            bmpSolution.Save(Path.Combine(Path.GetDirectoryName(dialog.FileName), "Solution" + Path.GetFileName(dialog.FileName)));
                        }
                        else {
                            // Export the puzzle as a javascript array (.pzl)
                            using (StreamWriter writer = new StreamWriter(dialog.FileName)) {
                                // Write to the just created file
                                writer.WriteLine(this.puzzle.ToString());
                            }
                        }
                        this.fileName = Path.GetFileName(dialog.FileName);
                    }
                    catch {
                        MessageBox.Show("There was an error saving the puzzle.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            // Solve the puzzle
            this.btnSolve = new Btn("Solve", this);
            this.btnSolve.Click += (o, e) => {
                this.Cursor = Cursors.WaitCursor;
                if (!this.puzzle.Solve(!this.puzzle.EditorMode))
                    MessageBox.Show("This puzzle has no unique solution.", "Solve", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                else if (this.puzzle.EditorMode)
                    MessageBox.Show("This puzzle is valid.", "Solve", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (!this.puzzle.EditorMode)
                    this.Draw();
                this.Cursor = Cursors.Default;
            };

            // Check if you have no mistakes so far
            this.btnCheck = new Btn("Check", this);
            if (this.puzzle.EditorMode)
                this.btnCheck.Hide();
            this.btnCheck.Click += (o, e) => {
                switch (this.puzzle.Check(Settings.Get.StrictChecking)) {
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
            };

            // Clear
            this.btnClear = new Btn("Clear", this);
            this.btnClear.Click += (o, e) => {
                this.puzzle.Clear();
                this.Draw();
            };

            // The colour buttons
            this.btnColorBlack = new Btn("", this);
            this.btnColorBlack.BackColor = this.puzzlePainter.GetColor(Puzzle.Black);
            this.btnColorBlack.Size = new Size(this.btnColorBlack.Height, this.btnColorBlack.Height);
            this.btnColorBlack.MouseDown += this.colorBtnMouseDown;
            this.btnColorEmpty = new Btn("", this);
            this.btnColorEmpty.BackColor = this.puzzlePainter.GetColor(Puzzle.Empty);
            this.btnColorEmpty.Size = this.btnColorBlack.Size;
            this.btnColorEmpty.MouseDown += this.colorBtnMouseDown;

            // The move button
            this.btnMove = new Btn("Move", this);
            if (!this.puzzle.EditorMode)
                this.btnMove.Hide();
            this.btnMove.Click += (o, e) => {
                SizeDialog dialog = new SizeDialog("Move the puzzle" + Environment.NewLine + "(use negative values to move left or up)", 0, 0, "Move:");
                dialog.Text = "Move puzzle";
                if (dialog.ShowDialog() == DialogResult.OK) {
                    this.puzzle.Move(dialog.ChosenSize);
                    this.Draw();
                }
            };

            // The size button
            this.btnSize = new Btn("Change size", this);
            if (!this.puzzle.EditorMode)
                this.btnSize.Hide();
            this.btnSize.Click += (o, e) => {
                SizeDialog dialog = new SizeDialog("Change the size of this puzzle", this.puzzle.Width, this.puzzle.Height);
                if (dialog.ShowDialog() == DialogResult.OK) {
                    this.puzzle.ChangeSize(dialog.ChosenSize);
                    this.OnResize();
                }
            };

            // The strictness checkbox
            this.cbStrictChecking = new Cb("Strict check", this);
            this.cbStrictChecking.Size = new Size(this.btnSize.Width + 10, this.cbStrictChecking.Height);
            this.cbStrictChecking.CheckedChanged += (o, e) => { Settings.Get.StrictChecking = this.cbStrictChecking.Checked; };
            if (this.puzzle.EditorMode)
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

        private void colorBtnMouseDown(object o, MouseEventArgs e) {
            Btn btn = (Btn)o;
            if (e.Button == MouseButtons.Left) {
                if (btn.BackColor == this.puzzlePainter.GetColor(Puzzle.Black))
                    btn.BackColor = this.puzzlePainter.GetColor(Puzzle.Red);
                else if (btn.BackColor == this.puzzlePainter.GetColor(Puzzle.Red))
                    btn.BackColor = this.puzzlePainter.GetColor(Puzzle.Black);
                else if (btn.BackColor == this.puzzlePainter.GetColor(Puzzle.Empty))
                    btn.BackColor = this.puzzlePainter.GetColor(Puzzle.Decoration);
                else if (btn.BackColor == this.puzzlePainter.GetColor(Puzzle.Decoration))
                    btn.BackColor = this.puzzlePainter.GetColor(Puzzle.Empty);
            }
            else if (e.Button == MouseButtons.Right) {
                int type = 0;
                for (int i = -2; i < 3; i++)
                    if (btn.BackColor == this.puzzlePainter.GetColor(i)) {
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
            if (!this.puzzlePainter.SetColor(type, color)) {
                MessageBox.Show("This colour is already in use.", "Colour", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (btn != null)
                btn.BackColor = color;
            Settings.Get.SetColor(type, color);
            return true;
        }

        public bool LoadPuzzleFromFile(string filename) {
            // Create the streamReader
            try {
                using (StreamReader reader = new StreamReader(filename)) {
                    // Load the puzzle
                    this.setNewPuzzleObject(Puzzle.FromString(reader.ReadToEnd()));
                    this.puzzle.EditorMode = Settings.Get.EditorMode;
                    this.OnResize();
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
            int selectedColour = this.btnColorBlack.BackColor == this.puzzlePainter.GetColor(Puzzle.Black) ? Puzzle.Black : Puzzle.Red;
            Point innerOffset = this.puzzlePainter.InnerOffset;
            this.puzzlePainter.Draw(this.CreateGraphics(), this.mouse, selectedColour);
            if (innerOffset != this.puzzlePainter.InnerOffset)
                this.OnResize();
        }

        public override void OnResize() {
            // The button locations
            this.btnEditorMode.LocateInside(this, Btn.Horizontal.Right, Btn.Vertical.Top, 20);
            this.btnNewPuzzle.LocateFrom(this.btnEditorMode, Btn.Horizontal.CopyRight, Btn.Vertical.Bottom);
            this.btnLoad.LocateFrom(this.btnNewPuzzle, Btn.Horizontal.CopyRight, Btn.Vertical.Bottom);
            this.btnSave.LocateFrom(this.btnLoad, Btn.Horizontal.CopyRight, Btn.Vertical.Bottom);
            this.btnSolve.LocateFrom(this.btnSave, Btn.Horizontal.CopyRight, Btn.Vertical.Bottom);
            this.btnCheck.LocateFrom(this.btnSolve, Btn.Horizontal.CopyRight, Btn.Vertical.Bottom);
            this.btnMove.LocateFrom(this.btnSolve, Btn.Horizontal.CopyLeft, Btn.Vertical.Bottom);
            this.btnSize.LocateFrom(this.btnMove, Btn.Horizontal.CopyLeft, Btn.Vertical.Bottom);
            this.btnClear.LocateFrom(this.puzzle.EditorMode ? this.btnSize : this.btnCheck, Btn.Horizontal.CopyRight, Btn.Vertical.Bottom);
            this.btnColorBlack.LocateFrom(this.btnClear, Btn.Horizontal.CopyLeft, Btn.Vertical.Bottom);
            this.btnColorEmpty.LocateFrom(this.btnColorBlack, Btn.Horizontal.Right, Btn.Vertical.CopyTop, 5);

            this.cbDarkerBackground.LocateInside(this, Btn.Horizontal.Right, Btn.Vertical.Bottom, 10);
            this.cbStrictChecking.LocateFrom(this.cbDarkerBackground, Btn.Horizontal.CopyLeft, Btn.Vertical.Top);

            // The puzzle location and size
            this.puzzlePainter.Size = new Point(this.btnNewPuzzle.Location.X - 30, this.ClientSize.Height - 20);
            this.Invalidate();
        }

        private int mouseButton2Type(MouseButtons buttons) {
            if (buttons == MouseButtons.Left) {
                if (this.btnColorBlack.BackColor == this.puzzlePainter.GetColor(Puzzle.Black))
                    return Puzzle.Black;
                else
                    return Puzzle.Red;
            }
            if (buttons == MouseButtons.Right) {
                if (this.btnColorEmpty.BackColor == this.puzzlePainter.GetColor(Puzzle.Empty))
                    return Puzzle.Empty;
                else
                    return Puzzle.Decoration;
            }
            return Puzzle.Unknown;
        }

        private int moved(Point newMouse) {
            return this.puzzle.MouseMoved(this.mouse, newMouse);
        }
    }
}
