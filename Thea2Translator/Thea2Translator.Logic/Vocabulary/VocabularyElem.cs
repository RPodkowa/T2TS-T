using System;
using System.Collections.Generic;

namespace Thea2Translator.Logic
{
    public class VocabularyElem
    {
        public int UsageCountDataBase { get; private set; }
        public int UsageCountModules { get; private set; }

        public int Flag { get; private set; }
        public bool IsActive
        {
            get { return FlagHelper.IsSettedBit(Flag, 0); }
            set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
        }
        public bool HasConflict
        {
            get { return FlagHelper.IsSettedBit(Flag, 1); }
            set { Flag = FlagHelper.GetSettedBitValue(Flag, 1, value); }
        }

        public string OriginalWord;
        public string Translation;

        public VocabularyElem(string line)
        {            
            var elems = line.Split(';');

            if (elems.Length != 4 && elems.Length != 5)
                throw new Exception($"Niepoprawna linia '{line}'!");

            int elem = 0;
            UsageCountDataBase = int.Parse(elems[elem++]);
            UsageCountModules = 0;
            if (elems.Length == 5) UsageCountModules = int.Parse(elems[elem++]);
            Flag = int.Parse(elems[elem++]);
            OriginalWord = elems[elem++];
            Translation = elems[elem++];
        }

        public VocabularyElem(string originalWord, string translation)
        {
            UsageCountDataBase = 0;
            UsageCountModules = 0;
            Flag = 0;
            IsActive = true;
            OriginalWord = originalWord;
            Translation = translation;
        }

        public void ResetUsages(FilesType filesType)
        {
            switch (filesType)
            {
                case FilesType.DataBase: UsageCountDataBase = 0; break;
                case FilesType.Modules: UsageCountModules = 0; break;
            }
        }

        public void AddUsage(FilesType filesType, int usages = 1)
        {
            switch (filesType)
            {
                case FilesType.DataBase: UsageCountDataBase += usages; break;
                case FilesType.Modules: UsageCountModules += usages; break;
            }
        }

        private bool OccursInPreparedText(string text)
        {
            var word = OriginalWord.ToLower();
            return text.Contains(word);
        }

        public bool CanShowElemForPreparedText(FilesType filesType, string text)
        {
            if (!IsActive) return false;
            if (GetUsageCount(filesType) <= 1) return false;
                        
            return OccursInPreparedText(text);
        }

        public int GetUsageCount(FilesType filesType)
        {
            int usageCount = 0;
            switch (filesType)
            {
                case FilesType.DataBase: usageCount = UsageCountDataBase; break;
                case FilesType.Modules: usageCount = UsageCountModules; break;
            }

            return usageCount;
        }

        public string GetStringToSave()
        {
            var arr = new List<string>();
            arr.Add(UsageCountDataBase.ToString());
            arr.Add(UsageCountModules.ToString());
            arr.Add(Flag.ToString());
            arr.Add(OriginalWord);
            arr.Add(Translation);
            string text = string.Join(";", arr.ToArray());
            return text;
        }

        public override string ToString()
        {
            string text = $"({UsageCountDataBase},{UsageCountModules}) {OriginalWord}";
            if (!string.IsNullOrEmpty(Translation)) text = $"{text} - {Translation}";
            return text;
        }

        public static bool IsEquals(VocabularyElem elem1, VocabularyElem elem2)
        {
            if (elem1 == null && elem2 == null) return true;

            if (elem1 == null && elem2 != null) return false;
            if (elem1 != null && elem2 == null) return false;
            if (elem1.Translation != elem2.Translation) return false;
            return true;
        }

        public void SetConlfictWith(VocabularyElem elem)
        {
            HasConflict = true;
            Translation += $"###" + elem.Translation;
        }
    }
}
