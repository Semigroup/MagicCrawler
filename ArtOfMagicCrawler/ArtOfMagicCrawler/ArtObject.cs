using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtOfMagicCrawler
{
   public class ArtObject
    {
        public string ImageURL { get; set; }
        public string Artist { get; set; }
        public string CardName { get; set; }
        public string MagicSet { get; set; }

        public override string ToString()
        {
            return CardName + " (" + MagicSet + "), " + Artist;
        }
    }
}
