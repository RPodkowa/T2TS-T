using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Thea2Translator.Logic.Languages;

namespace Thea2Translator.DesktopApp.Helpers
{
    static class LanguageHelper
    {
        public static ResourceDictionary GetLanguageDictinary(Languages lang)
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (lang)
            {
                case Languages.English:
                    dict.Source = new Uri("..\\Resources\\StringResources.en.xaml",
                                  UriKind.Relative);
                    break;
                case Languages.Polish:
                    dict.Source = new Uri("..\\Resources\\StringResources.pl.xaml",
                                       UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\StringResources.en.xaml",
                                      UriKind.Relative);
                    break;
            }

            return dict;
        }
    }
}
