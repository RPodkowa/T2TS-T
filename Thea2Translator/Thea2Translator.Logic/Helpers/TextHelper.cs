﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Thea2Translator.Logic
{
    public class TextHelper
    {
        public static string GetStringFromList(List<string> list, string separator)
        {
            if (list == null)
                return "";

            list.RemoveAll(x => string.IsNullOrEmpty(x));
            if (list.Count == 0)
                return "";

            return string.Join(separator, list.ToArray());
        }

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
            ret = ret.ToLower();
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

        public static string NormalizeNames(string text)
        {
            var ret = text;
            ret = ret.Replace("\\n", "");
            ret = ret.Replace("\n", "");
            ret = ret.Replace("\\r", "");
            ret = ret.Replace("\r", "");
            ret = ret.Replace("\\t", "");
            ret = ret.Replace("\t", "");
            ret = ret.Replace(" ", "");
            ret = ret.Replace("_", " ");
            return ret;
        }

        public static string NormalizeName(string text, out string translated)
        {
            translated = "";

            int indexOfOpen = text.IndexOf("[", 0);
            int indexOfClose = text.IndexOf("]", indexOfOpen + 1);
            if (indexOfOpen>0 && indexOfClose>0)
            {
                translated = text.Substring(indexOfOpen + 1, indexOfClose - indexOfOpen - 1);
                text = text.Replace($"[{translated}]", "");
            }

            return text;
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

        public static int StringOccurens(string text, string pattern)
        {
            return Regex.Matches(text, pattern).Count;
        }

        public static bool IsEqualsSpecials(List<string> specialsL, List<string> specialsR)
        {
            if (specialsL == null && specialsR == null) return true;

            if (specialsL == null && specialsR != null) return false;
            if (specialsL != null && specialsR == null) return false;
            if (specialsL.Count != specialsR.Count) return false;

            for (int i=0; i< specialsL.Count;i++)
            {
                var specialL = specialsL[i];
                var specialR = specialsR[i];

                if (specialL.Substring(0, 3) == "EOL" && specialR.Substring(0, 3) == "EOL")
                    continue;

                if (specialL != specialR) return false;
            }

            return true;
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

        public static string PrepareNamesString()
        {
            string ret = "";
            //ret = FileHelper.ReadFileString(@"D:\RPA\T2TS-T\Thea2Translator\Thea2Translator.DesktopApp\bin\x64\Meskie.txt");
            //ret = FileHelper.ReadFileString(@"D:\RPA\T2TS-T\Thea2Translator\Thea2Translator.DesktopApp\bin\x64\Zenski.txt");
            if (string.IsNullOrEmpty(ret))
                return ret;
                       
            ret = ret.Replace("1", "");
            ret = ret.Replace("2", "");
            ret = ret.Replace("3", "");
            ret = ret.Replace("4", "");
            ret = ret.Replace("5", "");
            ret = ret.Replace("6", "");
            ret = ret.Replace("7", "");
            ret = ret.Replace("8", "");
            ret = ret.Replace("9", "");
            ret = ret.Replace("0", "");
            
            ret = ret.Replace(" stycznia", "");
            ret = ret.Replace(" lutego", "");
            ret = ret.Replace(" marca", "");
            ret = ret.Replace(" kwietnia", "");
            ret = ret.Replace(" maja", "");
            ret = ret.Replace(" czerwca", "");
            ret = ret.Replace(" lipca", "");
            ret = ret.Replace(" sierpnia", "");
            ret = ret.Replace(" września", "");
            ret = ret.Replace(" października", "");
            ret = ret.Replace(" listopada", "");
            ret = ret.Replace(" grudnia", "");

            ret = ret.Replace("(wróć do indeksu)", "");
            ret = ret.Replace("(imię)", "");
            ret = ret.Replace("zob.", "");
            ret = ret.Replace("(", "");
            ret = ret.Replace(")", "");
            ret = ret.Replace("?", "");
            ret = ret.Replace("-", "");
            ret = ret.Replace("–", "");
            ret = ret.Replace("—", "");            
            ret = ret.Replace(" ", "");
            ret = ret.Replace("\t", "");
            ret = ret.Replace("\r", ",");
            ret = ret.Replace("\n", ",");
            ret = ret.Replace(",,,,,", ",");
            ret = ret.Replace(",,,,", ",");
            ret = ret.Replace(",,,", ",");
            ret = ret.Replace(",,", ",");
            ret = ret.Replace(",", ",");
            ret=ret.ToUpper();

            var list = ret.Split(',').ToList();
            var dict = new Dictionary<string, List<string>>();

            foreach (var elem in list)
            {
                if (string.IsNullOrEmpty(elem))
                    continue;

                if (elem.Length == 1)
                    continue;

                var first = elem[0].ToString();

                if (!dict.Keys.Contains(first))
                    dict.Add(first, new List<string>());

                var dictList = dict[first];

                var name = elem.First().ToString() + elem.Substring(1).ToLower();

                if (!dictList.Contains(name))
                {
                    dictList.Add(name);
                    dictList.Sort();
                }
            }
            
            var sorted = dict.OrderBy(x => x.Key);

            list = new List<string>();
            foreach (var key in dict.Keys)
            {
                list.Add(string.Join(", ", dict[key].ToArray()));
            }
                        
            ret = string.Join(",", list.ToArray());
            //ret = string.Join(",\r\n", list.ToArray());

            var newList = ret.Split(',').ToList();
            newList = newList.Where(x => x.EndsWith("sław")).ToList();
            var newList2 = new List<string>();
            foreach (var newElem in newList)
            {
                var e = newElem.Replace("sław", "");
                e = e.Replace(" ", "");
                if (e.Length <= 2)
                    continue;
                var l = e.Last().ToString();
                if (l=="o" || l=="i")
                {
                    if (newList2.Contains(e)) continue;
                    newList2.Add(e);
                }
            }
            var ret2 = string.Join(",", newList.ToArray());
            var ret3 = string.Join(",", newList2.ToArray());

            return ret;
        }
    }
}
