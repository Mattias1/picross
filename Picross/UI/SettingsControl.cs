using MattyControls;

namespace Picross.UI
{
    class SettingsControl : MattyUserControl
    {
        Btn btnOk, btnCancel;
        Cb cbUseAutoBlanker, cbStrictChecking, cbDarkerBackground, cbOnlyStatusBar;

        public SettingsControl() {
            this.addControls();
            this.addSettings();
        }

        private void addControls() {
            this.btnOk = new Btn("Ok", this);
            this.btnOk.Click += (o, e) => { this.saveAndGoBack(); };

            this.btnCancel = new Btn("Cancel", this);
            this.btnCancel.Click += (o, e) => { this.ShowLastVisitedUserControl(); };
        }

        private void addSettings() {
            this.cbUseAutoBlanker = new Cb("Use the autoblanker", this);
            this.cbStrictChecking = new Cb("Use a strict check in play mode", this);
            this.cbDarkerBackground = new Cb("Use a grey theme instead of a white theme", this);
            this.cbOnlyStatusBar = new Cb("Use only the statusbar, no popup messagese", this);
        }

        public override void OnResize() {
            this.btnCancel.PositionBottomRightInside(this);
            this.btnOk.PositionLeftOf(this.btnCancel);

            this.cbUseAutoBlanker.PositionTopLeftInside(this);
            this.cbStrictChecking.PositionBelow(this.cbUseAutoBlanker);
            this.cbDarkerBackground.PositionBelow(this.cbStrictChecking);
            this.cbOnlyStatusBar.PositionBelow(this.cbDarkerBackground);

            this.cbUseAutoBlanker.StretchRightInside(this);
            this.cbStrictChecking.StretchRightInside(this);
            this.cbDarkerBackground.StretchRightInside(this);
            this.cbOnlyStatusBar.StretchRightInside(this);
        }

        public override void OnShow() {
            this.cbUseAutoBlanker.Checked = Settings.Get.UseAutoBlanker;
            this.cbStrictChecking.Checked = Settings.Get.StrictChecking;
            this.cbDarkerBackground.Checked = Settings.Get.DarkerBackground;
            this.cbOnlyStatusBar.Checked = Settings.Get.OnlyStatusBar;
        }

        private void saveAndGoBack() {
            Settings.Get.UseAutoBlanker = this.cbUseAutoBlanker.Checked;
            Settings.Get.StrictChecking = this.cbStrictChecking.Checked;
            Settings.Get.DarkerBackground = this.cbDarkerBackground.Checked;
            Settings.Get.OnlyStatusBar = this.cbOnlyStatusBar.Checked;

            this.ShowLastVisitedUserControl();
        }
    }
}
