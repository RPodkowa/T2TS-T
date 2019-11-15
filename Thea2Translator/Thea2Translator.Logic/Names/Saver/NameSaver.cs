using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public class NameSaver
    {
        public List<NameSaverElem> NameSaverElems { get; private set; }

        public NameSaver(IList<CacheElem> cacheElems)
        {
            NameSaverElems = new List<NameSaverElem>();

            foreach (var cacheElem in cacheElems)
            {
                AddCacheElem(cacheElem);
            }

            NameSaverElems.Sort();
        }

        private void AddCacheElem(CacheElem cacheElem)
        {
            if (!cacheElem.IsNamesElem) return;
            if (!cacheElem.IsActive) return;

            var elem = GetOrCreateElem(cacheElem.Key);
            elem.AddByCacheElem(cacheElem);
        }

        private NameSaverElem GetOrCreateElem(string key)
        {
            var keyArray = key.Split(':');
            if (keyArray.Length == 0)
                throw new Exception($"NameSaver: Cos nie tak z elementem '{key}'");

            var collectionName = keyArray[0];

            var elem = NameSaverElems.FirstOrDefault(x => x.Collection == collectionName);
            if (elem == null)
            {
                elem = new NameSaverElem(collectionName, keyArray);
                NameSaverElems.Add(elem);
            }
            return elem;
        }
    }
}
