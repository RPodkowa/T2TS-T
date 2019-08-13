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
        public static string UserId;
        public static string UserName;

        private static DataCache _dataBase;
        private static DataCache _modules;
        private static DataCache _names;
        private static LanguageManager _languageManager;
        private static Statistic _statistic;

        public static IDataCache DataBase => 
            _dataBase != null ?_dataBase : _dataBase = new DataCache(FilesType.DataBase);

        public static IDataCache Modules =>
            _modules != null ? _modules : _modules = new DataCache(FilesType.Modules);

        public static IDataCache Names =>
            _names != null ? _names : _names = new DataCache(FilesType.Names);

        public static ILanguageManager Language =>
            _languageManager != null ? _languageManager : _languageManager = new LanguageManager();

        public static IStatistic Statistic =>
            _statistic != null ? _statistic : _statistic = new Statistic();

        public static WwwStatusModel GetWwwStatus()
        {
            var ret = new WwwStatusModel();
            ret.modifiedDate = DateTime.Now.ToString("dd.MM.yyyy");
            ret.modules = new List<Module>();
            ret.modules.Add(GetModuleInfo(DataBase));
            ret.modules.Add(GetModuleInfo(Modules));
            return ret;
        }

        private static Module GetModuleInfo(IDataCache dataCache)
        {
            dataCache.ReloadElems();
            var statistic = new Statistic();
            statistic.Reload(dataCache);

            var ret = new Module();
            ret.name = statistic.Type.ToString().ToLower(); //"name":"database",
            ret.allRecords = statistic.AllItemsCount.ToString(); //"allRecords":"5 336",
            ret.translatedByGoogle = statistic.TranslatedItemsCount.ToString(); //"translatedByGoogle":"4 761",
            ret.translatedByGooglePercent = statistic.TranslatedPercent.ToString(); //"translatedByGooglePercent":"89",
            ret.correctedRecords = statistic.ConfirmedItemsCount.ToString(); //"correctedRecords": "1 191",
            ret.correctedPercent = statistic.ConfirmedPercent.ToString(); //"correctedPercent":"22"
            return ret;

        }
    }
}
