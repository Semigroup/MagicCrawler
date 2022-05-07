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

        [STAThread]
        static void Main(string[] args)
        {
            string root = @"E:\ArtOfMagicLibrary";

            //DownloadList(root);
            //DownloadLibrary(root);
            //DownloadArt(root, false);
            //CreateThumbnails(root, false);
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
        static void CreateThumbnails(string root, bool forceRecreation)
        {
            ArtLibrary lib = ArtLibrary.ReadLibrary(root);
            var thumbnailDir = Path.Combine(root, "thumbnails");
            int i = 0;
            foreach (var art in lib.ArtObjects)
            {
                var path = art.AbsoluteThumbnailPath;
                var dir = path.Substring(0, path.LastIndexOf("\\"));
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                int h = LibraryImageSelectionDialog.ThumbnailHeight;
                int w = (int)(art.Width * h * 1.0 / art.Height);

                if (!File.Exists(path) || forceRecreation)
                    using (Image thumbnail = new Bitmap(w, h))
                    using (Graphics g = Graphics.FromImage(thumbnail))
                    using (Image original = Image.FromFile(art.AbsoluteImagePath))
                    {
                        g.DrawImage(original, 0, 0, w, h);
                        thumbnail.Save(path, ImageFormat.Jpeg);
                    }
                else
                    Logger.LogWarning("Thumbnail already exists. Not recreated!");

                Console.WriteLine(path);
                Console.WriteLine("Created thumbnail " + (i++) + " of " + lib.ArtObjects.Length);
            }
        }
        static void DownloadArt(string root, bool forceReload)
        {
            ArtLibrary lib = ArtLibrary.ReadLibrary(root);

            ArtDownloader artDownloader = new ArtDownloader();
            artDownloader.DownloadArt(root, lib, forceReload);

            ArtLibrary.WriteLibrary(root, lib);
        }
      
      

        static void DownloadLibrary(string root)
        {
            string listPath = Path.Combine(root, "art-pages.list");
            ArtDownloader artDownloader = new ArtDownloader();
            List<ArtObject> art = new List<ArtObject>();

            foreach (var page in File.ReadLines(listPath))
            {
                if (page.Length == 0)
                    continue;
                var result = artDownloader.GetArtObject(page);

                if (result != null)
                    art.Add(result);
            }
            ArtLibrary lib = new ArtLibrary()
            {
                ArtObjects = art.ToArray()
            };
            ArtLibrary.WriteLibrary(root, lib);

            void checkString(string text)
            {
                if (text == null)
                    return;
                for (int i = 0; i < text.Length; i++)
                {
                    if ('a' <= text[i] && text[i] <= 'z')
                        continue;
                    if ('A' <= text[i] && text[i] <= 'Z')
                        continue;
                    if ('0' <= text[i] && text[i] <= '9')
                        continue;
                    if (text[i] == ' ')
                        continue;
                    if (text[i] == ',')
                        continue;
                    if (text[i] == '.')
                        continue;
                    if (text[i] == '!')
                        continue;
                    if (text[i] == ':')
                        continue;
                    if (text[i] == '-')
                        continue;
                    if (text[i] == '&')
                        continue;
                    if (text[i] == '|')
                        continue;
                    if (text[i] == '/')
                        continue;
                    if (text[i] == '\'')
                        continue;
                    if (text[i] == '(')
                        continue;
                    if (text[i] == ')')
                        continue;
                    Console.WriteLine(text);
                }
            }
        }

        static void DownloadList(string root)
        {
            string target = "https://www.artofmtg.com/art/";

            string filePath = Path.Combine(root, "art-pages.list");
            Spider spider = new Spider();

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            if (!File.Exists(filePath))
            {
                var fs = File.Create(filePath);
                fs.Close();
            }

            var pages = new List<string>();
            foreach (var page in spider.CollectPages("https://www.artofmtg.com/", page => page.StartsWith(target)))
                pages.Add(page);

            pages.Sort();

            using (var writer = new StreamWriter(filePath, false))
                foreach (var page in pages)
                    writer.WriteLine(page);

            Console.WriteLine("found " + pages.Count + " art pages");
        }
    }
}
