using System.Drawing;
using System.Windows.Forms;
using MattyControls;

namespace Picross.UI
{
    class SizeDialog : Form
    {
        Lbl lblDescription;
        Btn btnOk, btnCancel;
        Tb tbWidth, tbHeight;

        public Point ChosenSize {
            get { return new Point(int.Parse(this.tbWidth.Text), int.Parse(this.tbHeight.Text)); }
        }
        public string Description { get; private set; }

        public SizeDialog(string description, Point? defaultSize = null, string labelDescription = "Size:") {
            // Some settings
            this.Description = description;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ClientSize = new Size(220, 120);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.Text = "Puzzle size";

            // Add the controls
            this.addControls(defaultSize ?? new Point(20, 15));
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;

            // Locate the controls
            this.positionControls(labelDescription);
        }

        private void addControls(Point defaultSize) {
            // The description label
            this.lblDescription = new Lbl(this.Description, this);
            this.lblDescription.Size = new Size(this.Width - 20, this.lblDescription.Height + 1);

            // The size textboxes
            this.tbWidth = new Tb(this);
            this.tbWidth.Text = defaultSize.X.ToString();
            this.tbWidth.Size = new Size(this.tbWidth.Width / 2, this.tbWidth.Height);
            this.tbHeight = new Tb(this);
            this.tbHeight.Text = defaultSize.Y.ToString();
            this.tbHeight.Size = this.tbWidth.Size;

            // The Ok and Cancel buttons
            this.btnOk = new Btn("Ok", this);
            this.btnOk.Click += (o, e) => {
                int number;
                if (!int.TryParse(this.tbWidth.Text, out number) || !int.TryParse(this.tbHeight.Text, out number)) {
                    MessageBox.Show("Please enter an integer value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.btnCancel = new Btn("Cancel", this);
            this.btnCancel.Click += (o, e) => {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
        }

        private void positionControls(string labelDescription) {
            this.lblDescription.PositionTopLeftInside(this);
            this.tbWidth.PositionBelow(this.lblDescription);
            this.tbWidth.AddLabel(labelDescription, true, 50, 0);
            this.tbHeight.PositionRightOf(tbWidth);
            this.tbHeight.AddLabel(",", false, 10, 0);
            this.btnCancel.PositionBottomRightInside(this);
            this.btnOk.PositionLeftOf(this.btnCancel);
        }
    }
}
