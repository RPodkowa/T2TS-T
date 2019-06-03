﻿using System;
using System.Collections.Generic;
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
            private set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
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
        /// Poprzedni przetlumaczony tekst
        /// Tekst jest ustawiany w momencie importu z Steam jesli zmienil sie oryginalny tekst
        /// Usuwane z XML po zatwierdzeniu tlumaczenia
        /// </summary>
        public string OldTranslatedText { get; private set; }
        /// <summary>
        /// Tekst wyjsciowy (przetlumaczony) bez formatowania/normalizowania
        /// Taki tekst jest zapisywany w trakcie exportu
        /// </summary>
        public string OutputText { get; private set; }

        private string ConfirmationTime;
        private string ConfirmationUser;
        private string ConfirmationGuid;

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

            InputText = GetNodeText(element, "Texts/Input");
            OriginalText = TextHelper.Normalize(InputText, out Specials);
            OutputText = GetNodeText(element, "Texts/Output");
            TranslatedText = TextHelper.Normalize(OutputText);
            OldTranslatedText = GetNodeText(element, "Texts/Old");

            ConfirmationTime = GetNodeText(element, "Confirmation/Time");
            ConfirmationGuid = GetNodeText(element, "Confirmation/GUID");
            ConfirmationUser = GetNodeText(element, "Confirmation/User");
        }

        private string GetNodeText(XmlNode element, string xpath)
        {
            var node = element.SelectSingleNode(xpath);
            if (node == null) return "";

            var ret = node.InnerText;
            return ret;
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

            XmlNode confirmationNode = doc.CreateElement("Confirmation");
            if (!string.IsNullOrEmpty(ConfirmationTime)) confirmationNode.AppendChild(GetNode(doc, "Time", ConfirmationTime));
            if (!string.IsNullOrEmpty(ConfirmationGuid)) confirmationNode.AppendChild(GetNode(doc, "GUID", ConfirmationGuid));
            if (!string.IsNullOrEmpty(ConfirmationUser)) confirmationNode.AppendChild(GetNode(doc, "User", ConfirmationUser));
            if (confirmationNode.ChildNodes != null && confirmationNode.ChildNodes.Count > 0)
                elementNode.AppendChild(confirmationNode);

            XmlNode groupsNode = doc.CreateElement("Groups");
            foreach (var group in Groups)
            {
                groupsNode.AppendChild(GetNode(doc, "Group", group));
            }
            elementNode.AppendChild(groupsNode);

            XmlNode textsNode = doc.CreateElement("Texts");
            textsNode.AppendChild(GetNode(doc, "Input", InputText));
            textsNode.AppendChild(GetNode(doc, "Output", OutputText));
            if (!string.IsNullOrEmpty(OldTranslatedText)) textsNode.AppendChild(GetNode(doc, "Old", OldTranslatedText));
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

        public void SetTranslated(string text, bool withConfirm)
        {
            TranslatedText = text;
            OutputText = TextHelper.UnNormalize(text, Specials);
            if (IsModulesElem) OutputText = TextHelper.ReplacePolishChars(OutputText);
            OldTranslatedText = "";

            if (withConfirm)
            {
                IsCorrectedByHuman = true;
                ConfirmationTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                ConfirmationUser = Environment.UserName;
                ConfirmationGuid = LogicProvider.UserId;
            }
        }

        public void TryUpdateValue(string text)
        {
            if (TextHelper.EqualsTexts(InputText, text))
                return;

            OldTranslatedText = TranslatedText;
            InputText = text;
            OriginalText = TextHelper.Normalize(InputText, out Specials);
            TranslatedText = OriginalText;
            OutputText = InputText;
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
