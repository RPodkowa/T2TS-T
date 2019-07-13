using System.Collections.Generic;
using System.Linq;

namespace Thea2Translator.Logic
{
    public class TextHelper
    {
        public static bool EqualsTexts(string text1, string text2)
        {
            text1 = new string(text1.Where(c => char.IsLetterOrDigit(c)).ToArray());
            text2 = new string(text2.Where(c => char.IsLetterOrDigit(c)).ToArray());

            return text1 == text2;
        }

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

        public static List<string> GetGroupsFromKey(string key, bool usePureKey)
        {
            var ret = new List<string>();

            if (!usePureKey)
            {
                key = CutTail(key, "_DES");
                key = new string(key.Where(c => char.IsLetter(c) || c == '_').ToArray());
            }

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

        public static string Normalize(string text)
        {
            List<string> specials;
            return Normalize(text, out specials);
        }

        public static string PrepereToCompare(string text)
        {
            var ret = text;
            ret = ret.Replace("\\n", "");
            ret = ret.Replace("\n", "");
            ret = ret.Replace("\\r", "");
            ret = ret.Replace("\r", "");
            return ret;
        }

        public static string Normalize(string text, out List<string> specials)
        {
            specials = null;
            if (string.IsNullOrEmpty(text))
                return "";

            var ret = text;
            ret = ret.Replace("\\n", "{EOLNN}");
            ret = ret.Replace("\n", "{EOLN}");
            ret = ret.Replace("\\r", "{EOLRR}");
            ret = ret.Replace("\r", "{EOLR}");

            ret = removeSpecials(ret, out specials, "{", "}");
            return ret;
        }

        private static string removeSpecials(string text, out List<string> specials, string c1, string c2)
        {
            specials = new List<string>();
            string result = text;
            int indexOfOpen = 0;
            do
            {
                indexOfOpen = text.IndexOf(c1, indexOfOpen);
                int indexOfClose = text.IndexOf(c2, indexOfOpen + 1);
                if (indexOfOpen < 0) break;
                if (indexOfClose < 0)
                    break;

                var special = text.Substring(indexOfOpen + 1, indexOfClose - indexOfOpen-1);
                specials.Add(special);

                text = text.Substring(0, indexOfOpen + 1) + text.Substring(indexOfClose);                
                indexOfOpen += 2;

            } while (true);

            return text;
        }

        public static string UnNormalize(string text, List<string> specials)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var ret = text;
            ret = ret.Replace("} ", "}");
            ret = InsertSpecials(ret, specials, "{", "}");
            ret = ret.Replace("{EOLNN}", "\\n");
            ret = ret.Replace("{EOLN}", "\n");
            ret = ret.Replace("{EOLRR}", "\\r");
            ret = ret.Replace("{EOLR}", "\r");
            return ret;
        }

        public static string ReplacePolishChars(string text)
        {
            string[,] polishChars =
{
                        { "Ą", "A" }, { "Ć", "C" }, { "Ę", "E" }, { "Ł", "L" }, { "Ń", "N" }, { "Ó", "O" }, { "Ś", "S" }, { "Ź", "Z" }, { "Ż", "Z" },
                        { "ą", "a" }, { "ć", "c" }, { "ę", "e" }, { "ł", "l" }, { "ń", "n" }, { "ó", "o" }, { "ś", "s" }, { "ź", "z" }, { "ż", "z" },
                };

            for (int i = 0; i < polishChars.GetLength(0); i++)
            {
                text = text.Replace(polishChars[i, 0], polishChars[i, 1]);
            }

            return text;
        }

        private static string InsertSpecials(string text, List<string> specials, string c1, string c2)
        {
            string result = text;
            int indexOfOpenT = 0;
            int listIndex = 0;
            do
            {
                indexOfOpenT = text.IndexOf(c1, indexOfOpenT);
                int indexOfCloseT = text.IndexOf(c2, indexOfOpenT + 1);
                if (indexOfOpenT < 0) break;
                if (indexOfCloseT < 0)
                    break;

                string textL = text.Substring(0, indexOfOpenT + 1);
                string textM = specials[listIndex++];
                string textR = text.Substring(indexOfCloseT);
                text = textL + textM + textR;
                indexOfOpenT += 2;

            } while (true);

            return text;
        }
    }
}
