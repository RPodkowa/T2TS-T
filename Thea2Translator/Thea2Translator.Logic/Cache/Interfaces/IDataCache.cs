using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public interface IDataCache
    {
        event Action<string, double> StatusChanged;

        IList<CacheElem> CacheElems { get; }
        IList<string> Groups { get; }
        Vocabulary Vocabulary { get; }

        void UpdateVocabulary(Vocabulary vocabulary);
        void ReloadElems(bool withGroups = false, bool withVocabulary = false);
        void SaveElems(bool withVocabulary = false);
        void MakeStep(AlgorithmStep step);

    }
}
