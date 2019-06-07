using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thea2Translator.Logic.Cache.Interfaces;

namespace Thea2Translator.Logic.Cache
{
    class Statistic:IStatistic
    {
        public int AllItemsCount { get; private set; }
        public int TranslatedItemsCount { get; private set; }
        public int ItemWithoutTranslationCount { get; private set; }
        public int TranslatedPercent { get; private set; }

        public void Reload(IDataCache dataCache)
        {
            var allElements = dataCache.CacheElems;
            AllItemsCount = allElements.Count;
            TranslatedItemsCount = allElements.Count(e => !e.ToTranslate);
            ItemWithoutTranslationCount = allElements.Count(e => e.ToTranslate);

            TranslatedPercent = (int)(((double)TranslatedItemsCount / (double)AllItemsCount) * 100);
        }
    }
}
