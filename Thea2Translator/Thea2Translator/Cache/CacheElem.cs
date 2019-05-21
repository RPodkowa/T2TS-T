using System;
using System.Collections.Generic;
using Thea2Translator.Helpers;

namespace Thea2Translator.Cache
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
                OriginalNormalizedText = TextHelper.Normalize(_originalText);
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
                _translatedNormalizedText = TextHelper.Normalize(_translatedText);
            }
        }
        private string _translatedNormalizedText;
        public string TranslatedNormalizedText
        {
            get { return _translatedNormalizedText; }
            private set
            {
                _translatedNormalizedText = value;
                _translatedText = TextHelper.UnNormalize(_translatedNormalizedText, OriginalText, IsModulesElem); ;
            }
        }

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

            if (IsDataBaseElem)
            {
                Key = elems[elem++];
                OriginalText = elems[elem++];
                TranslatedText = elems[elem++];
            }
            else
            {
                OriginalNormalizedText = elems[elem++];
                TranslatedNormalizedText = elems[elem++];
            }
        }

        private bool CheckElems(string[] elems)
        {
            if (IsDataBaseElem && elems.Length != 5) return false;
            if (IsModulesElem && elems.Length != 4) return false;
            return true;
        }

        public void SetTranslated(string text)
        {
            TranslatedNormalizedText = text;
        }

        public override string ToString()
        {
            var arr = new List<string>();
            arr.Add(Id.ToString());
            arr.Add(Flag.ToString());
            if (IsDataBaseElem)
            {
                arr.Add(Key);
                arr.Add(OriginalText);
                arr.Add(TranslatedText);
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
