using System;
using System.Windows.Forms;

namespace Picross
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the mainlication.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            // Load the settigns
            Settings.Get.Load();

            // Pass the arguments on to the application
            Main main = new Main();

            // Load puzzle from args
            if (args.Length > 0) {
                main.LoadPuzzleFromFile(args[0]);
            }

            // Run the main application
            Application.EnableVisualStyles();
            Application.Run(main);

            // Save the settings
            Settings.Get.Save();
        }
    }
}
