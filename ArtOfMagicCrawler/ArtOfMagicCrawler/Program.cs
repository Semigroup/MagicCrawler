using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtOfMagicCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            string target = "https://www.artofmtg.com/art/";

            Spider spider = new Spider();
            int i = 0;
            foreach (var page in spider.CollectPages("https://www.artofmtg.com/", page => page.StartsWith(target)))
            {
                i++;
                Console.WriteLine(i + ": " + page);
            }
            Console.ReadKey();
        }
    }
}
