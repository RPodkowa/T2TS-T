﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Cache.Interfaces
{
    public interface IStatistic
    {
        int AllItemsCount { get; }
        int TranslatedItemsCount { get; }
        int ItemWithoutTranslationCount { get; }
        int TranslatedPercent { get; }

        void Reload(IDataCache dataCache);
    }
}