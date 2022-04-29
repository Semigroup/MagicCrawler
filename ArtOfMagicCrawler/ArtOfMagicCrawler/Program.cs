using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ArtOfMagicCrawler
{
    class Program
    {

        static void Main(string[] args)
        {
            string root = @"E:\ArtOfMagicLibrary";
            //WriteList(root);
            //WriteLibrary(root);
            DownloadLibrary(root);
        }
        static void DownloadLibrary(string root)
        {
            string libPath = Path.Combine(root, "art.library");
            var serializer = new XmlSerializer(typeof(ArtLibrary));
            ArtLibrary lib;
            using (var sr = new StreamReader(libPath))
                lib = (ArtLibrary)serializer.Deserialize(sr);

            ArtDownloader artDownloader = new ArtDownloader();
            artDownloader.DownloadArt(root, lib);

            using (var sw = new StreamWriter(Path.Combine(root, "art.library")))
                serializer.Serialize(sw, lib);
        }
        static void WriteLibrary(string root)
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
            var serializer = new XmlSerializer(lib.GetType());
            using (var sw = new StreamWriter(Path.Combine(root, "art.library")))
                serializer.Serialize(sw, lib);

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
