using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using EBookCrawler.Parsing;
using EBookCrawler;
using System.IO;
using System.Web;
using System.Drawing;
using static System.Net.WebRequestMethods;


namespace ArtOfMagicCrawler
{
    public class ArtDownloader
    {
        public WebClient Client { get; set; } = new WebClient()
        {
            Encoding = Encoding.UTF8
        };
        public Tokenizer Tokenizer { get; set; } = new Tokenizer();
        public Repairer Repairer { get; set; } = new Repairer();
        public Parser Parser { get; set; } = new Parser();
        public MagicInfoProvider InfoProvider { get; set; }

        public ArtDownloader(string pathCardDatabase)
        {
            this.InfoProvider = new MagicInfoProvider(pathCardDatabase);
        }


        public ArtObject GetArtObject(string url)
        {
            string source = Client.GetSource(url);
            if (source == null)
                return null;

            source = HTMLHelper.RemoveDoctype(source, out string doctype);
            source = HTMLHelper.RemoveHTMLComments(source);
            source = HttpUtility.HtmlDecode(source);

            System.IO.File.WriteAllText("test.html", source);
            Tokenizer.Tokenize(source);
            IEnumerable<Token> tokens = Tokenizer.Tokens;

            tokens = Repairer.RemoveTag("link", tokens);
            Repairer.Repair(source, tokens);
            tokens = Repairer.Output;

            Parser.Parse(tokens);

            var divArea = FindPostArea(Parser.Root);
            if (divArea == null)
            {
                Logger.LogError("Couldnt find post area in " + url);
                return null;
            }
            var header1 = FindHeader1(divArea);
            var result = new ArtObject()
            {
                WebPage = url
            };

            var headerWords = CollectRaws(header1).ToArray();
            int i = headerWords[0].IndexOf("MtG Art");
            headerWords[0] = headerWords[0].Substring(0, i);
            //i = headerWords[0].IndexOf('(');
            //if (i > 0)
            //    headerWords[0] = headerWords[0].Substring(0, i);
            result.CardName = headerWords[0].Trim();
            result.MagicSet = headerWords[1].Trim();
            if (headerWords.Length > 3)
                result.Artist = headerWords[3].Trim();

            var imgNode = FindImage(divArea);
            if (imgNode == null)
                Logger.LogError("[ArtDownloader] Couldnt find Image on " + url);
            else
            {
                result.ImageURL = imgNode.Token.GetAttribute("data-src");
                //sometimes result.ImageURL is /cdn-cgi/mirage/3197c01eb48ccb5cd20121f2837c38f111c2b014c64370f762b356a65dd36ebb/1440/https://www.artofmtg.com/wp-content/uploads/2022/12/Rocketship-Unfinity-MtG-Art.jpg
                //I have no idea why (this is not the case in the original source code accessed by browser)
                int index = result.ImageURL.IndexOf("https://");
                result.ImageURL = result.ImageURL.Substring(index);
            }

            IEnumerable<string> keys = null;
            if (!result.CardName.ToLower().EndsWith(" token"))
                keys = InfoProvider.GetKeys(result.CardName);
            if (keys == null)
                keys = new string[0];

            if (result.CardName != null)
                keys = keys.Append(result.CardName);
            if (result.Artist != null)
                keys = keys.Append(result.Artist);
            if (result.MagicSet != null)
                keys = keys.Append(result.MagicSet);

            result.Keys = keys.Select(x => x.ToLower()).ToArray();

            return result;
        }

        public string SimplifySymbols(string source)
        {
            source = source.Replace("Æ", "AE");
            source = source.Replace("ñ", "n");
            source = source.Replace("ó", "o");
            source = source.Replace("é", "e");
            source = source.Replace("ł", "l");
            source = source.Replace("\u200F", "");
            source = source.Replace("–", "-");
            source = source.Replace("+", "//");
            source = source.Replace("’", "'");
            source = source.Replace("`", "'");
            source = source.Replace("´", "'");
            return source;
        }
        private IEnumerable<string> CollectRaws(Parser.Node node)
        {
            if (node.Token.MyKind == Token.Kind.Raw)
                yield return SimplifySymbols(node.Token.Text);
            else
                foreach (var child in node.Children)
                    foreach (var raw in CollectRaws(child))
                        yield return SimplifySymbols(raw);
        }
        private Parser.Node Find(Parser.Node node, Predicate<Token> selector)
        {
            if (selector(node.Token))
                return node;

            if (!node.IsLeaf)
                foreach (var child in node.Children)
                {
                    var result = Find(child, selector);
                    if (result != null)
                        return result;
                }
            return null;
        }
        private Parser.Node FindPostArea(Parser.Node root)
        {
            bool isPostArea(Token token)
            {
                if (token == null)
                    return false;
                if (token.Tag == null)
                    return false;
                if (token.Tag.ToLower() != "div")
                    return false;
                foreach (var att in token.Attributes)
                    if (att.Name == "id" && att.Value == "post-area")
                        return true;
                return false;
            }

            return Find(root, isPostArea);
        }
        private Parser.Node FindHeader1(Parser.Node node)
            => Find(node, t => t.Tag == "h1");
        private Parser.Node FindImage(Parser.Node node)
             => Find(node,
                 t => t.Tag == "img"
                 && t.GetAttribute("class").Contains("attachment-full")
                 && t.GetAttribute("class").Contains("wp-post-image")
                 );

        public void DownloadArt(string root, ArtLibrary lib, bool forceReload)
        {
            string imgDir = Path.Combine(root, @"images\");
            if (!Directory.Exists(imgDir))
                Directory.CreateDirectory(imgDir);

            List<string> imagePaths = new List<string>();
            List<ArtObject> non_webps = new List<ArtObject>();

            int i = 0;
            foreach (var art in lib.ArtObjects)
            {
                var folderName = ToFolderName(art.MagicSet);
                var rawName = ToFileName(art.CardName);
                var extension = Path.GetExtension(art.ImageURL);
                var fileName = rawName + extension;

                var directory = Path.Combine(imgDir, folderName);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string path = Path.Combine(directory, fileName);
                if (imagePaths.Contains(path))
                {
                    int n = 2;
                    fileName = rawName + n + Path.GetExtension(art.ImageURL);
                    path = Path.Combine(directory, fileName);
                    while (imagePaths.Contains(path))
                    {
                        n++;
                        fileName = rawName + n + Path.GetExtension(art.ImageURL);
                        path = Path.Combine(directory, fileName);
                    }
                }
                imagePaths.Add(path);

                i++;
                Logger.LogInfo("ArtDownloader", "Downloading to " + path);
                Logger.LogInfo("ArtDownloader", i + " of " + lib.ArtObjects.Length);

                if (!System.IO.File.Exists(path) || forceReload)
                    Client.DownloadFile(art.ImageURL, path);
                else
                    Logger.LogWarning("ArtDownloader", "File already exists. Not reloaded!");

                art.AbsoluteImagePath = path;
                art.RelativeImagePath = Path.Combine(@"\images\", folderName, fileName);

                try
                {
                    using (Image img = Image.FromFile(path))
                    {
                        art.Width = img.Width;
                        art.Height = img.Height;
                    }
                    non_webps.Add(art);
                }
                catch (Exception)
                {
                    Logger.LogError("[ArtDownloader] WebP cannot be handled: " + path);
                }
                Logger.LogInfo("ArtDownloader", art);
                Logger.LogInfo("ArtDownloader", art.ImageURL);
            }
            lib.ArtObjects = non_webps.ToArray();
        }

        private string ToFolderName(string setname)
        {
            setname = setname.Replace('-', '_');
            setname = setname.Replace(".", "");
            setname = setname.Replace(":", "");
            setname = setname.Replace("'", "");
            setname = setname.Replace(",", "");
            setname = setname.Replace("(", "");
            setname = setname.Replace(")", "");
            setname = setname.Replace(" ", "");
            return setname;
        }
        private string ToFileName(string cardname)
        {
            cardname = cardname.Replace('-', '_');
            cardname = cardname.Replace(".", "");
            cardname = cardname.Replace(":", "");
            cardname = cardname.Replace("'", "");
            cardname = cardname.Replace(",", "");
            cardname = cardname.Replace("|", "");
            cardname = cardname.Replace("(", "");
            cardname = cardname.Replace(")", "");
            cardname = cardname.Replace("/", "");
            cardname = cardname.Replace(" ", "");
            cardname = cardname.Replace("?", "");
            cardname = cardname.Replace("!", "");
            return cardname;
        }
    }
}
