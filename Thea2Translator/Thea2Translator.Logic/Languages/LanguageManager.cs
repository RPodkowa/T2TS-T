using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thea2Translator.Logic.Languages.Interfaces;

namespace Thea2Translator.Logic.Languages
{
    class LanguageManager : ILanguageManager
    {
        public Languages CurrentLanguage { private set; get; }

        public LanguageManager()
        {
            CurrentLanguage = Languages.English;
        }

        public void SetLanguage(Languages newLanguage)
        {
            CurrentLanguage = newLanguage;
        }
    }
}
