using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Languages.Interfaces
{
    public interface ILanguageManager
    {
        void SetLanguage(Languages newLanguage);
        Languages CurrentLanguage { get; }
    }
}
