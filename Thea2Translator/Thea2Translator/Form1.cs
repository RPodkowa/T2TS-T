using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Thea2Translator.Cache;
using Thea2Translator.Helpers;

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
        private void btnStart_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            ProcessFilesDataBase();
            ProcessFilesModule();
        }

        private void ProcessFilesDataBase()
        {            
            var cache = new DataCache(FilesType.DataBase);
            cache.MakeStep1();
        }

        private void ProcessFilesModule()
        {
            var cache = new DataCache(FilesType.Modules);
            cache.MakeStep1();
        }

        #endregion

        #region Step2
        private void bttnStart2_Click(object sender, EventArgs e)
        {
            FileHelper.MainDir = textBoxDir.Text;
            ProcessFilesDataBase2();
            ProcessFilesModule2();
        }

        private void ProcessFilesDataBase2()
        {
            var cache = new DataCache(FilesType.DataBase);
            cache.MakeStep2();
        }

        private void ProcessFilesModule2()
        {
            var cache = new DataCache(FilesType.Modules);
            cache.MakeStep2();
        }

        //private void ProcessFileDataBase2(string file, Dictionary<string, string> dic)
        //{
        //    var fn = Path.GetFileNameWithoutExtension(file);

        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(file);
        //    foreach (XmlNode lvl1 in doc.DocumentElement.ChildNodes)
        //    {
        //        foreach (XmlNode lvl2 in lvl1.ChildNodes)
        //        {
        //            if (lvl2.Attributes == null) continue;

        //            string k = "";
        //            string v = "";

        //            XmlAttribute attributeVal = null;

        //            foreach (XmlAttribute attribute in lvl2.Attributes)
        //            {
        //                if (attribute.Name == "Key") k = attribute.Value;
        //                if (attribute.Name == "Val")
        //                {
        //                    v = normalizeText(attribute.Value);
        //                    attributeVal = attribute;
        //                }
        //            }

        //            if (dic.ContainsKey(k) && attributeVal != null)
        //            {
        //                attributeVal.Value = unNormalizeText(dic[k], attributeVal.Value);
        //            }
        //        }
        //    }

        //    string path = GetPath(GetDirName("DataBaseNew"));
        //    string newFile = path + fn + ".xml";
        //    doc.Save(newFile);
        //}
        //private void ProcessFileModule2(string file, Dictionary<string, string> dic)
        //{
        //    var fn = Path.GetFileNameWithoutExtension(file);

        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(file);
        //    foreach (XmlNode lvl1 in doc.DocumentElement.ChildNodes)
        //    {
        //        foreach (XmlNode lvl2 in lvl1.ChildNodes)
        //        {
        //            if (lvl2.Name != "nodes") continue;

        //            foreach (XmlNode lvl3 in lvl2.ChildNodes)
        //            {
        //                if (lvl3.Name != "outputs" && lvl3.Name != "#text") continue;

        //                if (lvl3.Name == "#text")
        //                {
        //                    string txt = normalizeText(lvl3.InnerText);
        //                    if (!string.IsNullOrEmpty(txt))
        //                    {
        //                        if (dic.ContainsKey(txt))
        //                            lvl3.InnerText = unNormalizeText(dic[txt], lvl3.InnerText, true);
        //                    }
        //                    continue;
        //                }

        //                if (lvl3.Attributes == null) continue;

        //                foreach (XmlAttribute attribute in lvl3.Attributes)
        //                {
        //                    string v = "";
        //                    if (attribute.Name == "name") v = normalizeText(attribute.Value);

        //                    if (string.IsNullOrEmpty(v)) continue;
        //                    if (dic.ContainsKey(v))
        //                    {
        //                        attribute.Value = unNormalizeText(dic[v], attribute.Value, true);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    string path = GetPath(GetDirName("ModulesNew"));
        //    string newFile = path + fn + ".xml";
        //    //var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        //    //doc.ReplaceChild(declaration, doc.FirstChild);
        //    doc.Save(newFile);
        //}

        //private Dictionary<string, string> GetDictrionary(string dir)
        //{
        //    Dictionary<string, string> dicKey = null;
        //    Dictionary<string, string> dicValue = null;
        //    ReadDictionarys(dir, ref dicKey, ref dicValue);

        //    Dictionary<string, string> dicRet = new Dictionary<string, string>();

        //    foreach (var elem in dicKey)
        //    {
        //        var k = elem.Key;
        //        var newK = elem.Value;

        //        var newV = dicValue[k].Trim();
        //        dicRet.Add(newK, newV);
        //    }

        //    return dicRet;
        //}

        //private void ReadDictionarys(string dir, ref Dictionary<string, string> dicKey, ref Dictionary<string, string> dicValue)
        //{            
        //    string keyFile = dir + "Out.txt";
        //    string valueFile = dir + "Out_v_";
        //    {
        //        string step1Dir = GetDirName(dir + "Step1");
        //        string[] fileEntries = Directory.GetFiles(step1Dir);
        //        foreach (string fileName in fileEntries)
        //        {                 
        //            if (fileName.Contains(keyFile)) ReadDictionary(fileName, ref dicKey);
        //        }
        //    }
        //    {
        //        string step2Dir = GetDirName(dir + "Step2");
        //        string[] fileEntries = Directory.GetFiles(step2Dir);
        //        foreach (string fileName in fileEntries)
        //        {                    
        //            if (fileName.Contains(valueFile)) ReadDictionary(fileName, ref dicValue);
        //        }
        //    }
        //}

        //private void ReadDictionary(string file, ref Dictionary<string, string> dic)
        //{
        //    if (dic == null) dic = new Dictionary<string, string>();

        //    char[] chars = { ':' };
        //    char[] chars2 = { '.' };
        //    char[] chars3 = { ' ' };

        //    string[] lines = File.ReadAllLines(file);
        //    foreach (string line in lines)
        //    {
        //        var elems = line.Split(chars);
        //        if (elems == null || elems.Length < 2)
        //        {
        //            //Szansa 2
        //            elems = line.Split(chars2);
        //            if (elems == null || elems.Length < 2)
        //            {
        //                //Szansa 3
        //                elems = line.Split(chars3);
        //                if (elems == null || elems.Length < 2)
        //                    continue;
        //            }
        //        }


        //        var key = elems[0];
        //        key = key.Replace(" ", "");
        //        var val = string.Join(":", elems,1,elems.Length-1);

        //        if (dic.ContainsKey(key))
        //            continue;

        //        dic.Add(key, val);
        //    }
        //}

        //#endregion

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
