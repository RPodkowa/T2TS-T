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
            arr.Add($"\tPrzetłumaczonych: {TranslatedItemsCount} ({TranslatedPercent}%)");
            arr.Add($"\tPotwierdzonych/skorygowanych: {ConfirmedItemsCount} ({ConfirmedPercent}%)");

            string text = string.Join("\r\n", arr.ToArray());
            return text;
        }

        public void Reload(IDataCache dataCache)
        {
            Type = dataCache.GetFileType();

            var allElements = dataCache.CacheElems.Where(x => x.IsActive).ToList();

            AllItemsCount = allElements.Count;

            TranslatedItemsCount = allElements.Count(e => !e.ToTranslate);
            ItemWithoutTranslationCount = AllItemsCount - TranslatedItemsCount;
            TranslatedPercent = (int)(((double)TranslatedItemsCount / (double)AllItemsCount) * 100);

            ConfirmedItemsCount = allElements.Count(e => e.IsCorrectedByHuman);
            ItemWithoutConfirmationCount = AllItemsCount - ConfirmedItemsCount;
            ConfirmedPercent = (int)(((double)ConfirmedItemsCount / (double)AllItemsCount) * 100);
        }

        public void SaveFullModuleStatistics(IDataCache dataCache)
        {
            MakeCvsStatisticFile(dataCache, StatisticType.Date);
            MakeCvsStatisticFile(dataCache, StatisticType.Time);
            MakeCvsStatisticFile(dataCache, StatisticType.User);
            MakeCvsStatisticFile(dataCache, StatisticType.UserDate);
            MakeCvsStatisticFile(dataCache, StatisticType.DayOfWeek);
            MakeCvsStatisticFile(dataCache, StatisticType.Month);        
        }

        private void MakeCvsStatisticFile(IDataCache dataCache, StatisticType statisticType)
        {
            var dict = new Dictionary<string, int>();
            var users = new List<string>();

            var elements = dataCache.CacheElems.Where(x => x.IsActive).ToList();
            foreach (var elem in elements)
            {
                string time = "";
                string date = "";
                string dayOfWeek = "";
                string month = "";
                string user = "";

                if (!elem.ToConfirm)
                {
                    time = elem.ConfirmationTime.Value.ToString("HH");
                    date = elem.ConfirmationTime.Value.ToString("yyyyMMdd");
                    dayOfWeek = elem.ConfirmationTime.Value.DayOfWeek.ToString();
                    month = elem.ConfirmationTime.Value.ToString("yyyyMM");
                    user = elem.ConfirmationUser;
                }
                else
                {
                    user = elem.ToTranslate ? "Brak" : "Google";
                }

                string key = null;
                switch (statisticType)
                {
                    case StatisticType.Date: key = date; break;
                    case StatisticType.Time: key = time; break;
                    case StatisticType.User: key = user; break;
                    case StatisticType.UserDate: key = $"{user}_{date}"; break;
                    case StatisticType.DayOfWeek: key = dayOfWeek; break;
                    case StatisticType.Month: key = month; break;
                }

                if (!dict.Keys.Contains(key))
                    dict.Add(key, 0);

                if (!users.Contains(user))
                    users.Add(user);

                dict[key]++;
            }

            var elems = new List<string>();
            var sorted = dict.OrderBy(x => x.Key);

            if (statisticType== StatisticType.UserDate)
            {
                var dict2 = new Dictionary<string, Dictionary<string, int>>();
                
                foreach (var elem in sorted)
                {
                    var key = elem.Key;
                    var keyElems = key.Split('_');
                    var dateKey = keyElems[1];
                    var userKey = keyElems[0];

                    if (!dict2.Keys.Contains(dateKey))
                    {
                        dict2.Add(dateKey, new Dictionary<string, int>());
                        foreach (var user in users)                        
                            dict2[dateKey].Add(user, 0);                        
                    }

                    (dict2[dateKey])[userKey] = elem.Value;
                }

                var l = "date";
                foreach (var user in users)
                    l += $";{user}";

                elems.Add(l);

                foreach (var elem in dict2)
                {
                    var line = $"{elem.Key}";                                       
                    foreach (var userElem in elem.Value)                    
                        line += $";{userElem.Value}";                   
                    
                    elems.Add(line);
                }
            }
            else
            {
                foreach (var elem in sorted)
                {
                    elems.Add($"{elem.Key};{elem.Value}");
                }
            }

            var file = FileHelper.GetLocalFilePatch(DirectoryType.Statistics, $"{dataCache.GetFileType()}_{statisticType}.csv");
            FileHelper.SaveListToFile(elems, file);
        }

        //private void 
    }
}
