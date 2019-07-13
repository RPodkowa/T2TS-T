using System;
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

        public void Reload(IDataCache dataCache, DirectoryType directoryType = DirectoryType.Cache)
        {
            ReadFromFile(directoryType);

            if (VocabularyElems == null)
                VocabularyElems = new List<VocabularyElem>();

            VocabularyElems = ((List<VocabularyElem>)VocabularyElems).OrderByDescending(x => x.GetUsageCount(Type)).ToList();
        }

        public void ReadFromFile(DirectoryType directoryType)
        {
            if (VocabularyElems == null)
                VocabularyElems = new List<VocabularyElem>();

            var fullPath = FileHelper.GetLocalFilePatch(directoryType, FilesType.Vocabulary);
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
            var fullPath = FileHelper.GetLocalFilePatch(DirectoryType.Cache, FilesType.Vocabulary);
            VocabularyElems = ((List<VocabularyElem>)VocabularyElems).OrderBy(x => x.OriginalWord).ToList();
            FileHelper.SaveElemsToFile(VocabularyElems, fullPath);
        }

        public void UpdateByCache(IDataCache dataCache)
        {
            if (VocabularyElems == null)
                VocabularyElems = new List<VocabularyElem>();

            dataCache.ReloadElems(false, false);

            foreach (var elem in VocabularyElems)
            {
                elem.ResetUsages(dataCache.GetFileType());
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
                    VocabularyElem.AddUsage(dataCache.GetFileType());
                }
            }

            SaveElems();
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

        public static bool HasConflicts()
        {
            var vocabulary = new Vocabulary(FilesType.Vocabulary);
            vocabulary.ReadFromFile(DirectoryType.Cache);
            foreach (var elem in vocabulary.VocabularyElems)
            {
                if (elem.HasConflict) return true;
            }

            return false;
        }

        protected void AddElem(VocabularyElem elem)
        {
            if (VocabularyElems == null)
                VocabularyElems = new List<VocabularyElem>();

            VocabularyElems.Add(elem);
        }

        protected void RemoveElem(VocabularyElem elem)
        {
            if (elem == null)
                return;

            if (VocabularyElems == null)
                VocabularyElems = new List<VocabularyElem>();

            VocabularyElems.Remove(elem);
        }

        public static void MergeCache()
        {
            var original = new Vocabulary(FilesType.Vocabulary);
            var originalOld = new Vocabulary(FilesType.Vocabulary);
            var cacheOld = new Vocabulary(FilesType.Vocabulary);

            original.ReadFromFile(DirectoryType.Original);
            originalOld.ReadFromFile(DirectoryType.OriginalOld);
            cacheOld.ReadFromFile(DirectoryType.CacheOld);

            var cacheNew = new Vocabulary(FilesType.Vocabulary);

            //-----------------------------------
            //  	O   |   OO	|	CO	|	CN	|
            //-----------------------------------
            //0. 	A	|	-	|	-	|	A	|
            //1.	A	|	A	|	A	|	A	|
            //2.	A	|	A	|	B	|	B	|
            //3.	A	|	B	|	B	|	A	|
            //4.	A	|	B	|	A	|	A	|
            //5.    A   |   B   |   C   |   ?   |
            //-----------------------------------

            foreach (var originalElem in original.VocabularyElems)
            {
                var word = originalElem.OriginalWord;
                var originalOldElem = originalOld.GetElem(word, false);
                var cacheOldElem = cacheOld.GetElem(word, false);

                if ((originalOldElem == null && cacheOldElem != null) || (cacheOldElem == null && originalOldElem != null))
                    throw new Exception($"Cos nie tak ze slowem='{word}'");
                
                //0. 	A	|	-	|	-	|	A	|
                if (originalOldElem == null && cacheOldElem == null)
                {
                    cacheNew.AddElem(originalElem);
                    continue;
                }

                //1.	A	|	A	|	A	|	A	|
                //2.	A	|	A	|	B	|	B	|
                if (VocabularyElem.IsEquals(originalElem, originalOldElem))
                {
                    cacheNew.AddElem(cacheOldElem);
                    originalOld.RemoveElem(originalOldElem);
                    cacheOld.RemoveElem(cacheOldElem);
                    continue;
                }

                //3.	A	|	B	|	B	|	A	|
                // !VocabularyElem.IsEquals(originalElem, originalOldElem)
                if (VocabularyElem.IsEquals(originalOldElem, cacheOldElem))
                {
                    cacheNew.AddElem(originalElem);
                    originalOld.RemoveElem(originalOldElem);
                    cacheOld.RemoveElem(cacheOldElem);
                    continue;
                }

                //4.	A	|	B	|	A	|	A	|
                // !VocabularyElem.IsEquals(originalElem, originalOldElem)
                // !VocabularyElem.IsEquals(originalOldElem, cacheOldElem)
                if (VocabularyElem.IsEquals(originalElem, cacheOldElem))
                {
                    cacheNew.AddElem(originalElem);
                    originalOld.RemoveElem(originalOldElem);
                    cacheOld.RemoveElem(cacheOldElem);
                    continue;
                }

                //5.    A   |   B   |   C   |   ?   |
                // !VocabularyElem.IsEquals(originalElem, originalOldElem)
                // !VocabularyElem.IsEquals(originalOldElem, cacheOldElem)
                // !VocabularyElem.IsEquals(originalElem, cacheOldElem))
                originalElem.SetConlfictWith(cacheOldElem);
                cacheNew.AddElem(originalElem);
                originalOld.RemoveElem(originalOldElem);
                cacheOld.RemoveElem(cacheOldElem);
            }

            foreach (var oldElem in cacheOld.VocabularyElems)
            {
                var word = oldElem.OriginalWord;
                var originalOldElem = originalOld.GetElem(word, false);

                if (originalOldElem != null)
                    throw new Exception($"Cos nie tak ze slowem='{word}' (petla 2)");

                cacheNew.AddElem(oldElem);
            }

            cacheNew.SaveElems();
        }
    }
}
