using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Cache
{
    public class GroupValuesModel
    {
        public int CorrectedValue { get; private set; }
        public int NotCorrectedValue { get; private set; }
        public int SumValue { get { return CorrectedValue + NotCorrectedValue; } }

        public GroupValuesModel()
        {
            CorrectedValue = 0;
            NotCorrectedValue = 0;
        }

        public void AddValue(bool corrected)
        {
            if (corrected) CorrectedValue++;
            else NotCorrectedValue++;
        }
    }
}
