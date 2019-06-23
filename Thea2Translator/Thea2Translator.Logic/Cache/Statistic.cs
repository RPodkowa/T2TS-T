using System.Linq;
using Thea2Translator.Logic.Cache.Interfaces;

namespace Thea2Translator.Logic.Cache
{
    class Statistic : IStatistic
    {
        public int AllItemsCount { get; private set; }
        public int TranslatedItemsCount { get; private set; }
        public int ItemWithoutTranslationCount { get; private set; }
        public int TranslatedPercent { get; private set; }
        public int ConfirmedItemsCount { get; private set; }
        public int ItemWithoutConfirmationCount { get; private set; }
        public int ConfirmedPercent { get; private set; }

        public void Reload(IDataCache dataCache)
        {
            var allElements = dataCache.CacheElems;
            AllItemsCount = allElements.Count;

            TranslatedItemsCount = allElements.Count(e => !e.ToTranslate);
            ItemWithoutTranslationCount = AllItemsCount - TranslatedItemsCount;
            TranslatedPercent = (int)(((double)TranslatedItemsCount / (double)AllItemsCount) * 100);

            ConfirmedItemsCount = allElements.Count(e => e.IsCorrectedByHuman);
            ItemWithoutConfirmationCount = AllItemsCount - ConfirmedItemsCount;
            ConfirmedPercent = (int)(((double)ConfirmedItemsCount / (double)AllItemsCount) * 100);
        }
    }
}
