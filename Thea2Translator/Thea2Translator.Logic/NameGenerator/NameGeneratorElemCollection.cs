using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NameGeneratorElemCollection
    {
        public string Gender { get; private set; }
        public string Prefix { get; private set; }
        public string Sufix { get; private set; }
        public bool IsMale { get { return Gender == NameGeneratorElem.MaleString; } }
        public bool IsFemale { get { return Gender == NameGeneratorElem.FemaleString; } }
        public List<string> Part1 { get; private set; }
        public List<string> Part2 { get; private set; }

        public NameGeneratorElemCollection(XmlNode element)
        {
            if (element.Attributes != null)
            {
                Gender = element.Attributes["Gender"]?.Value.ToString();
                Prefix = element.Attributes["Prefix"]?.Value.ToString();
                Sufix = element.Attributes["Sufix"]?.Value.ToString();
            }
            
            ReadPartFromString(XmlHelper.GetNodeText(element, "Part1"), true);
            ReadPartFromString(XmlHelper.GetNodeText(element, "Part2"), false);
        }
        
        private void ReadPartFromString(string namesString, bool part1)
        {
            namesString = TextHelper.NormalizeName(namesString);
            var list = namesString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (part1) Part1 = list;
            else Part2 = list;
        }

        public static List<NameGeneratorElemCollection> ReadFromXmlNode(XmlNode element)
        {
            var collections = element.SelectNodes("Collections/Collection");
            if (collections.Count == 0)
                return null;

            var ret = new List<NameGeneratorElemCollection>();
            foreach (XmlNode collection in collections)
            {
                ret.Add(new NameGeneratorElemCollection(collection));
            }

            return ret;
        }

        public List<string> GetNames()
        {
            var ret = new List<string>();
            foreach (var element1 in Part1)
            {
                foreach (var element2 in Part2)
                {
                    var name = $"{element1} {element2}";
                    if (!string.IsNullOrEmpty(Prefix)) name = Prefix + name;
                    if (!string.IsNullOrEmpty(Sufix)) 
                    {
                        name = name + Sufix;
                        name = name.Replace("ssdottir", "sdottir");
                        name = name.Replace("ssson", "sson");
                    }
                    ret.Add($"{Prefix}{element1} {element2}{Sufix}");
                }
            }

            return ret;
        }
    }
}