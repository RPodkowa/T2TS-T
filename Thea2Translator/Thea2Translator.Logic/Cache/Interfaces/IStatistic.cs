using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Cache.Interfaces
{
    public interface IStatistic
    {
        FilesType Type { get; }
        int AllItemsCount { get; }
        int TranslatedItemsCount { get; }
        int ItemWithoutTranslationCount { get; }
        int TranslatedPercent { get; }
        int ConfirmedItemsCount { get; }
        int ItemWithoutConfirmationCount { get; }
        int ConfirmedPercent { get; }

        void Reload(IDataCache dataCache);
        void SaveFullModuleStatistics(IDataCache dataCache);
        string GetSummary(bool forPublication);        
    }
}
