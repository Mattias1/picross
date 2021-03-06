﻿using System.Drawing;
using MattyControls;
using Picross.Model;

namespace Picross
{
    class Settings : SettingsSingleton
    {
        public enum SolverSetting { Smart, OnlyBacktracking, OnlyLogic };

        protected override string Name => "Picross";

        /// <summary>
        /// The instance of the settings singleton
        /// </summary>
        public static Settings Get => SettingsSingleton.GetSingleton<Settings>();

        // Settings properties
        /// <summary>
        /// Whether or not picross is in editor mode
        /// </summary>
        public bool EditorMode {
            get { return this.get("editormode", true); }
            set { this.set("editormode", value); }
        }

        /// <summary>
        /// Whether or strict checking is enabled
        /// </summary>
        public bool StrictChecking {
            get { return this.get("strictchecking", false); }
            set { this.set("strictchecking", value); }
        }

        /// <summary>
        /// Whether or not darker background is enabled
        /// </summary>
        public bool DarkerBackground {
            get { return this.get("darkerbackground", false); }
            set { this.set("darkerbackground", value); }
        }

        /// <summary>
        /// Whether or not to use the autoblanker
        /// </summary>
        public bool UseAutoBlanker {
            get { return this.get("useautoblanker", false); }
            set { this.set("useautoblanker", value); }
        }

        /// <summary>
        /// Whether or not to use only the status bar, or a status bar and messageboxes
        /// </summary>
        public bool OnlyStatusBar {
            get { return this.get("onlystatusbar", false); }
            set { this.set("onlystatusbar", value); }
        }

        /// <summary>
        /// Set a colour
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Color GetColor(Field type) {
            return this.get("color" + type.ToString(), defaultColor(type));
        }
        private Color defaultColor(Field type) {
            var colors = new Color[]
            {
                Color.FromArgb(255, 128, 0),
                Color.FromArgb(255, 255, 128),
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(128, 64, 0),
                Color.FromArgb(255, 0, 0)
            };
            return colors[type.Index];
        }
        /// <summary>
        /// Get a colour
        /// </summary>
        /// <param name="type"></param>
        /// <param name="color"></param>
        public void SetColor(Field type, Color color) {
            this.set("color" + type.ToString(), color);
        }

        /// <summary>
        /// Which algorithms to use for solving and uniqueness checks
        /// </summary>
        public SolverSetting Solver {
            get { return this.getEnum("solver", SolverSetting.Smart); }
            set { this.setEnum("solver", value); }
        }
    }
}
