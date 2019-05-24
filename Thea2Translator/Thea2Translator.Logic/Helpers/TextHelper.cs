using System.Collections.Generic;
using System.Linq;

namespace Thea2Translator.Logic
{
    public class TextHelper
    {
        private static string CutTail(string source, string tail)
        {
            if (!isEndingWith(source, tail))
                return source;

            return source.Substring(0, source.Length - tail.Length);
        }

        private static string GetLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;

            return source.Substring(source.Length - tail_length);
        }

        private static bool isEndingWith(string source, string tail)
        {
            var lasts = GetLast(source, tail.Length);
            return (lasts == tail);
        }

        public static List<string> GetGroupsFromKey(string key)
        {
            var ret = new List<string>();

            key = CutTail(key, "_DES");
            key = new string(key.Where(c => char.IsLetter(c) || c == '_').ToArray());

            var elems = key.Split('_');
            var group = "";
            foreach (var elem in elems)
            {
                group += elem;
                ret.Add(group);
                group += "_";
            }

            return ret;
        }

        public static string NormalizeForVocabulary(string text)
        {
            var ret = RemoveUnnecessaryForVocabulary(text);
            if (ret.Length > 1)
                ret = ret.First().ToString() + ret.Substring(1).ToLower();

            return ret;
        }

        public static string RemoveUnnecessaryForVocabulary(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var ret = text.ToUpper();
            ret = ret.Trim();
            ret = ret.Replace("[EOLNN]", "");
            ret = ret.Replace("[EOLN]", "");
            ret = ret.Replace("[EOLRR]", "");
            ret = ret.Replace("[EOLR]", "");

            ret = new string(ret.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '\'').ToArray());
            return ret;
        }

        public static string Normalize(string text, bool withRemoveSpecials)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var ret = text;
            ret = ret.Replace("\\n", "[EOLNN]");
            ret = ret.Replace("\n", "[EOLN]");
            ret = ret.Replace("\\r", "[EOLRR]");
            ret = ret.Replace("\r", "[EOLR]");

            if (withRemoveSpecials) ret = removeSpecials(ret, "{", "}");
            return ret;
        }

        public static string UnNormalize(string text, string textPattern, bool withRemoveSpecials, bool replaceSpecialChars)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var ret = text;
            ret = ret.Replace("[EOLNN]", "\\n");
            ret = ret.Replace("[EOLN]", "\n");
            ret = ret.Replace("[EOLRR]", "\\r");
            ret = ret.Replace("[EOLR]", "\r");

            if (replaceSpecialChars)
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

            if (withRemoveSpecials) ret = replaceSpecials(ret, textPattern, "{", "}");
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
