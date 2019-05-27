using Thea2Translator.Logic.Languages;
using Thea2Translator.Logic.Languages.Interfaces;

namespace Thea2Translator.Logic
{
    public static class LogicProvider
    {
        private static DataCache _dataBase;
        private static DataCache _modules;
        private static LanguageManager _languageManager;

        public static IDataCache DataBase => 
            _dataBase != null ?_dataBase : _dataBase = new DataCache(FilesType.DataBase);

        public static IDataCache Modules =>
            _modules != null ? _modules : _modules = new DataCache(FilesType.Modules);

        public static ILanguageManager Language =>
            _languageManager != null ? _languageManager : _languageManager = new LanguageManager();

    }
}
