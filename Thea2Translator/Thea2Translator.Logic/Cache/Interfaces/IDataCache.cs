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
        IList<string> Authors { get; }

        Vocabulary Vocabulary { get; }
        Navigation Navigation { get; }

        void UpdateVocabulary(Vocabulary vocabulary);
        void ReloadElems(bool withGroups = false, bool withVocabulary = false, bool withNavigation = false);
        void SaveElems(bool withVocabulary = false);
        void MakeStep(AlgorithmStep step);
        string GetDirectoryName(AlgorithmStep step);

        IList<string> GetStartingGroups();

        string GetSummary();

        FilesType GetFileType();
    }
}
