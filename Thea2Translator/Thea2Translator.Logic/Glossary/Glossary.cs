using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thea2Translator.Logic.Cache.Interfaces;
using Thea2Translator.Logic.Helpers;

namespace Thea2Translator.Logic.Glossary
{
    public class Glossary
    {
        public IList<GlossaryElem> GlossaryElems { get; private set; }
        
        public void ReadFromFile()
        {

        }

        public void UpdateByCache(IDataCache dataCache)
        {
            if (GlossaryElems == null)
                GlossaryElems = new List<GlossaryElem>();

            foreach (var cacheElem in dataCache.CacheElems)
            {
                var txt = cacheElem.OriginalNormalizedText;

                var elems = txt.Split(' ');
                foreach (var elem in elems)
                {
                    var word = TextHelper.NormalizeForGlossary(elem);
                    if (string.IsNullOrEmpty(word)) continue;
                    if (word.Length == 1) continue;

                    var glossaryElem = GetElem(word, true);
                    glossaryElem.AddUsage();
                }
            }
        }

        public IList<GlossaryElem> GetElemsForText(string text)
        {
            text = TextHelper.RemoveUnnecessaryForGlossary(text).ToLower();

            var elems = GlossaryElems.Where(x => x.OccursInPreparedText(text)).OrderByDescending(x => x.UsageCount);

            return elems.ToList();
        }

        private bool Contains(string word)
        {
            return (GetElem(word, false) != null);
        }

        private GlossaryElem GetElem(string word, bool withCreate)
        {
            foreach (var elem in GlossaryElems)
            {
                if (elem.OriginalWord == word) return elem;
            }

            if (!withCreate)
                return null;

            GlossaryElem ret = new GlossaryElem(word, "");
            GlossaryElems.Add(ret);
            return ret;
        }
    }
}
