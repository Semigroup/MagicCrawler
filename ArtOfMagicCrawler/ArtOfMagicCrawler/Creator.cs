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
    public static class Creator
    {
        public static void UpdateMtgLibrary(string root)
        {
            Logger.LogInfo("Updating Mtg Library");
            Logger.LogInfo("Downloading List (1/5)");
            DownloadList(root);
            Logger.LogInfo("Downloading Card Database (2/5)");
            string pathCardDatabase = DownloadCardDatabase(root);
            //string pathCardDatabase = Path.Combine(root, "AtomicCards.json");
            Logger.LogInfo("Downloading Library Structure (3/5)");
            DraftLibrary(root, pathCardDatabase);
            Logger.LogInfo("Downloading Art (4/5)");
            DownloadArt(root, false, pathCardDatabase);
            Logger.LogInfo("Creating Thumbnails (5/5)");
            CreateThumbnails(root, false);
            Logger.LogInfo("Finished Updating Mtg Library");

        }

        public static string DownloadCardDatabase(string root)
        {
            var dest = Path.Combine(root, "AtomicCards.json");
            NetHelper.DownloadFile("https://mtgjson.com/api/v5/AtomicCards.json", dest);
            return dest;
        }


        public static void CreateThumbnails(string root, bool forceRecreation)
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

                Logger.LogInfo("CreateThumbnails", path);
                Logger.LogInfo("CreateThumbnails", "Created thumbnail " + (i++) + " of " + lib.ArtObjects.Length);
            }
        }
        public static void DownloadArt(string root, bool forceReload, string pathCardDatabase)
        {
            ArtLibrary lib = ArtLibrary.ReadLibrary(root);

            ArtDownloader artDownloader = new ArtDownloader(pathCardDatabase);
            artDownloader.DownloadArt(root, lib, forceReload);

            ArtLibrary.WriteLibrary(root, lib);
        }

        static void DraftLibrary(string root, string pathCardDatabas)
        {
            string listPath = Path.Combine(root, "art-pages.list");
            ArtDownloader artDownloader = new ArtDownloader(pathCardDatabas);
            List<ArtObject> art = new List<ArtObject>();

            int inspectedPages = 0;
            string[] lines = File.ReadAllLines(listPath);
            foreach (var page in lines)
            {
                if (page.Length == 0)
                    continue;
                var result = artDownloader.GetArtObject(page);

                if (result != null)
                    art.Add(result);
                inspectedPages++;
                if (inspectedPages % 100 == 0)
                    Logger.LogInfo("DraftLibrary", "Inspected " + inspectedPages + " pages of " + lines.Length);
            }
            ArtLibrary lib = new ArtLibrary()
            {
                ArtObjects = art.ToArray()
            };
            ArtLibrary.WriteLibrary(root, lib);

            //void checkString(string text)
            //{
            //    if (text == null)
            //        return;
            //    for (int i = 0; i < text.Length; i++)
            //    {
            //        if ('a' <= text[i] && text[i] <= 'z')
            //            continue;
            //        if ('A' <= text[i] && text[i] <= 'Z')
            //            continue;
            //        if ('0' <= text[i] && text[i] <= '9')
            //            continue;
            //        if (text[i] == ' ')
            //            continue;
            //        if (text[i] == ',')
            //            continue;
            //        if (text[i] == '.')
            //            continue;
            //        if (text[i] == '!')
            //            continue;
            //        if (text[i] == ':')
            //            continue;
            //        if (text[i] == '-')
            //            continue;
            //        if (text[i] == '&')
            //            continue;
            //        if (text[i] == '|')
            //            continue;
            //        if (text[i] == '/')
            //            continue;
            //        if (text[i] == '\'')
            //            continue;
            //        if (text[i] == '(')
            //            continue;
            //        if (text[i] == ')')
            //            continue;
            //        Console.WriteLine(text);
            //    }
            //}
        }

        public static void DownloadList(string root)
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
            int printUpdateCounter = 0;
            foreach (var page in spider.CollectPages("https://www.artofmtg.com/", page => page.StartsWith(target)))
            {
                if (page != null)
                    pages.Add(page);
                printUpdateCounter++;
                if (printUpdateCounter >= 100)
                {
                    Logger.LogInfo("Visited " + spider.HandledPages.Count + " pages");
                    Logger.LogInfo("ToDo: Visit " + spider.PagesToVisit.Count + " pages");
                    Logger.LogInfo("Collected " + pages.Count + " pages");
                    printUpdateCounter = 0;
                }
            }

            pages.Sort();

            using (var writer = new StreamWriter(filePath, false))
                foreach (var page in pages)
                    writer.WriteLine(page);

            Logger.LogInfo("found " + pages.Count + " art pages");
        }
    }
}
