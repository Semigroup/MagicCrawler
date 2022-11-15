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

        public Queue<string> PagesToVisit { get; private set; }
        public HashSet<string> HandledPages { get; private set; }

        public IEnumerable<string> CollectPages(string root, Predicate<string> target)
        {
            var regex = new Regex("\"" + root.Replace(".", "\\.") + "[^\"]*\"");
            var client = new WebClient();

            this.HandledPages = new HashSet<string>();
            this.PagesToVisit = new Queue<string>();
            PagesToVisit.Enqueue(root);

            while (PagesToVisit.Count > 0)
            {
                var page = PagesToVisit.Dequeue();

                string source = client.GetSource(page);
                if (source == null)
                    continue;

                var matches = regex.Matches(source);
                foreach (Match match in matches)
                {
                    var newPage = match.Value;
                    newPage = newPage.Substring(1, newPage.Length - 2);

                    if (HandledPages.Contains(newPage))
                        continue;

                    HandledPages.Add(newPage);

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
                            {
                                Logger.LogWarning("[Spider] page refers to unknown file endings:");
                                Logger.LogWarning("[Spider] page:" + page);
                                Logger.LogWarning("[Spider] link:" + newPage);
                                Logger.LogWarning("[Spider] ending:" + ending);
                            }
                        }
                        else
                            PagesToVisit.Enqueue(newPage);
                        yield return null;
                    }
                }
            }

            yield break;
        }


    }
}
