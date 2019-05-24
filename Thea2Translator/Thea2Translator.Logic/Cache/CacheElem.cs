using System;
using System.Collections.Generic;
using System.Linq;

namespace Thea2Translator.Logic
{
    public class CacheElem
    {
        internal const char GroupSeparator = ';';

        public FilesType Type { get; private set; }
        public int Id { get; private set; }

        public int Flag { get; private set; }
        public bool IsCorrectedByHuman
        {
            get { return FlagHelper.IsSettedBit(Flag, 0); }
            set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
        }

        private string _key;
        public string Key
        {
            get
            {
                if (IsModulesElem) return OriginalNormalizedText;
                return _key;
            }
            private set
            {
                _key = value;
            }
        }
        private string _originalText;
        public string OriginalText
        {
            get
            {
                if (IsModulesElem) return OriginalNormalizedText;
                return _originalText;
            }
            private set
            {
                _originalText = value;
                OriginalNormalizedText = TextHelper.Normalize(_originalText, IsModulesElem);
            }
        }
        public string OriginalNormalizedText { get; private set; }
        private string _translatedText;
        public string TranslatedText
        {
            get { return _translatedText; }
            private set
            {
                _translatedText = value;
                _translatedNormalizedText = TextHelper.Normalize(_translatedText, IsModulesElem);
            }
        }
        private string _translatedNormalizedText;
        public string TranslatedNormalizedText
        {
            get { return _translatedNormalizedText; }
            private set
            {
                _translatedNormalizedText = value;
                _translatedText = TextHelper.UnNormalize(_translatedNormalizedText, OriginalText, IsModulesElem, IsModulesElem); ;
            }
        }

        public List<string> Groups;

        public bool IsDataBaseElem { get { return Type == FilesType.DataBase; } }
        public bool IsModulesElem { get { return Type == FilesType.Modules; } }

        public bool ToTranslate { get { return OriginalNormalizedText == TranslatedNormalizedText; } }

        public CacheElem(FilesType type, int id, string key, string originalText)
        {
            Type = type;
            Id = id;
            Flag = 0;
            Key = key;
            OriginalText = originalText;
            TranslatedText = originalText;
            Groups = new List<string>();
        }

        public CacheElem(FilesType type, string line)
        {
            Type = type;
            var separator = new[] { DataCache.DataSeparator };
            var elems = line.Split(separator, StringSplitOptions.None);

            if (!CheckElems(elems))
                throw new Exception($"Niepoprawna linia '{line}'!");

            int elem = 0;
            Id = int.Parse(elems[elem++]);
            Flag = int.Parse(elems[elem++]);
            var groups = elems[elem++];

            if (IsDataBaseElem)
            {
                Key = elems[elem++];
                //OriginalText = elems[elem++];
                //TranslatedText = elems[elem++];
                OriginalNormalizedText = elems[elem++];
                _originalText = OriginalNormalizedText;
                TranslatedNormalizedText = elems[elem++];
            }
            else
            {
                OriginalNormalizedText = elems[elem++];
                TranslatedNormalizedText = elems[elem++];
            }

            Groups = groups.Split(GroupSeparator).ToList();
            if (Groups == null) Groups = new List<string>();
        }

        private bool CheckElems(string[] elems)
        {
            if (IsDataBaseElem && elems.Length != 6) return false;
            if (IsModulesElem && elems.Length != 5) return false;
            return true;
        }

        public void SetTranslated(string text)
        {
            TranslatedNormalizedText = text;
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

        public override string ToString()
        {
            var arr = new List<string>();
            arr.Add(Id.ToString());
            arr.Add(Flag.ToString());
            arr.Add(string.Join(GroupSeparator.ToString(), Groups.ToArray()));
            if (IsDataBaseElem)
            {
                arr.Add(Key);
                //arr.Add(OriginalText);
                //arr.Add(TranslatedText);
                arr.Add(OriginalNormalizedText);
                arr.Add(TranslatedNormalizedText);
            }
            else
            {
                arr.Add(OriginalNormalizedText);
                arr.Add(TranslatedNormalizedText);
            }
            string text = string.Join(DataCache.DataSeparator, arr.ToArray());
            return text;
        }
    }
}
