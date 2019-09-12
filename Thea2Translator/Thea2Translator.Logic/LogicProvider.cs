using System;
using System.Collections.Generic;
using Thea2Translator.Logic.Cache;
using Thea2Translator.Logic.Cache.Interfaces;
using Thea2Translator.Logic.Languages;
using Thea2Translator.Logic.Languages.Interfaces;

namespace Thea2Translator.Logic
{
    public static class LogicProvider
    {
        private static DataCache _dataBase;
        private static DataCache _modules;
        private static DataCache _names;
        private static LanguageManager _languageManager;
        private static Statistic _statistic;

        public static IDataCache DataBase =>
            _dataBase != null ? _dataBase : _dataBase = new DataCache(FilesType.DataBase);

        public static IDataCache Modules =>
            _modules != null ? _modules : _modules = new DataCache(FilesType.Modules);

        public static IDataCache Names =>
            _names != null ? _names : _names = new DataCache(FilesType.Names);

        public static ILanguageManager Language =>
            _languageManager != null ? _languageManager : _languageManager = new LanguageManager();

        public static IStatistic Statistic =>
            _statistic != null ? _statistic : _statistic = new Statistic();

        public static void RepairDates()
        {
            RepairDates(DataBase);
            RepairDates(Modules);
        }

        private static void RepairDates(IDataCache dataCache)
        {
            string filePath = dataCache.GetFullPath();
            var db = FileHelper.ReadFileString(filePath);
            FileHelper.DeleteFileIfExists(filePath);

            for (int m = 5; m <= 9; m++)
            {
                string mounth = m.ToString();
                if (m < 10) mounth = "0" + mounth;
                for (int d = 1; d <= 31; d++)
                {
                    string day = d.ToString();
                    if (d < 10) day = "0" + day;

                    db = db.Replace($"<Time>{mounth}-{day}-2019", $"<Time>2019-{mounth}-{day}");
                    db = db.Replace($"<Time>{mounth}.{day}.2019", $"<Time>2019-{mounth}-{day}");
                    db = db.Replace($"<Time>{day}-{mounth}-2019", $"<Time>2019-{mounth}-{day}");
                    db = db.Replace($"<Time>{day}.{mounth}.2019", $"<Time>2019-{mounth}-{day}");
                }
            }

            FileHelper.WriteFileString(filePath, db);
        }

        public static WwwModuleStatus GetWwwStatus(FilesType filesType)
        {
            if (filesType == FilesType.StatusDatabase) return GetModuleInfo(DataBase);
            if (filesType == FilesType.StatusModules) return GetModuleInfo(Modules);            
            return null;
        }

        private static WwwModuleStatus GetModuleInfo(IDataCache dataCache)
        {
            dataCache.ReloadElems();
            if (dataCache.CacheElems == null || dataCache.CacheElems.Count == 0)
                return null;

            var statistic = new Statistic();
            statistic.Reload(dataCache);

            var ret = new WwwModuleStatus
            {
                modifiedDate = DateTime.Now.ToString("dd.MM.yyyy"),
                allRecords = statistic.AllItemsCount.ToString(), //"allRecords":"5 336",
                translatedByGoogle = statistic.TranslatedItemsCount.ToString(), //"translatedByGoogle":"4 761",
                translatedByGooglePercent = statistic.TranslatedPercent.ToString(), //"translatedByGooglePercent":"89",
                correctedRecords = statistic.ConfirmedItemsCount.ToString(), //"correctedRecords": "1 191",
                correctedPercent = statistic.ConfirmedPercent.ToString() //"correctedPercent":"22"
            };

            return ret;
        }
    }
}
