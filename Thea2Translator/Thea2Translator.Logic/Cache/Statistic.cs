using System.Collections.Generic;
using System.Linq;
using Thea2Translator.Logic.Cache.Interfaces;

namespace Thea2Translator.Logic.Cache
{
    class Statistic : IStatistic
    {
        public FilesType Type { get; private set; }
        public int AllItemsCount { get; private set; }
        public int TranslatedItemsCount { get; private set; }
        public int ItemWithoutTranslationCount { get; private set; }
        public int TranslatedPercent { get; private set; }
        public int ConfirmedItemsCount { get; private set; }
        public int ItemWithoutConfirmationCount { get; private set; }
        public int ConfirmedPercent { get; private set; }

        public string GetSummary()
        {
            var arr = new List<string>();
            arr.Add($"\tLinii: {AllItemsCount}");
            arr.Add($"\tPrzetlumaczonych: {TranslatedItemsCount} ({TranslatedPercent}%)");
            arr.Add($"\tPotwierdzonych/skorygowanych: {ConfirmedItemsCount} ({ConfirmedPercent}%)");

            string text = string.Join("\r\n", arr.ToArray());
            return text;
        }

        public void Reload(IDataCache dataCache)
        {
            Type = dataCache.GetFileType();

            var allElements = dataCache.CacheElems;
            AllItemsCount = allElements.Count;

            TranslatedItemsCount = allElements.Count(e => !e.ToTranslate);
            ItemWithoutTranslationCount = AllItemsCount - TranslatedItemsCount;
            TranslatedPercent = (int)(((double)TranslatedItemsCount / (double)AllItemsCount) * 100);

            ConfirmedItemsCount = allElements.Count(e => e.IsCorrectedByHuman);
            ItemWithoutConfirmationCount = AllItemsCount - ConfirmedItemsCount;
            ConfirmedPercent = (int)(((double)ConfirmedItemsCount / (double)AllItemsCount) * 100);
        }

        public void MakeFile()
        {

        }
    }
}
