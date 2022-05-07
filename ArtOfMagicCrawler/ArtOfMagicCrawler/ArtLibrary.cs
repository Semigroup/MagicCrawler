using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace ArtOfMagicCrawler
{
    public class ArtLibrary
    {
        public ArtObject[] ArtObjects { get; set; }

        public static ArtLibrary ReadLibrary(string root)
        {
            string libPath = Path.Combine(root, "art.library");
            var serializer = new XmlSerializer(typeof(ArtLibrary));
            ArtLibrary lib;
            using (var sr = new StreamReader(libPath))
                lib = (ArtLibrary)serializer.Deserialize(sr);
            return lib;
        }
        public static void WriteLibrary(string root, ArtLibrary library)
        {
            var serializer = new XmlSerializer(library.GetType());
            using (var sw = new StreamWriter(Path.Combine(root, "art.library")))
                serializer.Serialize(sw, library);
        }
    }
}
