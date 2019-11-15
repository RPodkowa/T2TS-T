using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public class NameDictionary
    {
        public IList<NameDictionaryElem> NameDictionaryElems { get; private set; }

        public void TryAddWord(string category, string oryginalWord, string translatedWord)
        {
            if (NameDictionaryElems == null) NameDictionaryElems = new List<NameDictionaryElem>();
            foreach (var elem in NameDictionaryElems)
            {
                if (elem.Category == category)
                {
                    elem.TryAddWord(oryginalWord, translatedWord);
                    return;
                }
            }

            NameDictionaryElems.Add(new NameDictionaryElem(category, oryginalWord, translatedWord));
        }
        
        public void ReadFromNameGeneratorElem(NameGeneratorElem nameGeneratorElem)
        {
            if (nameGeneratorElem.Collections == null) return;
            foreach (var collection in nameGeneratorElem.Collections)
            {
                var dictionary = collection.CollectionDictionary;
                if (dictionary.NameDictionaryElems == null) continue;
                foreach (var dictionaryElem in dictionary.NameDictionaryElems)
                {
                    AddFromDictionaryElems(dictionaryElem);
                }
            }
        }

        private void AddFromDictionaryElems(NameDictionaryElem nameDictionaryElem)
        {
            if (NameDictionaryElems == null) NameDictionaryElems = new List<NameDictionaryElem>();
            foreach (var elem in NameDictionaryElems)
            {
                if (elem.Category != nameDictionaryElem.Category) continue;
                elem.AddFromDictionaryElems(nameDictionaryElem);
                return;
            }

            NameDictionaryElems.Add(new NameDictionaryElem(nameDictionaryElem));
        }

        public override string ToString()
        {
            var ret = new List<string>();

            if (NameDictionaryElems == null) NameDictionaryElems = new List<NameDictionaryElem>();
            foreach (var elem in NameDictionaryElems)
            {
                ret.Add(elem.ToString());
            }

            return string.Join("\r\n", ret.ToArray());
        }
    }
}
