using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ArtOfMagicCrawler
{
    public class ArtObject
    {
        public string Artist { get; set; }
        public string CardName { get; set; }
        public string MagicSet { get; set; }
        public string WebPage { get; set; }
        public string ImageURL { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string[] Keys { get; set; }
        /// <summary>
        /// Bei Magic Karten hier eintragen, ob ein DB Entry gefunden wurde
        /// </summary>
        public string Note { get; set; }

        public string AbsoluteImagePath { get; set; }
        public string RelativeImagePath { get; set; }
        public string AbsoluteThumbnailPath
        {
            get
            {
                var start = AbsoluteImagePath.Substring(0, AbsoluteImagePath.Length - RelativeImagePath.Length);
                var middle = @"thumbnails";
                var replaced = @"\images\";
                var end = RelativeImagePath.Substring(replaced.Length, RelativeImagePath.Length - replaced.Length);
                var path = Path.Combine(Path.Combine(start, middle), end);
                return path;
            }
        }

        public override string ToString()
        {
            string s;
            if (Artist == null)
                s = CardName + " (" + MagicSet + ")";
            else
                s = CardName + " (" + MagicSet + "), " + Artist;
            s += ", " + Width + "x" + Height;
            if (WebPage != null)
                s += "\r\n" + WebPage;
            if (ImageURL != null)
                s += "\r\n" + ImageURL;
            if (AbsoluteImagePath != null)
                s += "\r\n" + AbsoluteImagePath;

            return s;
        }
    }
}
