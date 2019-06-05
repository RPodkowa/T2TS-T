using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Thea2Translator.Logic;
using System.Windows;

namespace Thea2Translator.DesktopApp.Helpers
{
    public static class ExtensionsMethods
    {
        public static void SetLanguageDictinary(this Page page)
        {
            var currentLangDictinary = LanguageHelper.GetLanguageDictinary(LogicProvider.Language.CurrentLanguage);
            page.Resources.MergedDictionaries.Add(currentLangDictinary);
        }

        public static void SetLanguageDictinary(this Window window)
        {
            var currentLangDictinary = LanguageHelper.GetLanguageDictinary(LogicProvider.Language.CurrentLanguage);
            window.Resources.MergedDictionaries.Add(currentLangDictinary);
        }
    }
}
