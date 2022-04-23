using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ArtOfMagicCrawler
{
    class Program
    {

        static void Main(string[] args)
        {
            string root = @"E:\ArtOfMagicLibrary";
            //WriteList(root);
            DownloadArt(root);
        }

        static void DownloadArt(string root)
        {
            string listPath = Path.Combine(root, "art-pages.list");
            ArtDownloader artDownloader = new ArtDownloader();

            foreach (var page in File.ReadLines(listPath))
            {
                if (page.Length == 0)
                    continue;
                var result = artDownloader.Download(page);
                Console.WriteLine(page);
                Console.WriteLine(result);
            }

        }

        static void WriteList(string root)
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
