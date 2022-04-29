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

        public string[] Keys { get; set; }

        public string AbsoluteImagePath { get; set; }
        public string RelativeImagePath { get; set; }

        public override string ToString()
        {
            string s;
            if (Artist == null)
                s = CardName + " (" + MagicSet + ")";
            else
                s = CardName + " (" + MagicSet + "), " + Artist;
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
