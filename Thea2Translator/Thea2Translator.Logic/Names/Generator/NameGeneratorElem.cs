using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NameGeneratorElem
    {
        public static string MaleString = "CharacterMale";
        public static string FemaleString = "CharacterFemale";

        public string Collection { get; private set; }
        public string Race { get; private set; }
        public string Description { get; private set; }
        public int Flag { get; private set; }
        public bool IsConfirmation
        {
            get { return FlagHelper.IsSettedBit(Flag, 0); }
            private set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
        }
        public bool IsDeactivation
        {
            get { return FlagHelper.IsSettedBit(Flag, 1); }
            private set { Flag = FlagHelper.GetSettedBitValue(Flag, 1, value); }
        }
        public List<string> Subraces { get; private set; }
        public List<string> CharacterMaleNames { get; private set; }
        public List<string> CharacterFemaleNames { get; private set; }
        public List<NameGeneratorElemCollection> Collections { get; private set; }

        public bool Used { get; private set; }

        public NameGeneratorElem(XmlNode element)
        {
            if (element.Attributes != null)
            {
                Collection = XmlHelper.GetNodeAttribute(element, "Collection");
                Race = XmlHelper.GetNodeAttribute(element, "Race");
                Flag = XmlHelper.GetNodeAttribute(element, "Flag", 0);
            }

            Description = XmlHelper.GetNodeText(element, "Description");

            Subraces = new List<string>();

            var subraces = element.SelectNodes("Subraces/Subrace");
            foreach (XmlNode subrace in subraces)
            {
                Subraces.Add(subrace.InnerText);
            }

            ReadNamesFromString(XmlHelper.GetNodeText(element, "Names/Male"), true);
            ReadNamesFromString(XmlHelper.GetNodeText(element, "Names/Female"), false);

            Collections = NameGeneratorElemCollection.ReadFromXmlNode(element);
            Used = false;
        }

        public void InsertCacheElems(IList<CacheElem> cacheElems)
        {
            InsertCacheElemsFromNames(cacheElems, true);
            InsertCacheElemsFromNames(cacheElems, false);
            Used = true;
        }

        private void InsertCacheElemsFromNames(IList<CacheElem> cacheElems, bool male)
        {
            var names = GetAllNames(male);
            names = names.Distinct().ToList();
            names.Sort();
            var gender = male ? MaleString : FemaleString;
            if (names == null) return;
            foreach (var name in names)
            {
                cacheElems.Add(new CacheElem(Collection, Race, Subraces, gender, name));
            }
        }

        private List<string> GetAllNames(bool male)
        {
            var allNames = new List<string>();
            if (male && CharacterMaleNames != null) allNames.AddRange(CharacterMaleNames);
            if (!male && CharacterFemaleNames != null) allNames.AddRange(CharacterFemaleNames);

            if (Collections == null)
                return allNames;

            foreach (var collection in Collections)
            {
                if (collection.IsMale == male)
                    allNames.AddRange(collection.GetNames());
            }

            return allNames;
        }

        private void ReadNamesFromString(string namesString, bool male)
        {
            if (string.IsNullOrEmpty(namesString))
                return;

            namesString = TextHelper.NormalizeNames(namesString);

            var list = namesString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (male) CharacterMaleNames = list;
            else CharacterFemaleNames = list;
        }
    }
}

//<NameGenerator>
//  <Element Collection = "NAME_COLLECTION-PET_STRONG_BEAR" Race="RACE-BEAST">    
//    <Subraces>
//      <Subrace>SUBRACE-PET_VWEAK_BEAR</Subrace>
//      <Subrace>SUBRACE-PET_WEAK_BEAR</Subrace>
//      <Subrace>SUBRACE-PET_BEAR</Subrace>
//      <Subrace>SUBRACE-PET_STRONG_BEAR</Subrace>
//      <Subrace>SUBRACE-PET_VSTRONG_BEAR</Subrace>
//    </Subraces>
//    <Names>
//      <Male>Wits</Male>
//      <Female>Spryt</Female>
//    </Names>
//    <Series>
//      <Serie Sex="F">
//        <Part1>Duzy,Maly</Part1>
//        <Part2>Palec,Kiel</Part2>
//      </Serie>
//      <Serie Sex="M">
//        <Part1>Duzy,Maly</Part1>
//        <Part2>Palec,Kiel</Part2>
//      </Serie>
//    </Series>
//  </Element>
//</NameGenerator>

