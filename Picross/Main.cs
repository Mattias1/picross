using System;
using System.Drawing;
using System.Windows.Forms;
using MattyControls;

namespace Picross
{
    class Main : MattyForm
    {
        private static Size MinSize = new Size(665, 435);

        public Main() : base(MinSize, Settings.Get) {
            // Load and apply the settings
            Settings s = Settings.Get;
            this.Icon = Properties.Resources.EyesHybrid;
            this.MaximizeBox = false;

            // Set some standard values
            this.Text = "Picross";

            // Add the controls
            this.AddUserControl(new MainControl());
            this.ShowUserControl<MainControl>();
        }

        public void LoadPuzzleFromFile(string filename) {
            this.GetUserControl<MainControl>().LoadPuzzleFromFile(filename);
        }
    }
}
