using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using EBookCrawler;

namespace ArtOfMagicCrawler
{
    class Program
    {
        [DllImport("Shcore.dll")]
        static extern int SetProcessDpiAwareness(int PROCESS_DPI_AWARENESS);

        // According to https://msdn.microsoft.com/en-us/library/windows/desktop/dn280512(v=vs.85).aspx
        private enum DpiAwareness
        {
            None = 0,
            SystemAware = 1,
            PerMonitorAware = 2
        }

        //[STAThread]
        static void Main(string[] args)
        {
            string root = "";
            if (args.Length > 0)
                root = args[0];
            else
            {
                var partitions = new string[]{ "E", "D", "C", "F", "G" };
                foreach (var p in partitions)
                {
                    root = p + @":\ArtOfMagicLibrary";
                    if (Directory.Exists(root))
                        break;
                }
            }

            if (!Directory.Exists(root))
            {
                Logger.LogError("Cannot find directory " + root + "!");
                Console.ReadKey();
                return;
            }

            Logger.ShowWarnings = true;
            Logger.ShowErrors = true;
            Logger.ShowInfo = true;


            Creator.UpdateMtgLibrary(root);
            RunDialog(root);
        }

        static void RunDialog(string root)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SetProcessDpiAwareness((int)DpiAwareness.PerMonitorAware);
            //(int)DpiAwareness.PerMonitorAware makes the line height of fonts higher. Why?
            //Has been fixed by changes in FontGraphicsMeasurer in Assistment.Texts

            var dialog = new LibraryImageSelectionDialog();
            var lib = ArtLibrary.ReadLibrary(root);
            dialog.SetLibrary(lib);

            Application.Run(dialog);
        }

    }
}
