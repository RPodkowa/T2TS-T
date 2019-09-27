using System;
using System.Collections.Generic;

namespace Thea2Translator.Logic
{
    public class VocabularyElem
    {
        public const int UsageTypesCount = 2;
        private int?[] UsageCount;

        public int Flag { get; private set; }
        public bool HasConflict
        {
            get { return FlagHelper.IsSettedBit(Flag, 0); }
            set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
        }        

        public bool NewElem;
        public bool DontSave;

        public string OriginalWord { get; set; }
        public string NormalizedOriginalWord { get; private set; }
        public string Translation;
        
        public VocabularyElem(string line)
        {
            InitUsages();
            DontSave = false;
            NewElem = false;
            var elems = line.Split(';');

            if (elems.Length != 4)
                throw new Exception($"Niepoprawna linia '{line}'!");

            int elem = 0;
            ParseUsages(elems[elem++]);
            Flag = int.Parse(elems[elem++]);
            OriginalWord = elems[elem++];
            Translation = elems[elem++];

            NormalizedOriginalWord = TextHelper.NormalizeForVocabulary(OriginalWord);
        }

        public VocabularyElem(string originalWord, string translation)
        {
            InitUsages();
            DontSave = false;
            NewElem = false;
            Flag = 0;
            OriginalWord = originalWord;
            Translation = translation;

            NormalizedOriginalWord = TextHelper.NormalizeForVocabulary(OriginalWord);
        }

        private void InitUsages()
        {
            UsageCount = new int?[UsageTypesCount] { null, null };
        }

        public void ResetUsages(FilesType filesType)
        {
            TryResetUsages(GetUsagesIndex(filesType), false);
        }

        public void ForceResetUsages(FilesType filesType)
        {
            TryResetUsages(GetUsagesIndex(filesType), true);
        }

        public bool IsUnknown(FilesType filesType)
        {
            var usagesIndex = GetUsagesIndex(filesType);
            if (!CheckUsagesIndex(usagesIndex)) return false;

            var usageCount = UsageCount[usagesIndex.Value];
            return (usageCount == null);
        }

        private bool CheckUsagesIndex(int? usagesIndex)
        {
            if (usagesIndex == null) return false;
            if (UsageCount.Length <= usagesIndex) return false;
            return true;
        }

        private void TryResetUsages(int? usagesIndex, bool force)
        {
            if (!CheckUsagesIndex(usagesIndex)) return;
            if (UsageCount[usagesIndex.Value] == null && !force) return;
            UsageCount[usagesIndex.Value] = 0;
        }

        private void TryAddUsages(int? usagesIndex, int usages)
        {
            if (!CheckUsagesIndex(usagesIndex)) return;

            if (UsageCount[usagesIndex.Value] == null) UsageCount[usagesIndex.Value] = 0;
            UsageCount[usagesIndex.Value] += usages;
        }

        public int GetUsageCount(FilesType filesType)
        {
            return GetUsageCount(GetUsagesIndex(filesType));
        }

        private int GetUsageCount(int? usagesIndex)
        {
            if (!CheckUsagesIndex(usagesIndex)) return 0;

            var usageCount = UsageCount[usagesIndex.Value];
            if (usageCount == null) usageCount = 0;
            return usageCount.Value;
        }

        private string GetUsageValue(int? usagesIndex)
        {
            if (!CheckUsagesIndex(usagesIndex)) return "-";

            var usageCount = UsageCount[usagesIndex.Value];
            if (usageCount == null) return "?";
            return usageCount.Value.ToString();
        }

        private int? GetUsagesIndex(FilesType filesType)
        {
            switch (filesType)
            {
                case FilesType.DataBase: return 0;
                case FilesType.Modules: return 1;
            }

            return null;
        }

        private void ParseUsages(string usages)
        {
            var elems = usages.Split(',');
            for (int i = 0; i < elems.Length; i++)
            {
                if (UsageCount.Length <= i) break;

                int usage;
                if (int.TryParse(elems[i],out usage))
                    UsageCount[i] = usage;
            }
        }

        public string GetUsagesText()
        {
            if (UsageCount == null) InitUsages();

            var ret = new List<string>();

            ret.Add($"Database: {GetUsageValue(0)}");
            ret.Add($"Modules: {GetUsageValue(1)}");

            return string.Join(" ", ret.ToArray());
        }

        private string GetUsages()
        {
            if (UsageCount == null) InitUsages();
            var ret = new List<string>();

            for (int i = 0; i < UsageCount.Length; i++)
            {
                ret.Add(GetUsageValue(i));
            }

            return string.Join(",", ret.ToArray());
        }

        public void CalculateUsages()
        {
            CalculateUsages(LogicProvider.DataBase);
            CalculateUsages(LogicProvider.Modules);
        }

        private void CalculateUsages(IDataCache dataCache)
        {
            ResetUsages(dataCache.GetFileType());

            NormalizedOriginalWord = TextHelper.NormalizeForVocabulary(OriginalWord);

            foreach (var cacheElem in dataCache.CacheElems)
            {
                var normalizedTxt = TextHelper.NormalizeForVocabulary(cacheElem.OriginalText);
                TryAddUsagesByNormalizedText(normalizedTxt, dataCache.GetFileType());
            }
        }

        public void TryAddUsagesByNormalizedText(string normalizedTxt, FilesType filesType)
        {
            var occurens = TextHelper.StringOccurens(normalizedTxt, NormalizedOriginalWord);
            if (occurens > 0)
                AddUsage(filesType, occurens);
        }

        private void AddUsage(FilesType filesType, int usages = 1)
        {
            TryAddUsages(GetUsagesIndex(filesType), usages);
        }

        private bool OccursInPreparedText(string text)
        {
            return text.Contains(NormalizedOriginalWord);
        }

        public bool CanShowElemForPreparedText(FilesType filesType, string text)
        {                        
            return OccursInPreparedText(text);
        }

        public string GetStringToSave()
        {
            var arr = new List<string>();
            arr.Add(GetUsages());
            arr.Add(Flag.ToString());
            arr.Add(OriginalWord);
            arr.Add(Translation);
            string text = string.Join(";", arr.ToArray());
            return text;
        }

        public override string ToString()
        {
            string text = "";
            if (DontSave) text += "🚫";
            if (HasConflict) text += "⚠️";
            text += $"({GetUsages()}) {OriginalWord}";            
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
