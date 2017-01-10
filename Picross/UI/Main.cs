using System.Drawing;
using System.Windows.Forms;
using MattyControls;

namespace Picross.UI
{
    class Main : MattyForm
    {
        private static Size MinSize = new Size(665, 435);
        private static Size StartSize = new Size(888, 600);

        public Main() : base(MinSize, StartSize, Settings.Get) {
            // Load and apply the settings
            this.Icon = Properties.Resources.EyesHybrid;
            this.MaximizeBox = false;

            // Set some standard values
            this.Text = "Picross";

            // The status bar
            this.UseStatusStrip();
            var statusBarElements = new StatusBarElements(this.StatusStrip);

            // Add the controls
            this.AddUserControl(new MainControl(statusBarElements), new SettingsControl());
            this.ShowUserControl<MainControl>();
        }

        public void LoadPuzzleFromFile(string filename) {
            this.GetUserControl<MainControl>().LoadPuzzleFromFile(filename);
        }
    }

    class StatusBarElements
    {
        public MattyStatusStrip StatusStrip { get; private set; }

        public ToolStripStatusLabel StatusLabel { get; set; }

        public StatusBarElements(MattyStatusStrip statusStrip) {
            this.StatusStrip = statusStrip;
            this.StatusLabel = statusStrip.AddLabel(ToolStripItemDisplayStyle.Text);
        }
    }
}
