using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class NameGeneratorElemCollection
    {
        public string Gender { get; private set; }
        public string Prefix { get; private set; }
        public string Infix { get; private set; }
        public string Sufix { get; private set; }
        public bool IsMale { get { return Gender == NameGeneratorElem.MaleString; } }
        public bool IsFemale { get { return Gender == NameGeneratorElem.FemaleString; } }
        public int Flag { get; private set; }
        public bool AddElemAsName
        {
            get { return FlagHelper.IsSettedBit(Flag, 0); }
            private set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
        }
        public bool AddPrefix
        {
            get { return FlagHelper.IsSettedBit(Flag, 1); }
            private set { Flag = FlagHelper.GetSettedBitValue(Flag, 1, value); }
        }
        public List<string> Part1 { get; private set; }
        public List<string> Part2 { get; private set; }

        public NameDictionary CollectionDictionary { get; private set; }

        public NameGeneratorElemCollection(XmlNode element)
        {
            Flag = 0;
            if (element.Attributes != null)
            {
                Gender = XmlHelper.GetNodeAttribute(element, "Gender");
                Prefix = XmlHelper.GetNodeAttribute(element, "Prefix");
                Infix = XmlHelper.GetNodeAttribute(element, "Infix");
                Sufix = XmlHelper.GetNodeAttribute(element, "Sufix");

                if (Prefix == null) Prefix = "";
                if (Infix == null) Infix = " ";
                if (Sufix == null) Sufix = "";

                TryAddToDictionary("Przedrostek", Prefix, XmlHelper.GetNodeAttribute(element, "PrefixInfo"));
                TryAddToDictionary("Końcówka", Sufix, XmlHelper.GetNodeAttribute(element, "SufixInfo"));

                Flag = XmlHelper.GetNodeAttribute(element, "Flag", 0);
            }

            ReadPart(element, 1);
            ReadPart(element, 2);
        }

        private void TryAddToDictionary(string type, string oryginalWord, string translatedWord)
        {
            if (CollectionDictionary == null) CollectionDictionary = new NameDictionary();

            if (string.IsNullOrWhiteSpace(oryginalWord) || string.IsNullOrWhiteSpace(translatedWord))
                return;

            CollectionDictionary.TryAddWord(type, oryginalWord, translatedWord);
        }

        private void ReadPart(XmlNode element, int partNumber)
        {
            var partName = $"Part{partNumber}";
            var partNode = XmlHelper.GetChildNode(element, partName);
            if (partNode == null)
                throw new Exception($"Cos nie tak! Brak part: '{partName}'");

            var info = XmlHelper.GetNodeAttribute(partNode, "Info");

            var namesString = TextHelper.NormalizeNames(partNode.InnerText);
            var names = namesString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var namesList = new List<string>();
            foreach (var nameElem in names)
            {
                string translated = "";
                var name = TextHelper.NormalizeName(nameElem, out translated);
                TryAddToDictionary(info, name, translated);
                namesList.Add(name);
            }

            if (partNumber == 1) Part1 = namesList;
            else Part2 = namesList;
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
                    var name = $"{Prefix}{element1}{Infix}{element2}{Sufix}";
                    name = name.Replace("ssdottir", "sdottir");
                    name = name.Replace("ssson", "sson");
                    ret.Add(name);

                    if (AddElemAsName) ret.Add(element2);
                    if (AddPrefix) ret.Add($"{Prefix}{element2}");
                }

                if (AddElemAsName) ret.Add(element1);
                if (AddPrefix) ret.Add($"{Prefix}{element1}");
            }

            if (AddPrefix) ret.Add(Prefix.Replace(" ",""));
            return ret;
        }
    }
}