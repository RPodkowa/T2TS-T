using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class CacheElem
    {
        public FilesType Type { get; private set; }
        public int Id { get; private set; }

        public int Flag { get; private set; }
        public bool IsCorrectedByHuman
        {
            get { return FlagHelper.IsSettedBit(Flag, 0); }
            set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
        }

        public string Key { get; private set; }
        /// <summary>
        /// Tekst wejsciowy (czysty) bez formatowania/normalizowania
        /// Taki tekst jest wczytywany w trakcie importu 
        /// </summary>
        public string InputText { get; private set; }
        /// <summary>
        /// Tekst do tlumaczenia po odpowiednim formatowaniu/normalizacji
        /// Taki tekst jest przekazywany do tlumaczenia (automatycznego i recznego)
        /// </summary>
        public string OriginalText { get; private set; }
        /// <summary>
        /// Tekst przetlumaczony po odpowiednim formatowaniu/normalizacji
        /// Taki tekst jest odbierany z tlumaczenia (automatycznego i recznego)
        /// </summary>
        public string TranslatedText { get; private set; }
        /// <summary>
        /// Tekst wyjsciowy (przetlumaczony) bez formatowania/normalizowania
        /// Taki tekst jest zapisywany w trakcie exportu
        /// </summary>
        public string OutputText { get; private set; }

        public List<string> Groups;
        private List<string> Specials;

        public bool IsDataBaseElem { get { return Type == FilesType.DataBase; } }
        public bool IsModulesElem { get { return Type == FilesType.Modules; } }
        public bool IsNamesElem { get { return Type == FilesType.Names; } }

        public bool ToTranslate { get { return TextHelper.EqualsTexts(OriginalText, TranslatedText); } }
        public bool ToConfirm { get { return !IsCorrectedByHuman; } }

        public CacheElem(FilesType type, int id, string key, string inputText)
        {
            Type = type;
            Id = id;
            Flag = 0;
            Key = key;
            if (IsModulesElem) Key = "";            
            InputText = inputText;
            OriginalText = TextHelper.Normalize(InputText, out Specials);
            TranslatedText = OriginalText;
            OutputText = InputText;
            Groups = new List<string>();
        }

        public CacheElem(FilesType type, XmlNode element)
        {
            Type = type;

            if (element.Attributes != null)
            {
                Id = int.Parse(element.Attributes["ID"]?.Value);
                Key = element.Attributes["Key"]?.Value.ToString();
                Flag = int.Parse(element.Attributes["Flag"]?.Value);
            }
            
            Groups = new List<string>();

            var groups = element.SelectNodes("Groups/Group");
            foreach (XmlNode group in groups)
            {
                AddGroup(group.InnerText);
            }

            InputText = element.SelectSingleNode("Texts/Input").InnerText;
            OriginalText = TextHelper.Normalize(InputText, out Specials);
            OutputText = element.SelectSingleNode("Texts/Output").InnerText;
            TranslatedText = TextHelper.Normalize(OutputText);
        }

        public bool EqualsTexts(string inputText)
        {
            return (GetTextToCompare() == inputText);
        }

        private string GetTextToCompare()
        {
            if (IsDataBaseElem) return Key;
            return InputText;
        }

        public XmlNode ToXmlNode(XmlDocument doc)
        {
            XmlNode elementNode = doc.CreateElement("Element");
            elementNode.Attributes.Append(GetAttribute(doc, "ID", Id.ToString()));
            elementNode.Attributes.Append(GetAttribute(doc, "Key", Key));
            elementNode.Attributes.Append(GetAttribute(doc, "Flag", Flag.ToString()));

            XmlNode groupsNode = doc.CreateElement("Groups");
            foreach (var group in Groups)
            {
                groupsNode.AppendChild(GetNode(doc, "Group", group));
            }
            elementNode.AppendChild(groupsNode);
            
            XmlNode textsNode = doc.CreateElement("Texts");
            textsNode.AppendChild(GetNode(doc, "Input", InputText));
            textsNode.AppendChild(GetNode(doc, "Output", OutputText));

            elementNode.AppendChild(textsNode);

            return elementNode;
        }

        private XmlNode GetNode(XmlDocument doc, string name, string value)
        {
            XmlNode node = doc.CreateElement(name);
            node.AppendChild(doc.CreateTextNode(value));
            return node;
        }

        private XmlAttribute GetAttribute(XmlDocument doc, string name, string value)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = value;
            return attribute;
        }

        public void SetTranslated(string text)
        {
            TranslatedText = text;
            OutputText = TextHelper.UnNormalize(text, Specials);
            if (IsModulesElem) OutputText = TextHelper.ReplacePolishChars(OutputText);
        }

        public void AddGroups(List<string> groups)
        {
            foreach (var group in groups)
            {
                AddGroup(group);
            }
        }

        public void AddGroup(string group)
        {
            if (InGroup(group)) return;
            Groups.Add(group);
        }

        public bool InGroup(string group)
        {
            return Groups.Contains(group);
        }

        public string GetTranslateLink()
        {
            return @"https://translate.google.pl/?hl=pl#view=home&op=translate&sl=en&tl=pl&text=" + OriginalText.ToLower();
        }
    }
}
