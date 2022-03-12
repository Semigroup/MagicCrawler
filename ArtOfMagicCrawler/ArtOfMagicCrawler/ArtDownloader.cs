using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using EBookCrawler.Parsing;
using EBookCrawler;

namespace ArtOfMagicCrawler
{
    public class ArtDownloader
    {
        public WebClient Client { get; set; } = new WebClient();
        public Tokenizer Tokenizer { get; set; } = new Tokenizer();
        public Repairer Repairer { get; set; } = new Repairer();
        public Parser Parser { get; set; } = new Parser();

        public ArtObject Download(string url)
        {
            string source = Client.GetSource(url);
            if (source == null)
                return null;
            source = HTMLHelper.RemoveHTMLComments(source);

            Tokenizer.Tokenize(source);
            var tokens = Tokenizer.Tokens;
            Repairer.Repair(source, tokens);
            tokens = Repairer.Output;
            Parser.Parse(tokens);

            var divArea = FindPostArea(Parser.Root);
            if (divArea == null)
            {
                Logger.LogLine("Couldnt find post area in " + url);
                return null;
            }



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
        private Parser.Node FindPostArea(Parser.Node node)
        {
            bool isPostArea(Token token)
            {
                if (node.Token.Tag == "div")
                    return false;
                foreach (var att in token.Attributes)
                    if (att.Name == "id" && att.Value == "post-area")
                        return true;
                return false;
            }

            return Find(node, isPostArea);
        }
        private Parser.Node FindHeader1(Parser.Node node)
            => Find(node, t => t.Tag == "h1");
    }
}
