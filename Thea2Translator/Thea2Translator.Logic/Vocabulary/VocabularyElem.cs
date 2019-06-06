using System;
using System.Collections.Generic;

namespace Thea2Translator.Logic
{
    public class VocabularyElem
    {
        public int UsageCount { get; private set; }
        public int Flag { get; private set; }
        public bool IsActive
        {
            get { return FlagHelper.IsSettedBit(Flag, 0); }
            set { Flag = FlagHelper.GetSettedBitValue(Flag, 0, value); }
        }
        
        public string OriginalWord;
        public string Translation;

        public VocabularyElem(string line)
        {            
            var elems = line.Split(';');

            if (elems.Length != 4)
                throw new Exception($"Niepoprawna linia '{line}'!");

            int elem = 0;
            UsageCount = int.Parse(elems[elem++]);
            Flag = int.Parse(elems[elem++]);
            OriginalWord = elems[elem++];
            Translation = elems[elem++];
        }

        public VocabularyElem(string originalWord, string translation)
        {
            UsageCount = 0;
            Flag = 0;
            IsActive = true;
            OriginalWord = originalWord;
            Translation = translation;
        }

        public void ResetUsages()
        {
            UsageCount = 0;
        }

        public void AddUsage(int usages = 1)
        {
            UsageCount += usages;
        }

        public bool OccursInText(string text)
        {
            return OccursInPreparedText(TextHelper.RemoveUnnecessaryForVocabulary(text).ToLower());
        }

        public bool OccursInPreparedText(string text)
        {
            var word = OriginalWord.ToLower();
            return text.Contains(word);
        }

        public string GetStringToSave()
        {
            var arr = new List<string>();
            arr.Add(UsageCount.ToString());
            arr.Add(Flag.ToString());
            arr.Add(OriginalWord);
            arr.Add(Translation);
            string text = string.Join(";", arr.ToArray());
            return text;
        }

        public override string ToString()
        {
            string text = $"({UsageCount}) {OriginalWord}";
            if (!string.IsNullOrEmpty(Translation)) text = $"{text} - {Translation}";
            return text;
        }
    }
}
