using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using EBookCrawler;

namespace ArtOfMagicCrawler
{
    public class Spider
    {
        public string[] Endings = {
        "js", "jpg", "ico", "css"
        };

        public IEnumerable<string> CollectPages(string root, Predicate<string> target)
        {
            var regex = new Regex("\"" + root.Replace(".", "\\.") + "[^\"]*\"");
            var client = new WebClient();

            var handled = new HashSet<string>();
            var toVisit = new Queue<string>();
            toVisit.Enqueue(root);

            while (toVisit.Count > 0)
            {
                var page = toVisit.Dequeue();

                string source = client.GetSource(page);
                if (source == null)
                    continue;

                var matches = regex.Matches(source);
                foreach (Match match in matches)
                {
                    var newPage = match.Value;
                    newPage = newPage.Substring(1, newPage.Length - 2);

                    if (handled.Contains(newPage))
                        continue;

                    handled.Add(newPage);

                    if (target(newPage))
                        yield return newPage;
                    else
                    {
                        var lastPart = NetHelper.GetLastPartOfURL(newPage);
                        if (lastPart.Contains('.'))
                        {
                            string[] subparts = lastPart.Split('.');
                            string ending = subparts.Last();

                            if (!Endings.Contains(ending))
                                Logger.LogLine(ending);
                        }
                        else
                            toVisit.Enqueue(newPage);
                    }
                }
            }

            yield break;
        }


    }
}
