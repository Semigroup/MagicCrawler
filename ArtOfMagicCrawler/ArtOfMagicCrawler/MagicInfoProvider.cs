using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using EBookCrawler;

namespace ArtOfMagicCrawler
{
    public class MagicInfoProvider
    {
        public struct Card
        {
            public string Name;
            public IEnumerable<string> Keys;
        }

        private SortedDictionary<string, Card> Cards = new SortedDictionary<string, Card>();

        public string PathCardDatabase { get; private set; }

        public MagicInfoProvider(string PathCardDatabase)
        {
                this.PathCardDatabase = PathCardDatabase;

            IEnumerable<string> getKeys(JToken arr)
            {
                yield return (string)arr["name"];

                JArray colors = arr["colorIdentity"] as JArray;
                foreach (string item in colors)
                {
                    switch (item.ToLower())
                    {
                        case "r":
                            yield return "red";
                            break;
                        case "u":
                            yield return "blue";
                            break;
                        case "g":
                            yield return "green";
                            break;
                        case "w":
                            yield return "white";
                            break;
                        case "b":
                            yield return "black";
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                JArray types = arr["types"] as JArray;
                foreach (string item in types)
                    yield return item;

                var text = arr["text"];
                if (text != null)
                    yield return (string)text;
            }

            using (StreamReader file = File.OpenText(PathCardDatabase))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JsonSerializer serializer = JsonSerializer.Create();
                var database = (JObject)serializer.Deserialize(reader);
                bool b = database.TryGetValue("data", out JToken token);
                foreach (JProperty item in token.Children())
                {
                    int subCards = item.First.Children().Count();
                    if (subCards > 2)
                    {
                        Logger.LogError("MagicInfoProvider", item.Name + " has " + subCards + " subcards!");
                        continue;
                    }
                    foreach (JObject cardInfo in item.First)
                    {
                        //TODO verbessern
                        string name;
                        if (subCards == 1)
                            name = (string)cardInfo["name"];
                        else
                            name = (string)cardInfo["faceName"];

                        if (name == null)
                        {
                            Logger.LogError("MagicInfoProvider", item.Name + " has subcards without name");
                            continue;
                        }

                        Card card = new Card()
                        {
                            Name = Simplify(name),
                            Keys = getKeys(cardInfo)
                        };
                        if (Cards.ContainsKey(card.Name))
                            Logger.LogError("MagicInfoProvider", card.Name + " (" + item.Name + ")" +
                                " already contained in database");
                        else
                            Cards.Add(card.Name, card);
                    }
                }
            }
        }

        private string Simplify(string name)
        {
            int i = name.IndexOf('(');
            if (i > 0)
                name = name.Substring(0, i);

            string[] trailingWords =
            {
                " promo",
                " variant",
                " duel decks version",
                " alternate art",
                " art"
            };

            name = name.Replace(",", "");
            name = name.ToLower();

            foreach (var trail in trailingWords)
                if (name.Length > trail.Length && name.Substring(name.Length - trail.Length) == trail)
                    name = name.Substring(0, name.Length - trail.Length);

            name = name.Trim();
            return name.ToLower();
        }

        public IEnumerable<string> GetKeys(string cardName)
        {
            //TODO verbessern
            var simpleName = Simplify(cardName);
            if (Cards.TryGetValue(simpleName, out Card card))
                return card.Keys;
            else
            {
                Logger.LogError("MagicInfoProvider", "Couldnt find DataBase Entry for " + cardName);
                return null;
            }
        }
    }
}
