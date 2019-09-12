using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class CacheElem
    {
        public static string ConflictSeparator = "\r\n===============\r\n";

        public FilesType Type { get; private set; }
        public int Id { get; private set; }
        public string StatusString { get; private set; }

        public int Flag { get; private set; }
        public bool IsCorrectedByHuman { get { return FlagHelper.IsSettedBit(Flag, 0); } private set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); } }
        public bool HasConflict { get { return FlagHelper.IsSettedBit(Flag, 1); } private set { Flag = FlagHelper.GetSettedBitValue(Flag, 1, value); } }
        public bool IsGenericName { get { return FlagHelper.IsSettedBit(Flag, 2); } private set { Flag = FlagHelper.GetSettedBitValue(Flag, 2, value); } }
        public bool IsInactive { get { return FlagHelper.IsSettedBit(Flag, 3); } private set { Flag = FlagHelper.GetSettedBitValue(Flag, 3, value); } }
        public bool IsActive { get { return !IsInactive; } private set { IsInactive = !value; } }
        public bool IsMale { get { return FlagHelper.IsSettedBit(Flag, 4); } private set { Flag = FlagHelper.GetSettedBitValue(Flag, 4, value); } }
        public bool IsFemale { get { return FlagHelper.IsSettedBit(Flag, 5); } private set { Flag = FlagHelper.GetSettedBitValue(Flag, 5, value); } }

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
        /// <summary>
        /// Konfliktowy tekst
        /// Powstaje przy mergowaniu w momencie powstania konfliktu
        /// </summary>
        public string ConflictTranslatedText { get; private set; }

        public string ConfirmationUser { get; private set; }

        public DateTime? ConfirmationTime { get; private set; }
        private string ConfirmationGuid;

        public List<string> Groups;
        private List<string> Specials;

        public bool IsDataBaseElem { get { return Type == FilesType.DataBase; } }
        public bool IsModulesElem { get { return Type == FilesType.Modules; } }
        public bool IsNamesElem { get { return Type == FilesType.Names; } }
        public bool IsDescription
        {
            get
            {
                if (!IsDataBaseElem) return false;
                string lastPart = Key.Substring(Key.Length - 4);
                string lastPartWithNumber = Key.Substring(Key.Length - 5, 4);
                return (lastPart == "_DES" || lastPartWithNumber == "_DES");
            }
        }
        public bool Changed { get; private set; }
        public bool HasMark { get; private set; }

        public bool ToTranslate { get { return !IsCorrectedByHuman && TextHelper.EqualsTexts(OriginalText, TranslatedText); } }
        public bool ToConfirm { get { return !IsCorrectedByHuman; } }

        //Nawigacyjne
        private bool? isInStartingGroup;
        public bool IsAdventureNodeRecord { get; private set; }
        public List<string> AdventureNodeGroups;
        public IList<NavigationNextAdventureElem> NavigationNextAdventureElems { get; private set; }

        private string ToCompareText = null;

        public CacheElem(FilesType type, int id, string key, string inputText)
        {
            Type = type;
            if (type != FilesType.Names) Id = id;
            Flag = 0;
            Key = key;
            if (IsModulesElem) Key = "";
            InputText = inputText;
            OriginalText = TextHelper.Normalize(InputText, out Specials);
            TranslatedText = OriginalText;
            OutputText = InputText;
            Groups = new List<string>();
            RefreshStatusString();
        }

        public CacheElem(string collection, string race, List<string> subraces, string gender, string name)
        {
            Type = FilesType.Names;
            Flag = 0;
            IsCorrectedByHuman = true;
            IsGenericName = true;
            SetGender(gender);
            Key = GetNameKey(collection, race, subraces, gender, name);
            InputText = name;
            OriginalText = name;
            TranslatedText = name;
            OutputText = name;
            Groups = GetNameGroups(collection, race, subraces, gender);
            RefreshStatusString();
        }

        public CacheElem(FilesType type, XmlNode element, IList<string> bookmarks)
        {
            Type = type;

            if (element.Attributes != null)
            {
                if (type != FilesType.Names) Id = int.Parse(element.Attributes["ID"]?.Value);
                Key = element.Attributes["Key"]?.Value.ToString();
                Flag = int.Parse(element.Attributes["Flag"]?.Value);
            }

            Groups = new List<string>();

            var groups = element.SelectNodes("Groups/Group");
            foreach (XmlNode group in groups)
            {
                AddGroup(group.InnerText);
            }

            InputText = XmlHelper.GetNodeText(element, "Texts/Input");
            OutputText = XmlHelper.GetNodeText(element, "Texts/Output");

            List<string> tmpSpecials = null;
            OriginalText = TextHelper.Normalize(InputText, out Specials);
            TranslatedText = TextHelper.Normalize(OutputText, out tmpSpecials);

            OldTranslatedText = XmlHelper.GetNodeText(element, "Texts/Old");
            ConflictTranslatedText = XmlHelper.GetNodeText(element, "Texts/Conflict");

            ConfirmationTime = XmlHelper.GetNodeDate(element, "Confirmation/Time");
            ConfirmationGuid = XmlHelper.GetNodeText(element, "Confirmation/GUID");
            ConfirmationUser = XmlHelper.GetNodeText(element, "Confirmation/User");

            if (IsModulesElem && !TextHelper.IsEqualsSpecials(Specials, tmpSpecials))
            {
                HasConflict = true;
                ConflictTranslatedText = TranslatedText;
            }

            if (bookmarks != null && bookmarks.Count > 0 && bookmarks.Contains(Id.ToString()))
                HasMark = true;

            RefreshStatusString();
        }

        private void RefreshStatusString()
        {
            StatusString = GetCommonStatusString();
        }

        public bool WithConflictText()
        {
            return !string.IsNullOrEmpty(ConflictTranslatedText);
        }

        public void ResolveConflict(bool resolved, string text)
        {
            HasConflict = !resolved;
            if (resolved)
                SetTranslated(text);
        }

        public void SetConlfictWith(CacheElem elem)
        {
            HasConflict = true;
            ConflictTranslatedText = elem.TranslatedText;
        }

        public bool EqualsPreparedTexts(string preparedInputText)
        {
            bool ret = GetTextToCompare() == preparedInputText;
            if (ret) SetNewTextToCompare(preparedInputText);
            return ret;
        }

        private string GetTextToCompare()
        {
            if (!string.IsNullOrEmpty(ToCompareText)) return ToCompareText;
            ToCompareText = InputText;
            if (IsDataBaseElem || IsNamesElem) ToCompareText = Key;
            ToCompareText = TextHelper.PrepereToCompare(ToCompareText);
            return ToCompareText;
        }

        private void SetNewTextToCompare(string newText)
        {
            if (IsDataBaseElem || IsNamesElem) return;
            //InputText = newText;
            ToCompareText = newText;
        }

        public XmlNode ToXmlNode(XmlDocument doc)
        {
            XmlNode elementNode = doc.CreateElement("Element");
            if (Type != FilesType.Names)
                elementNode.Attributes.Append(XmlHelper.GetAttribute(doc, "ID", Id.ToString()));
            elementNode.Attributes.Append(XmlHelper.GetAttribute(doc, "Key", Key));
            elementNode.Attributes.Append(XmlHelper.GetAttribute(doc, "Flag", Flag.ToString()));

            XmlNode confirmationNode = doc.CreateElement("Confirmation");
            if (ConfirmationTime.HasValue) confirmationNode.AppendChild(XmlHelper.GetNode(doc, "Time", ConfirmationTime.Value));
            if (!string.IsNullOrEmpty(ConfirmationGuid)) confirmationNode.AppendChild(XmlHelper.GetNode(doc, "GUID", ConfirmationGuid));
            if (!string.IsNullOrEmpty(ConfirmationUser)) confirmationNode.AppendChild(XmlHelper.GetNode(doc, "User", ConfirmationUser));
            if (confirmationNode.ChildNodes != null && confirmationNode.ChildNodes.Count > 0)
                elementNode.AppendChild(confirmationNode);

            XmlNode groupsNode = doc.CreateElement("Groups");
            foreach (var group in Groups)
            {
                groupsNode.AppendChild(XmlHelper.GetNode(doc, "Group", group));
            }
            elementNode.AppendChild(groupsNode);

            XmlNode textsNode = doc.CreateElement("Texts");
            textsNode.AppendChild(XmlHelper.GetNode(doc, "Input", InputText));
            textsNode.AppendChild(XmlHelper.GetNode(doc, "Output", OutputText));
            if (!string.IsNullOrEmpty(OldTranslatedText)) textsNode.AppendChild(XmlHelper.GetNode(doc, "Old", OldTranslatedText));
            if (HasConflict) textsNode.AppendChild(XmlHelper.GetNode(doc, "Conflict", ConflictTranslatedText));
            elementNode.AppendChild(textsNode);

            return elementNode;
        }

        public void SetTranslated(string text, bool withChanged = false)
        {
            string conflictText = "";
            if (HasConflict)
            {
                var textList = text.Split(new string[] { ConflictSeparator }, StringSplitOptions.None);
                if (textList.Length > 1)
                {
                    text = textList[0];
                    conflictText = textList[1];
                }
            }

            if (withChanged) withChanged = TranslatedText != text;
            TranslatedText = text;
            if (!string.IsNullOrEmpty(conflictText)) ConflictTranslatedText = conflictText;
            OutputText = TextHelper.UnNormalize(text, Specials);
            OldTranslatedText = "";
            if (withChanged) SetChanged(true);
            else RefreshStatusString();
        }

        public void SetActivation(bool how) { IsActive = how; }
        public void SetConfirmedtion(bool how) { IsCorrectedByHuman = how; }
        public void SetGender(string gender)
        {
            IsMale = (gender == NameGeneratorElem.MaleString);
            IsFemale = (gender == NameGeneratorElem.FemaleString);
        }

        public void SetChanged(bool how)
        {
            Changed = how;
            RefreshStatusString();
        }

        public void ToggleBookmark()
        {
            HasMark = !HasMark;
            RefreshStatusString();
        }

        private string GetCommonStatusString()
        {
            var status = string.Empty;
            if (Changed) status += "🖊️";
            if (HasMark) status += "📌";
            if (IsInactive) status += "🚫";
            if (HasConflict) status += "⚠️";
            if (IsGenericName) status += "⚙️";
            if (IsMale) status += "♂️";
            if (IsFemale) status += "♀️";
            if (IsDescription) status += "📖";            
            return status;
        }

        public void SetConfirmation(bool confirm)
        {
            if (confirm)
            {
                IsCorrectedByHuman = true;
                ConfirmationTime = DateTime.Now;
                ConfirmationUser = UserHelper.UserName;
                ConfirmationGuid = UserHelper.UserId;
            }
            else
            {
                IsCorrectedByHuman = false;
                ConfirmationTime = null;
                ConfirmationUser = "";
                ConfirmationGuid = "";
            }

            RefreshStatusString();
        }

        public void ChangeConfirmation()
        {
            SetConfirmation(!IsCorrectedByHuman);
        }

        public void TryUpdateValue(string text, bool withActivation)
        {
            if (withActivation) IsActive = true;

            if (TextHelper.EqualsTexts(InputText, text))
                return;

            //Mam roznice! a juz zatwierdzilem
            if (IsCorrectedByHuman)
            {
                HasConflict = true;
                ConflictTranslatedText = TranslatedText;
            }

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

        public static bool IsEquals(CacheElem elem1, CacheElem elem2)
        {
            if (elem1 == null && elem2 == null) return true;

            if (elem1 == null && elem2 != null) return false;
            if (elem1 != null && elem2 == null) return false;
            if (elem1.IsCorrectedByHuman != elem2.IsCorrectedByHuman) return false;
            if (elem1.TranslatedText != elem2.TranslatedText) return false;
            return true;
        }

        public bool OccursInStartingGroup(IList<string> startingGroups)
        {
            if (isInStartingGroup.HasValue) return isInStartingGroup.Value;

            isInStartingGroup = false;
            foreach (var startingGroup in startingGroups)
            {
                if (Groups.Contains(startingGroup))
                {
                    isInStartingGroup = true;
                    break;
                }
            }

            return isInStartingGroup.Value;
        }

        public void AddAdventureNodeGroup(string group)
        {
            if (AdventureNodeGroups == null) AdventureNodeGroups = new List<string>();
            AdventureNodeGroups.Add(group);
        }

        public void SetGroupContext(List<string> groups)
        {
            IsAdventureNodeRecord = false;

            if (AdventureNodeGroups == null)
                return;

            foreach (var adventureNodeGroup in AdventureNodeGroups)
            {
                if (groups.Contains(adventureNodeGroup))
                {
                    IsAdventureNodeRecord = true;
                    return;
                }
            }
        }

        public void ResetAdventureNodeRecord()
        {
            IsAdventureNodeRecord = false;
        }

        public IList<NavigationElemRelation> GetNextElemRelations(IDataCache dataCache, string actualGroup)
        {
            if (NavigationNextAdventureElems == null)
            {
                NavigationNextAdventureElems = new List<NavigationNextAdventureElem>();
                if (dataCache.Navigation == null) return new List<NavigationElemRelation>();
                dataCache.Navigation.UpdateNextAdventureElems(this);
            }

            return GetNextElemRelations(actualGroup);
        }

        private IList<NavigationElemRelation> GetNextElemRelations(string actualGroup)
        {
            var ret = new List<NavigationElemRelation>();

            if (NavigationNextAdventureElems == null)
                return ret;

            foreach (var navigationNextAdventureElem in NavigationNextAdventureElems)
            {
                if (navigationNextAdventureElem.Group != actualGroup)
                    continue;

                ret.AddRange(navigationNextAdventureElem.Relations);
            }

            return ret;
        }

        public void AddNavigationNextAdventureElem(NavigationNextAdventureElem navigationNextAdventureElem)
        {
            NavigationNextAdventureElems.Add(navigationNextAdventureElem);
        }

        public static string GetNameKey(string collection, string race, List<string> subraces, string gender, string name)
        {
            var ret = $"{collection}:{race}:";
            if (subraces != null && subraces.Count > 0) ret += $"{string.Join(",", subraces.ToArray())}";
            ret += $":{gender}:{name}";
            return ret;
        }

        public static List<string> GetNameGroups(string collection, string race, List<string> subraces, string gender)
        {
            var ret = new List<string>();
            ret.Add(collection);
            if (!string.IsNullOrEmpty(race))
            {
                ret.Add(race);
                ret.Add($"{race}_{gender}");
            }

            if (subraces != null)
            {
                foreach (var subrace in subraces)
                {
                    if (string.IsNullOrEmpty(subrace))
                        continue;

                    ret.Add(subrace);
                    ret.Add($"{subrace}_{gender}");
                }
            }

            return ret;
        }

        public string GetElemInfoString()
        {
            if (IsDataBaseElem) return GetDataBaseElemInfoString();
            if (IsModulesElem) return GetModulesElemInfoString();
            if (IsNamesElem) return GetNamesElemInfoString();

            return "???";
        }

        private string GetDataBaseElemInfoString()
        {
            var stringList = new List<string>();

            stringList.Add($"ID ='{Id}' Key='{Key}'");
            stringList.Add($"Status='{StatusString}'");
            stringList.Add(GetConfirmationInfoString());

            return string.Join("\r\n", stringList.ToArray());
        }

        private string GetModulesElemInfoString()
        {
            var stringList = new List<string>();
            stringList.Add($"ID ='{Id}'");
            stringList.Add(GetGroupsInfoString(6));
            stringList.Add(GetConfirmationInfoString());
            return string.Join("\r\n", stringList.ToArray());
        }

        private string GetNamesElemInfoString()
        {
            var stringList = new List<string>();
            stringList.Add($"Key='{Key}'");
            stringList.Add($"Status='{StatusString}'");
            stringList.Add(GetGroupsInfoString(10));
            stringList.Add(GetConfirmationInfoString());
            return string.Join("\r\n", stringList.ToArray());
        }

        private string GetConfirmationInfoString()
        {
            var stringList = new List<string>();

            if (!IsCorrectedByHuman)
            {
                stringList.Add("Niezatwierdzone");
            }
            else
            {
                stringList.Add($"Zatwierdzono: '{ConfirmationTime}'");
                stringList.Add($"Zatwierdzający: '{ConfirmationUser}' ({ConfirmationGuid})");
            }

            return string.Join("\r\n", stringList.ToArray());
        }

        private string GetGroupsInfoString(int limit)
        {
            if (Groups == null)
                return "Błąd: Brak grup";

            var stringList = new List<string>();

            stringList.Add($"Występowanie: {Groups.Count} grup");
            for (int i = 0; i < Groups.Count && i < limit; i++)
            {
                stringList.Add($"\tGrupa[{i}]: '{Groups[i]}'");
            }

            return string.Join("\r\n", stringList.ToArray());
        }
    }
}
