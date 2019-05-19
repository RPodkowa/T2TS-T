using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Thea2Translator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBoxDir.Text = @"d:\t2";
        }

        #region Step1
        private void bttnStart_Click(object sender, EventArgs e)
        {
            ProcessFilesDataBase();
            ProcessFilesModule();
        }

        private void ProcessFilesDataBase()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] fileEntriesM = Directory.GetFiles(GetDirName("DataBase"));
            foreach (string fileName in fileEntriesM)
            {
                ProcessFileDataBase(fileName, dic);
            }

            string path = GetPath(GetDirName("DataBaseStep1"));
            SaveFile(dic, path, "DataBaseOut");
        }

        private void ProcessFilesModule()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string[] fileEntriesM = Directory.GetFiles(GetDirName("Modules"));
            foreach (string fileName in fileEntriesM)
            {
                ProcessFileModule(fileName, dic);
            }

            string path = GetPath(GetDirName("ModulesStep1"));
            SaveFile(dic, path, "ModulesOut");
        }

        private void ProcessFileDataBase(string file, Dictionary<string, string> dic)
        {
            var fn = Path.GetFileNameWithoutExtension(file);

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            foreach (XmlNode lvl1 in doc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode lvl2 in lvl1.ChildNodes)
                {
                    if (lvl2.Attributes == null) continue;

                    string k = "";
                    string v = "";
                    foreach (XmlAttribute attribute in lvl2.Attributes)
                    {
                        if (attribute.Name == "Key") k = attribute.Value;
                        if (attribute.Name == "Val") v = normalizeText(attribute.Value);
                    }

                    if (dic.ContainsKey(k))
                        continue;

                    dic.Add(k, v);
                }
            }
        }

        private void ProcessFileModule(string file, Dictionary<string, string> dic)
        {
            var fn = Path.GetFileNameWithoutExtension(file);

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            foreach (XmlNode lvl1 in doc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode lvl2 in lvl1.ChildNodes)
                {
                    if (lvl2.Name != "nodes") continue;

                    string txt = normalizeText(lvl2.InnerText);
                    if (!string.IsNullOrEmpty(txt))
                    {
                        if (!dic.ContainsKey(txt))
                            dic.Add(txt, txt);
                    }

                    foreach (XmlNode lvl3 in lvl2.ChildNodes)
                    {
                        if (lvl3.Name != "outputs") continue;

                        if (lvl3.Attributes == null) continue;

                        string v = "";
                        foreach (XmlAttribute attribute in lvl3.Attributes)
                        {
                            if (attribute.Name == "name") v = normalizeText(attribute.Value);
                        }

                        if (string.IsNullOrEmpty(v)) continue;
                        if (!dic.ContainsKey(v))
                            dic.Add(v, v);
                    }
                }
            }
        }

        #endregion

        #region Step2
        private void bttnStart2_Click(object sender, EventArgs e)
        {
            ProcessFilesDataBase2();
            ProcessFilesModule2();
        }

        private void ProcessFilesDataBase2()
        {
            var dic = GetDictrionary("DataBase");
            string[] fileEntriesM = Directory.GetFiles(GetDirName("DataBase"));
            foreach (string fileName in fileEntriesM)
            {
                ProcessFileDataBase2(fileName, dic);
            }
        }
        private void ProcessFileDataBase2(string file, Dictionary<string, string> dic)
        {
            var fn = Path.GetFileNameWithoutExtension(file);

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            foreach (XmlNode lvl1 in doc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode lvl2 in lvl1.ChildNodes)
                {
                    if (lvl2.Attributes == null) continue;

                    string k = "";
                    string v = "";

                    XmlAttribute attributeVal = null;

                    foreach (XmlAttribute attribute in lvl2.Attributes)
                    {
                        if (attribute.Name == "Key") k = attribute.Value;
                        if (attribute.Name == "Val")
                        {
                            v = normalizeText(attribute.Value);
                            attributeVal = attribute;
                        }
                    }

                    if (dic.ContainsKey(k) && attributeVal != null)
                    {
                        attributeVal.Value = unNormalizeText(dic[k], attributeVal.Value);
                    }
                }
            }

            string path = GetPath(GetDirName("DataBaseNew"));
            string newFile = path + fn + ".xml";
            doc.Save(newFile);
        }

        private void ProcessFilesModule2()
        {
            var dic = GetDictrionary("Modules");
            string[] fileEntriesM = Directory.GetFiles(GetDirName("Modules"));
            foreach (string fileName in fileEntriesM)
            {
                ProcessFileModule2(fileName, dic);
            }
        }
        private void ProcessFileModule2(string file, Dictionary<string, string> dic)
        {
            var fn = Path.GetFileNameWithoutExtension(file);

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            foreach (XmlNode lvl1 in doc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode lvl2 in lvl1.ChildNodes)
                {
                    if (lvl2.Name != "nodes") continue;

                    foreach (XmlNode lvl3 in lvl2.ChildNodes)
                    {
                        if (lvl3.Name != "outputs" && lvl3.Name != "#text") continue;

                        if (lvl3.Name == "#text")
                        {
                            string txt = normalizeText(lvl3.InnerText);
                            if (!string.IsNullOrEmpty(txt))
                            {
                                if (dic.ContainsKey(txt))
                                    lvl3.InnerText = unNormalizeText(dic[txt], lvl3.InnerText, true);
                            }
                            continue;
                        }

                        if (lvl3.Attributes == null) continue;

                        foreach (XmlAttribute attribute in lvl3.Attributes)
                        {
                            string v = "";
                            if (attribute.Name == "name") v = normalizeText(attribute.Value);

                            if (string.IsNullOrEmpty(v)) continue;
                            if (dic.ContainsKey(v))
                            {
                                attribute.Value = unNormalizeText(dic[v], attribute.Value, true);
                            }
                        }
                    }
                }
            }
            
            string path = GetPath(GetDirName("ModulesNew"));
            string newFile = path + fn + ".xml";
            //var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            //doc.ReplaceChild(declaration, doc.FirstChild);
            doc.Save(newFile);
        }

        private Dictionary<string, string> GetDictrionary(string dir)
        {
            Dictionary<string, string> dicKey = null;
            Dictionary<string, string> dicValue = null;
            ReadDictionarys(dir, ref dicKey, ref dicValue);

            Dictionary<string, string> dicRet = new Dictionary<string, string>();

            foreach (var elem in dicKey)
            {
                var k = elem.Key;
                var newK = elem.Value;

                var newV = dicValue[k].Trim();
                dicRet.Add(newK, newV);
            }

            return dicRet;
        }

        private void ReadDictionarys(string dir, ref Dictionary<string, string> dicKey, ref Dictionary<string, string> dicValue)
        {            
            string keyFile = dir + "Out.txt";
            string valueFile = dir + "Out_v_";
            {
                string step1Dir = GetDirName(dir + "Step1");
                string[] fileEntries = Directory.GetFiles(step1Dir);
                foreach (string fileName in fileEntries)
                {                 
                    if (fileName.Contains(keyFile)) ReadDictionary(fileName, ref dicKey);
                }
            }
            {
                string step2Dir = GetDirName(dir + "Step2");
                string[] fileEntries = Directory.GetFiles(step2Dir);
                foreach (string fileName in fileEntries)
                {                    
                    if (fileName.Contains(valueFile)) ReadDictionary(fileName, ref dicValue);
                }
            }
        }

        private void ReadDictionary(string file, ref Dictionary<string, string> dic)
        {
            if (dic == null) dic = new Dictionary<string, string>();

            char[] chars = { ':' };
            char[] chars2 = { '.' };
            char[] chars3 = { ' ' };

            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                var elems = line.Split(chars);
                if (elems == null || elems.Length < 2)
                {
                    //Szansa 2
                    elems = line.Split(chars2);
                    if (elems == null || elems.Length < 2)
                    {
                        //Szansa 3
                        elems = line.Split(chars3);
                        if (elems == null || elems.Length < 2)
                            continue;
                    }
                }


                var key = elems[0];
                key = key.Replace(" ", "");
                var val = string.Join(":", elems,1,elems.Length-1);

                if (dic.ContainsKey(key))
                    continue;

                dic.Add(key, val);
            }
        }

        #endregion

        #region Helpers

        private string GetDirName(string dir)
        {
            return textBoxDir.Text + @"\" + dir;
        }

        private static string normalizeText(string text)
        {
            var ret = text;
            ret = ret.Replace("\\n", "[EOLNN]");
            ret = ret.Replace("\n", "[EOLN]");
            ret = ret.Replace("\\r", "[EOLRR]");
            ret = ret.Replace("\r", "[EOLR]");

            ret = removeSpecials(ret, "{", "}");
            return ret;
        }

        private static string unNormalizeText(string text, string textPattern, bool replaseSpecialChars = false)
        {
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

        private string GetPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + @"\";
        }

        private void SaveFile(Dictionary<string, string> dic, string path, string file)
        {
            TextWriter tw_k = new StreamWriter($"{path}{file}.txt", true);
            TextWriter tw_v = null;
            int i = 0;
            int f = 0;
            foreach (var elem in dic)
            {
                if (i % 6000 == 0)
                {
                    if (tw_v != null) tw_v.Close();
                    tw_v = new StreamWriter($"{path}{file}_v_{f++}.txt", true);
                }

                var k = elem.Key;
                var v = elem.Value;
                int nr = ++i;

                if (tw_k != null) tw_k.WriteLine($"{nr}:{k}");
                if (tw_v != null) tw_v.WriteLine($"{nr}:{v}");
            }

            if (tw_k != null) tw_k.Close();
            if (tw_v != null) tw_v.Close();
        }
        #endregion

        private void buttonDir_Click(object sender, EventArgs e)
        {
            searchFolder(textBoxDir, "Dir");
        }

        private void searchFolder(TextBox box, string description)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(box.Text))
                fbd.SelectedPath = box.Text;

            fbd.Description = description;
            if (fbd.ShowDialog() == DialogResult.OK)
                box.Text = fbd.SelectedPath;
        }
    }
}
