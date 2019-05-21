using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Thea2Translator.Helpers;

namespace Thea2Translator.Cache
{
    public class DataCache
    {
        public const string DataSeparator = "[::]";
        public const int LinesInFile = 6000;

        private readonly FilesType Type;
        private readonly string FullPath;
        private int CurrentId;
        private List<CacheElem> CacheElems;

        public bool IsDataBaseCache { get { return Type == FilesType.DataBase; } }
        public bool IsModulesCache { get { return Type == FilesType.Modules; } }

        public DataCache(FilesType type)
        {
            Type = type;
            FullPath = $"{FileHelper.MainDir}\\Cache\\{type}.cache";
            ResetElems();
        }

        private void ResetElems()
        {
            CurrentId = 0;
            CacheElems = new List<CacheElem>();
        }

        public void ReloadElems()
        {
            ResetElems();
            var lines = FileHelper.ReadFileLines(FullPath);
            foreach (var line in lines)
                LoadElem(line);
        }

        private void LoadElem(string line)
        {
            var elem = new CacheElem(Type, line);
            CurrentId = Math.Max(CurrentId, elem.Id);
            CacheElems.Add(elem);
        }

        public void SaveElems()
        {
            FileHelper.SaveElemsToFile(CacheElems, FullPath);
        }

        public void MakeStep(int step)
        {
            if (step == 1) MakeStep1();
            if (step == 2) MakeStep2();
        }

        #region Step1
        private void MakeStep1()
        {
            ReloadElems();
            DeleteFilesStep1();
            ReadFilesStep1();
            SaveElems();
            SaveFilesStep1();
        }
        private void DeleteFilesStep1()
        {
            FileHelper.DeletePath(GetDirectoryName("Step1"));
        }
        private void ReadFilesStep1()
        {
            string[] files = FileHelper.GetFiles(GetDirectoryName());
            if (files == null) return;
            foreach (string file in files)
            {
                if (IsDataBaseCache) ProcessFileDataBase(file, false);
                if (IsModulesCache) ProcessFileModules(file, false);
            }
        }
        private void SaveFilesStep1()
        {
            string path = FileHelper.GetCreatedPath(GetDirectoryName("Step1"));
            string file = GetFileName("Out");

            TextWriter tw_v = null;
            int i = 0;
            int f = 0;
            foreach (var elem in CacheElems)
            {
                if (!elem.ToTranslate)
                    continue;

                if (i % LinesInFile == 0)
                {
                    if (tw_v != null) tw_v.Close();
                    tw_v = new StreamWriter($"{path}{file}_v_{f++}.txt", true);
                }

                var id = elem.Id;
                var v = elem.OriginalNormalizedText;

                if (tw_v != null) tw_v.WriteLine($"{id}:{v}");
                i++;
            }

            if (tw_v != null) tw_v.Close();
        }
        #endregion
        #region Step2
        private void MakeStep2()
        {
            ReloadElems();
            DeleteFilesStep2();
            ReadFilesStep2();
            SaveElems();
            SaveFilesStep2();
        }
        private void DeleteFilesStep2()
        {
            FileHelper.DeletePath(GetDirectoryName("New"));
        }

        private void ReadFilesStep2()
        {
            string[] files = FileHelper.GetFiles(GetDirectoryName("Step2"));
            if (files == null) return;

            foreach (string file in files)            
                ProcessFile2(file);            
        }
        private void ProcessFile2(string file)
        {
            var lines = FileHelper.ReadFileLines(file);

            foreach (var line in lines)
            {                
                var elems = line.Split(':');
                if (elems.Length <= 1) continue;
                var id = int.Parse(elems[0]);
                var value = string.Join(":", elems, 1, elems.Length - 1);

                var elem = GetElemById(id);
                if (elem == null) continue;
                elem.SetTranslated(value);
            }
        }
        private void SaveFilesStep2()
        {
            string[] files = FileHelper.GetFiles(GetDirectoryName());
            if (files == null) return;
            foreach (string file in files)
            {
                if (IsDataBaseCache) ProcessFileDataBase(file, true);
                if (IsModulesCache) ProcessFileModules(file, true);
            }
        }
        #endregion
               
        private void ProcessFileDataBase(string file, bool saveToFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            var entrys = doc.SelectNodes("//LOC_LIBRARY-EN_DES/Entry");
            foreach (XmlNode entry in entrys)
            {
                if (entry.Attributes == null)
                    continue;

                var key = entry.Attributes["Key"]?.Value;

                if (!saveToFile)
                {
                    var val = entry.Attributes["Val"]?.Value;
                    TryAddToCache(key, val);
                }
                else
                {
                    var elem = GetElem(key);
                    if (elem == null) continue;

                    entry.Attributes["Val"].Value = elem.TranslatedText;
                }
            }

            if (saveToFile)
            {
                string path = FileHelper.GetCreatedPath(GetDirectoryName("New"));
                string newFile = path + Path.GetFileName(file);
                doc.Save(newFile);
            }
        }
        private void ProcessFileModules(string file, bool saveToFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            var adventures = doc.DocumentElement.GetElementsByTagName("Adventure");
            foreach (XmlNode adventure in adventures)
            {
                var nodes = adventure.SelectNodes("nodes");
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null)
                        continue;

                    var xsi_type = node.Attributes["xsi:type"]?.Value;
                    if (string.IsNullOrEmpty(xsi_type) || xsi_type != "NodeAdventure")
                        continue;

                    if (!saveToFile)                    
                        TryAddToCache(node.InnerText);                    
                    else
                    {
                        var key = TextHelper.Normalize(node.InnerText);
                        var elem = GetElem(key);
                        if (elem != null)
                            node.InnerText = elem.TranslatedText;
                    }

                    var outputs = adventure.SelectNodes("outputs");
                    foreach (XmlNode output in outputs)
                    {
                        if (output.Attributes == null)
                            continue;

                        if (!saveToFile)
                            TryAddToCache(output.Attributes["name"].ToString());
                        else
                        {                            
                            string name = output.Attributes["name"].ToString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                var key = TextHelper.Normalize(name);
                                var elem = GetElem(key);
                                if (elem != null)
                                    output.Attributes["name"].Value = elem.TranslatedText;
                            }
                        }
                    }
                }
            }

            if (saveToFile)
            {
                string path = FileHelper.GetCreatedPath(GetDirectoryName("New"));
                string newFile = path + Path.GetFileName(file);
                doc.Save(newFile);
            }            
        }

        private bool ContainsElem(string key)
        {
            return (GetElem(key) != null);
        }

        private CacheElem GetElem(string key)
        {
            CacheElem ret = null;

            foreach (var elem in CacheElems)
            {
                if (elem.Key == key) return elem;
            }

            return ret;
        }
        private CacheElem GetElemById(int id)
        {
            CacheElem ret = null;

            foreach (var elem in CacheElems)
            {
                if (elem.Id == id) return elem;
            }

            return ret;
        }

        private CacheElem CreateElem(string key, string value)
        {
            return new CacheElem(Type, ++CurrentId, key, value);
        }

        private string GetFileName(string sufix = "")
        {
            return GetDirectoryName(sufix);
        }

        private void TryAddToCache(string value)
        {
            TryAddToCache(value, value);
        }

        private void TryAddToCache(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (ContainsElem(key))
                return;

            CacheElems.Add(CreateElem(key, value));
        }

        private string GetDirectoryName(string sufix = "")
        {
            var dir = "DataBase";
            if (IsModulesCache) dir = "Modules";
            dir += sufix;
            return dir;
        }
    }
}
