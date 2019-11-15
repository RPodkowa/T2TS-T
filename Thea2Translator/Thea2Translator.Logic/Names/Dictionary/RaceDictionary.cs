using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public class RaceDictionary
    {
        public IList<RaceDictionaryElem> RaceDictionaryElems { get; private set; }

        public void ReadFromNameGeneratorElem(NameGeneratorElem nameGeneratorElem)
        {
            var race = nameGeneratorElem.Race;
            if (RaceDictionaryElems == null) RaceDictionaryElems = new List<RaceDictionaryElem>();
            foreach (var raceDictionaryElem in RaceDictionaryElems)
            {
                if (race==raceDictionaryElem.Race)
                {
                    raceDictionaryElem.ReadFromNameGeneratorElem(nameGeneratorElem);
                    return;
                }
            }

            RaceDictionaryElems.Add(new RaceDictionaryElem(nameGeneratorElem));
        }

        public override string ToString()
        {
            var ret = new List<string>();

            if (RaceDictionaryElems == null)
                return null;

            foreach (var elem in RaceDictionaryElems)
            {
                ret.Add(elem.GetRaceDictionaryDescription());
            }

            return TextHelper.GetStringFromList(ret, "\r\n");
        }
    }
}
