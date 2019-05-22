using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thea2Translator.Logic.Cache;
using Thea2Translator.Logic.Cache.Interfaces;

namespace Thea2Translator.Logic
{
    public static class LogicProvider
    {
        private static DataCache _dataBase;
        private static DataCache _modules;

        public static IDataCache DataBase => 
            _dataBase != null ?_dataBase : _dataBase = new DataCache(FilesType.DataBase);

        public static IDataCache Modules =>
            _modules != null ? _modules : _modules = new DataCache(FilesType.Modules);

    }
}
