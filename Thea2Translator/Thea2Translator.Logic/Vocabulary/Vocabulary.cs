using System.Collections.Generic;
using System.Linq;

namespace Thea2Translator.Logic
{
    public class Vocabulary
    {
        public IList<VocabularyElem> VocabularyElems { get; private set; }

        private readonly FilesType Type;

        public Vocabulary(FilesType type)
        {
            Type = type;
        }

        public void Reload(IDataCache dataCache)
        {
            ReadFromFile();
            UpdateByCache(dataCache);
            VocabularyElems = ((List<VocabularyElem>)VocabularyElems).OrderByDescending(x => x.GetUsageCount(Type)).ToList();
        }

        private void ReadFromFile()
        {
            if (VocabularyElems == null)
                VocabularyElems = new List<VocabularyElem>();

            var fullPath = $"{FileHelper.MainDir}\\Cache\\Vocabulary.cache";
            var lines = FileHelper.ReadFileLines(fullPath);
            foreach (var line in lines)
            {
                LoadElem(line);
            }
        }

        private void LoadElem(string line)
        {
            var elem = new VocabularyElem(line);
            VocabularyElems.Add(elem);
        }

        public void SaveElems()
        {
            var fullPath = $"{FileHelper.MainDir}\\Cache\\Vocabulary.cache";
            VocabularyElems = ((List<VocabularyElem>)VocabularyElems).OrderBy(x => x.OriginalWord).ToList();
            FileHelper.SaveElemsToFile(VocabularyElems, fullPath);
        }

        private void UpdateByCache(IDataCache dataCache)
        {
            if (VocabularyElems == null)
                VocabularyElems = new List<VocabularyElem>();

            foreach (var elem in VocabularyElems)
            {
                elem.ResetUsages(Type);
            }

            foreach (var cacheElem in dataCache.CacheElems)
            {
                var txt = cacheElem.OriginalText;

                var elems = txt.Split(' ');
                foreach (var elem in elems)
                {
                    var word = TextHelper.NormalizeForVocabulary(elem);
                    if (string.IsNullOrEmpty(word)) continue;
                    if (word.Length == 1) continue;

                    var VocabularyElem = GetElem(word, true);
                    VocabularyElem.AddUsage(Type);
                }
            }
        }

        public IList<VocabularyElem> GetElemsForText(string text)
        {
            text = TextHelper.RemoveUnnecessaryForVocabulary(text).ToLower();

            var elems = VocabularyElems.Where(x => x.CanShowElemForPreparedText(Type, text)).OrderByDescending(x => x.GetUsageCount(Type));

            return elems.ToList();
        }

        private bool Contains(string word)
        {
            return (GetElem(word, false) != null);
        }

        private VocabularyElem GetElem(string word, bool withCreate)
        {
            foreach (var elem in VocabularyElems)
            {
                if (elem.OriginalWord == word)
                    return elem;
            }

            if (!withCreate)
                return null;

            VocabularyElem ret = new VocabularyElem(word, "");
            VocabularyElems.Add(ret);
            return ret;
        }
    }
}
