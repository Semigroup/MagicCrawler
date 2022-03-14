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
           source = source.Replace("<!doctype html>", ""); //ToDo

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
            var header1 = FindHeader1(divArea);
            var result = new ArtObject();

            var headerWords = CollectRaws(header1).ToArray();
            int i = headerWords[0].IndexOf("MtG Art");
            headerWords[0] = headerWords[0].Substring(0, i);
            i = headerWords[0].IndexOf('(');
            if (i > 0)
                headerWords[0] = headerWords[0].Substring(0, i);
            result.CardName = headerWords[0].Trim();
            result.MagicSet = headerWords[1].Trim();
            result.Artist = headerWords[3].Trim();
            return result;
        }

        private IEnumerable<string> CollectRaws(Parser.Node node)
        {
            if (node.Token.MyKind == Token.Kind.Raw)
                yield return node.Token.Text;
            foreach (var child in node.Children)
                foreach (var raw in CollectRaws(child))
                    yield return raw;
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
                if (node.IsRoot)
                    return false;
                if (node.Token.Tag != "div")
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
