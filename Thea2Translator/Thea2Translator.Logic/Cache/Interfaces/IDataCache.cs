using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Cache.Interfaces
{
    public interface IDataCache
    {
        event Action<string, double> StatusChanged;

        IList<CacheElem> CacheElems { get; }

        void ReloadElems();
        void SaveElems();
        void MakeStep(AlgorithmStep step);

    }
}
