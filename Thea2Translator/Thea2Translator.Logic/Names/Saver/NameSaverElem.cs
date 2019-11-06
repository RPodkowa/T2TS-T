using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NameSaverElem : IComparable<NameSaverElem>
    {
        public string Collection { get; private set; }
        public RaceInfo Race { get; private set; }
        public List<string> Subraces { get; private set; }
        public List<string> CharacterMaleNames { get; private set; }
        public List<string> CharacterFemaleNames { get; private set; }

        public NameSaverElem(string collection, string[] keyArray)
        {
            Collection = collection;
            Race = RaceInfo.GetEmptyRaceInfo();
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
            var Races = new List<string>();
            TryAddListElem(keyList, Races, "RACE-");
            if (Races.Count > 1)
                throw new Exception("Wiecej niz 1 rasa!");

            if (Races.Count > 0)
                Race = new RaceInfo(Races[0]);
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

            if (Race.Race != RaceType.NONE)
                AddXmlNodeByList(elementNode, new List<string> { Race.GetFullName() }, "Race");

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

        public int CompareTo(NameSaverElem other)
        {
            var raceCompareResult = Race.CompareTo(other.Race);
            if (raceCompareResult != 0)
                return raceCompareResult;

            var subraceCompareResult = CompareSubraces(other.Subraces);
            if (subraceCompareResult != 0)
                return subraceCompareResult;

            return string.Compare(Collection, other.Collection, comparisonType: StringComparison.OrdinalIgnoreCase);
        }

        private int CompareSubraces(List<string> otherSubraces)
        {
            int myCount = 0;
            int otherCount = 0;
            if (Subraces != null) myCount = Subraces.Count;
            if (otherSubraces != null) otherCount = otherSubraces.Count;

            return myCount.CompareTo(otherCount);
        }

        public override string ToString()
        {
            var stringList = new List<string>();

            if (Race == null) stringList.Add($"NULL");
            else stringList.Add($"{Race.Race.ToString()}");

            stringList.Add($"{Collection} ({Subraces.Count}) ♀️({CharacterFemaleNames.Count}) ♂️({CharacterMaleNames.Count})");

            return string.Join(" ", stringList.ToArray());
        }
    }
}
