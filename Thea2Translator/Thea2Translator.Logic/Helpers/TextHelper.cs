using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Helpers
{
    public class TextHelper
    {
        public static string Normalize(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var ret = text;
            ret = ret.Replace("\\n", "[EOLNN]");
            ret = ret.Replace("\n", "[EOLN]");
            ret = ret.Replace("\\r", "[EOLRR]");
            ret = ret.Replace("\r", "[EOLR]");

            ret = removeSpecials(ret, "{", "}");
            return ret;
        }

        public static string UnNormalize(string text, string textPattern, bool replaseSpecialChars = false)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var ret = text;
            ret = ret.Replace("[EOLNN]", "\\n");
            ret = ret.Replace("[EOLN]", "\n");
            ret = ret.Replace("[EOLRR]", "\\r");
            ret = ret.Replace("[EOLR]", "\r");

            if (replaseSpecialChars)
            {
                string[,] specialChars =
                {
                        { "Ą", "A" }, { "Ć", "C" }, { "Ę", "E" }, { "Ł", "L" }, { "Ń", "N" }, { "Ó", "O" }, { "Ś", "S" }, { "Ź", "Z" }, { "Ż", "Z" },
                        { "ą", "a" }, { "ć", "c" }, { "ę", "e" }, { "ł", "l" }, { "ń", "n" }, { "ó", "o" }, { "ś", "s" }, { "ź", "z" }, { "ż", "z" },
                };

                for (int i = 0; i < specialChars.GetLength(0); i++)
                {
                    ret = ret.Replace(specialChars[i, 0], specialChars[i, 1]);
                }
            }

            ret = replaceSpecials(ret, textPattern, "{", "}");
            return ret;
        }

        private static string replaceSpecials(string text, string textPattern, string c1, string c2)
        {
            string result = text;
            int indexOfOpenT = 0;
            int indexOfOpenP = 0;
            do
            {
                indexOfOpenT = text.IndexOf(c1, indexOfOpenT);
                int indexOfCloseT = text.IndexOf(c2, indexOfOpenT + 1);
                if (indexOfOpenT < 0) break;
                if (indexOfCloseT < 0)
                    break;

                indexOfOpenP = textPattern.IndexOf(c1, indexOfOpenP);
                int indexOfCloseP = textPattern.IndexOf(c2, indexOfOpenP + 1);
                if (indexOfOpenP < 0) break;
                if (indexOfCloseP < 0)
                    break;

                string textL = text.Substring(0, indexOfOpenT + 1);
                string textM = textPattern.Substring(indexOfOpenP + 1, indexOfCloseP - (indexOfOpenP + 1));
                string textR = text.Substring(indexOfCloseT);
                text = textL + textM + textR;
                indexOfOpenT += 2;
                indexOfOpenP = indexOfCloseP;

            } while (true);

            return text;
        }

        private static string removeSpecials(string text, string c1, string c2)
        {
            string result = text;
            int indexOfOpen = 0;
            do
            {
                indexOfOpen = text.IndexOf(c1, indexOfOpen);
                int indexOfClose = text.IndexOf(c2, indexOfOpen + 1);
                if (indexOfOpen < 0) break;
                if (indexOfClose < 0)
                    break;

                text = text.Substring(0, indexOfOpen + 1) + text.Substring(indexOfClose);
                indexOfOpen += 2;

            } while (true);

            return text;
        }
    }
}
