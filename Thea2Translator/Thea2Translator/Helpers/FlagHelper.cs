using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Helpers
{
    public static class FlagHelper
    {
        public static bool IsSettedBit(int flag, int bit)
        {
            return (flag & (1 << bit)) != 0;
        }

        public static int GetSettedBitValue(int flag, int bit, bool value)
        {
            if (value) flag = (flag | (1 << bit));
            else flag = (flag & (~(1 << bit)));
            return flag;
        }
    }
}
