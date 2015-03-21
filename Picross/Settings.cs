using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Picross
{
    class Settings
    {
        private static string path = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "Picross.ini";

        // Singleton code
        /// <summary>
        /// The settings instance
        /// </summary>
        private static Settings instance;
        /// <summary>
        /// The instance of the settings singleton
        /// </summary>
        public static Settings Get {
            get { return Settings.instance == null ? Settings.instance = new Settings() : Settings.instance; }
        }

        // The list with all settings
        /// <summary>
        /// The list with all settings. All keys are lowercase.
        /// </summary>
        private Dictionary<string, string> hashList;
        /// <summary>
        /// The way to comminucate with the hashList
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns>The value of the key</returns>
        private string this[string key] {
            get {
                if (this.hashList.ContainsKey(key))
                    return this.hashList[key];
                else
                    return this.defaults[key];
            }
            set { this.hashList[key] = value; }
        }
        /// <summary>
        /// The dictinaty with all default values
        /// </summary>
        private Dictionary<string, string> defaults {
            get {
                Dictionary<string, string> list = new Dictionary<string, string>();
                list.Add("position", Settings.Pt2Str(new Point(25, 25)));
                list.Add("size", Settings.Pt2Str(new Point(888, 600)));
                list.Add("editormode", true.ToString());
                list.Add("strictchecking", false.ToString());
                list.Add("darkerbackground", false.ToString());
                list.Add("color-2", Settings.Color2Str(Color.FromArgb(255, 128, 0)));
                list.Add("color-1", Settings.Color2Str(Color.FromArgb(255, 255, 128)));
                list.Add("color0", Settings.Color2Str(Color.FromArgb(255, 255, 255)));
                list.Add("color1", Settings.Color2Str(Color.FromArgb(128, 64, 0)));
                list.Add("color2", Settings.Color2Str(Color.FromArgb(255, 0, 0)));
                return list;
            }
        }

        // Settings properties
        /// <summary>
        /// The position of the window
        /// </summary>
        public Point Position {
            get { return Settings.Str2Pt(this["position"]); }
            set { this["position"] = Settings.Pt2Str(value); }
        }
        /// <summary>
        /// The screen resolution
        /// </summary>
        public Point Size {
            get { return Settings.Str2Pt(this["size"]); }
            set { this["size"] = Settings.Pt2Str(value); }
        }
        /// <summary>
        /// Whether or not picross is in editor mode
        /// </summary>
        public bool EditorMode {
            get { return bool.Parse(this["editormode"]); }
            set { this["editormode"] = value.ToString(); }
        }
        /// <summary>
        /// Whether or strict checking is enabled
        /// </summary>
        public bool StrictChecking {
            get { return bool.Parse(this["strictchecking"]); }
            set { this["strictchecking"] = value.ToString(); }
        }
        /// <summary>
        /// Whether or not darker background is enabled
        /// </summary>
        public bool DarkerBackground {
            get { return bool.Parse(this["darkerbackground"]); }
            set { this["darkerbackground"] = value.ToString(); }
        }

        /// <summary>
        /// Set a colour
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Color GetColor(int type) {
            return Str2Color(this["color" + type.ToString()]);
        }
        /// <summary>
        /// Get a colour
        /// </summary>
        /// <param name="type"></param>
        /// <param name="color"></param>
        public void SetColor(int type, Color color) {
            this["color" + type.ToString()] = Color2Str(color);
        }

        // Private settings methods
        private Settings() {
            this.hashList = new Dictionary<string, string>();
            this.SetDefaults();
        }

        // Public settings methods
        /// <summary>
        /// Load the settings from file
        /// </summary>
        /// <returns>Whether there was no error loading</returns>
        public bool Load() {
            // If the file doesnt exists, load the defaults
            try {
                if (!File.Exists(Settings.path))
                    return false;
                using (StreamReader file = new StreamReader(Settings.path)) {
                    // Read all the tuples
                    string line;
                    while ((line = file.ReadLine()) != null) {
                        string[] keyVal = line.Split(':');
                        this[keyVal[0]] = keyVal[1];
                    }
                }
            }
            catch {
                MessageBox.Show("Error loading the settings", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Save the settings and scores to file
        /// </summary>
        /// <returns>Whether there was no error saving</returns>
        public bool Save() {
            try {
                using (StreamWriter file = new StreamWriter(Settings.path)) {
                    // Write all the tuples
                    foreach (var tuple in this.hashList) {
                        file.WriteLine(tuple.Key + ":" + tuple.Value);
                    }
                }
            }
            catch {
                MessageBox.Show("Error saving the settings", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults() {
            // Clone the values from the defaults in the hashList.
            this.hashList = new Dictionary<string, string>(this.defaults);
        }

        /// <summary>
        /// Parse a point to a string
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string Pt2Str(Point p, char seperator = ',') {
            return p.X.ToString() + seperator + p.Y.ToString();
        }

        /// <summary>
        /// Parse a string to a vector2
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static Point Str2Pt(string s, char seperator = ',') {
            string[] ps = s.Split(seperator);
            return new Point(int.Parse(ps[0]), int.Parse(ps[1]));
        }

        /// <summary>
        /// Parse a color to a string
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string Color2Str(Color c, char seperator = ',') {
            return c.R.ToString() + seperator + c.G.ToString() + seperator + c.B.ToString();
        }

        /// <summary>
        /// Parse a string to a color
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static Color Str2Color(string s, char seperator = ',') {
            string[] cs = s.Split(seperator);
            return Color.FromArgb(int.Parse(cs[0]), int.Parse(cs[1]), int.Parse(cs[2]));
        }
    }
}
