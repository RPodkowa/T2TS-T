using System.Collections.Generic;
using System.Linq;

namespace Thea2Translator.Logic
{
    public class NameDictionaryElem
    {
        public string Category { get; private set; }
        public Dictionary<string, string> Words { get; private set; }

        public NameDictionaryElem(NameDictionaryElem nameDictionaryElem)
        {
            Category = nameDictionaryElem.Category;
            AddFromDictionaryElems(nameDictionaryElem);
        }

        public NameDictionaryElem(string category, string oryginalWord, string translatedWord)
        {
            Category = category;
            Words = new Dictionary<string, string>();
            Words.Add(oryginalWord, translatedWord);
        }

        public void TryAddWord(string oryginalWord, string translatedWord)
        {
            if (Words == null) Words = new Dictionary<string, string>();
            if (Words.ContainsKey(oryginalWord))
            {
                if (Words[oryginalWord] != translatedWord)
                    throw new System.Exception($"Cos nie tak! '{oryginalWord}': '{Words[oryginalWord]}'!='{translatedWord}'");

                return;
            }

            Words.Add(oryginalWord, translatedWord);
        }

        public void AddFromDictionaryElems(NameDictionaryElem nameDictionaryElem)
        {
            if (Words == null) Words = new Dictionary<string, string>();
            foreach (var word in nameDictionaryElem.Words)
            {
                if (Words.ContainsKey(word.Key))
                    return;

                Words.Add(word.Key, word.Value);
            }
        }
        
        public override string ToString()
        {
            var ret = new List<string>();

            ret.Add($"\t{Category}:");

            var sorted = Words.OrderBy(x => x.Key);
            foreach (var elem in sorted)
            {
                ret.Add($"\t\t{elem.Key} - {elem.Value}");
            }

            return string.Join("\r\n", ret.ToArray());
        }
    }
}
