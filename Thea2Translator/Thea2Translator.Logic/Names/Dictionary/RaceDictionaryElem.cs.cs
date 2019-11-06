using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public class RaceDictionaryElem
    {
        public string Race { get; private set; }
        public string Description { get; private set; }

        public NameDictionary Dictionary { get; private set; }

        public RaceDictionaryElem(NameGeneratorElem nameGeneratorElem)
        {
            Race = nameGeneratorElem.Race;
            Description = nameGeneratorElem.Description;
            ReadFromNameGeneratorElem(nameGeneratorElem);
        }

        public void ReadFromNameGeneratorElem(NameGeneratorElem nameGeneratorElem)
        {
            if (Dictionary == null) Dictionary = new NameDictionary();
            Dictionary.ReadFromNameGeneratorElem(nameGeneratorElem);
        }

        public string GetRaceDictionaryDescription()
        {
            var ret = new List<string>();

            if (!string.IsNullOrEmpty(Description))
                ret.Add(Description);

            ret.Add(Dictionary.ToString());

            return string.Join("\r\n", ret.ToArray());
        }
    }
}
