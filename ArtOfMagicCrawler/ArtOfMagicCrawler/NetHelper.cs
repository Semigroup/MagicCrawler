using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using EBookCrawler;

namespace ArtOfMagicCrawler
{
    public static class NetHelper
    {
        public static string GetLastPartOfURL(string url)
        {
            int i = url.LastIndexOf("/");
            return url.Substring(i + 1);
        }

        public static string GetSource(this WebClient client, string url)
        {
            try
            {
                return client.DownloadString(url);
            }
            catch (Exception e)
            {
                Logger.LogError("Couldnt load source of " + url);
                Console.WriteLine(e);
                return null;
            }
        }

        public static void DownloadFile(string url, string pathDestination)
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.DownloadFile(url, pathDestination);
            }
        }
    }
}
