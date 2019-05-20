using System;
using System.Collections.Generic;

namespace Thea2Translator.Cache
{
    public class CacheElem
    {
        public FilesType Type { get; private set; }
        public int Id { get; private set; }
        public int Flag { get; private set; }
        private string _key;
        public string Key
        {
            get
            {
                if (IsModulesElem) return OriginalText;
                return _key;
            }
            private set
            {
                _key = value;
            }
        }
        public string OriginalText { get; private set; }
        public string TranslatedText { get; private set; }

        public bool IsDataBaseElem { get { return Type == FilesType.DataBase; } }
        public bool IsModulesElem { get { return Type == FilesType.Modules; } }

        public bool ToTranslate { get { return OriginalText == TranslatedText; } }

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
                Key = elems[elem++];

            OriginalText = elems[elem++];
            TranslatedText = elems[elem++];
        }

        private bool CheckElems(string[] elems)
        {
            if (IsDataBaseElem && elems.Length != 5) return false;
            if (IsModulesElem && elems.Length != 4) return false;
            return true;
        }

        public void SetTranslated(string text)
        {
            TranslatedText = text;
        }

        public override string ToString()
        {
            var arr = new List<string>();
            arr.Add(Id.ToString());
            arr.Add(Flag.ToString());
            if (IsDataBaseElem) arr.Add(Key);
            arr.Add(OriginalText);
            arr.Add(TranslatedText);
            string text = string.Join(DataCache.DataSeparator, arr.ToArray());
            return text;
        }
    }
}
