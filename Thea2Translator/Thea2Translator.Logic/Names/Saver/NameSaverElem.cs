using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NameSaverElem
    {
        public string Collection { get; private set; }
        public List<string> Races { get; private set; }
        public List<string> Subraces { get; private set; }
        public List<string> CharacterMaleNames { get; private set; }
        public List<string> CharacterFemaleNames { get; private set; }

        public NameSaverElem(string collection, string[] keyArray)
        {
            Collection = collection;
            Races = new List<string>();
            Subraces = new List<string>();
            CharacterMaleNames = new List<string>();
            CharacterFemaleNames = new List<string>();

            var keyList = new List<string>();
            foreach (var keyElem in keyArray)
            {
                if (string.IsNullOrEmpty(keyElem))
                    continue;

                var keyElemArray = keyElem.Split(',');
                keyList.AddRange(keyElemArray.ToList());
            }

            TryAddRaces(keyList);
            TryAddSubraces(keyList);
        }

        private void TryAddRaces(List<string> keyList)
        {
            TryAddListElem(keyList, Races, "RACE-");
        }

        private void TryAddSubraces(List<string> keyList)
        {
            TryAddListElem(keyList, Subraces, "SUBRACE-");
        }

        private void TryAddListElem(List<string> keyList, List<string> list, string pattern)
        {
            foreach (var keyElem in keyList)
            {
                if (keyElem.StartsWith(pattern)) list.Add(keyElem);
            }
        }

        public void AddByCacheElem(CacheElem cacheElem)
        {
            if (cacheElem.IsMale)
            {
                CharacterMaleNames.Add(cacheElem.OutputText);
                return;
            }

            if (cacheElem.IsFemale)
            {
                CharacterFemaleNames.Add(cacheElem.OutputText);
                return;
            }
            
            throw new Exception($"AddByCacheElem: Cos nie tak z elementem '{cacheElem.Key}'");
        }

        public XmlNode ToXmlNode(XmlDocument doc)
        {
            XmlNode elementNode = doc.CreateElement(Collection);

            AddXmlNodeByList(elementNode, Races, "Race");
            AddXmlNodeByList(elementNode, Subraces, "Subrace");
            AddXmlNodeByList(elementNode, CharacterMaleNames, "CharacterMale");
            AddXmlNodeByList(elementNode, CharacterFemaleNames, "CharacterFemale");

            return elementNode;
        }

        private void AddXmlNodeByList(XmlNode elementNode, List<string> list, string name)
        {
            if (list == null) return;
            
            foreach (var value in list)
            {
                XmlNode node = elementNode.OwnerDocument.CreateElement(name);
                node.Attributes.Append(XmlHelper.GetAttribute(elementNode.OwnerDocument, "Value", value));
                elementNode.AppendChild(node);
            }
        }
    }
}
