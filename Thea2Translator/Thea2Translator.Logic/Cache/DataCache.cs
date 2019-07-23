using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Thea2Translator.Logic.Helpers;

namespace Thea2Translator.Logic
{
    internal class DataCache : ProcessHelper, IDataCache
    {
        internal const int LinesInFile = 6000;

        public IList<CacheElem> CacheElems { get; private set; }                
        public IList<string> Groups { get; private set; }
        public Vocabulary Vocabulary { get; private set; }
        public Navigation Navigation { get; private set; }
        
        private readonly FilesType Type;
        private readonly string FullPath;
        private int CurrentId;

        internal bool IsDataBaseCache { get { return Type == FilesType.DataBase; } }
        internal bool IsModulesCache { get { return Type == FilesType.Modules; } }
        internal bool IsNamesCache { get { return Type == FilesType.Names; } }

        public DataCache(FilesType type, DirectoryType directoryType = DirectoryType.Cache)
        {
            Type = type;
            FullPath = FileHelper.GetLocalFilePatch(directoryType, type);
            ResetElems();
        }

        public string GetSummary()
        {            
            LoadFromFile();
            var statistic = LogicProvider.Statistic;
            statistic.Reload(this);
            
            return $"{Type.ToString()}:\r\n{statistic.GetSummary()}";
        }
        
        private void ResetElems()
        {
            CurrentId = 0;
            CacheElems = new List<CacheElem>();
        }

        public void ReloadElems(bool withGroups = false, bool withVocabulary = false)
        {
            LoadFromFile();

            if (withGroups) ReloadGroups();
            if (withVocabulary) ReloadVocabulary();
        }

        private void LoadFromFile()
        {
            ResetElems();

            if (!FileHelper.FileExists(FullPath))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(FullPath);
            var elements = doc.DocumentElement.GetElementsByTagName("Element");
            foreach (XmlNode element in elements)
            {
                var elem = new CacheElem(Type, element);
                CurrentId = Math.Max(CurrentId, elem.Id);
                AddElem(elem);
            }
        }

        protected void AddElem(CacheElem elem)
        {
            CacheElems.Add(elem);
        }

        protected void RemoveElem(CacheElem elem)
        {
            if (elem == null)
                return;

            CacheElems.Remove(elem);
        }

        private void ReloadGroups()
        {
            Groups = new List<string>();

            foreach (var elem in CacheElems)
            {
                foreach (var group in elem.Groups)
                {
                    if (Groups.Contains(group))
                        continue;

                    Groups.Add(group);
                }
            }

            ((List<string>)Groups).Sort();
        }

        private void ReloadVocabulary()
        {
            if (Type == FilesType.Names)
                return;

            Vocabulary = new Vocabulary(Type);
            Vocabulary.Reload(this);
        }

        public void UpdateVocabulary(Vocabulary vocabulary)
        {
            Vocabulary = vocabulary;
        }

        public void SaveElems(bool withVocabulary = false)
        {
            SaveToFile();
            if (withVocabulary) SaveVocabulary();
        }

        private void SaveToFile()
        {
            FileHelper.CreatedPathIfNotExists(FullPath);
            FileHelper.DeleteFileIfExists(FullPath);

            XmlDocument doc = new XmlDocument();
            XmlNode databaseNode = doc.CreateElement("Database");
            doc.AppendChild(databaseNode);

            foreach (var cacheElem in CacheElems)
            {
                databaseNode.AppendChild(cacheElem.ToXmlNode(doc));
            }

            doc.Save(FullPath);
        }

        private void SaveVocabulary()
        {
            if (Vocabulary == null)
                return;

            Vocabulary.SaveElems();
        }

        public void MakeStep(AlgorithmStep step)
        {
            if (step == AlgorithmStep.ImportFromSteam) MakeImportFromSteam();
            if (step == AlgorithmStep.PrepareToMachineTranslate) MakePrepareToMachineTranslate();
            if (step == AlgorithmStep.ImportFromMachineTranslate) MakeImportFromMachineTranslate();
            if (step == AlgorithmStep.ExportToSteam) MakeExportToSteam();
        }

        #region ImportFromSteam
        private void MakeImportFromSteam()
        {
            StartProcess(AlgorithmStep.ImportFromSteam.ToString(), 3);
            ReloadElems();
            StartNextProcessStep();
            ReadSteamFiles();
            StartNextProcessStep();
            SaveElems();
            StopProcess();
        }
        private void ReadSteamFiles()
        {
            string[] files = FileHelper.GetFiles(GetDirectoryName(AlgorithmStep.ImportFromSteam));
            if (files == null) return;

            Navigation = new Navigation();

            foreach (string file in files)
            {
                if (IsDataBaseCache) ProcessFileDataBase(file, false);
                if (IsModulesCache) ProcessFileModules(file, false);
                if (IsNamesCache) ProcessFileNames(file, false);
            }

            Navigation.SaveElems();
        }
        #endregion
        #region PrepareToMachineTranslate
        private void MakePrepareToMachineTranslate()
        {
            StartProcess(AlgorithmStep.PrepareToMachineTranslate.ToString(), 3);
            ReloadElems();
            StartNextProcessStep();
            DeleteFilesToMachineTranslate();
            StartNextProcessStep();
            SaveFilesToMachineTranslate();
            StopProcess();
        }
        private void DeleteFilesToMachineTranslate()
        {
            FileHelper.DeletePath(GetDirectoryName(AlgorithmStep.PrepareToMachineTranslate));
        }
        private void SaveFilesToMachineTranslate()
        {
            string path = FileHelper.GetCreatedPath(GetDirectoryName(AlgorithmStep.PrepareToMachineTranslate));
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
                var v = elem.OriginalText;

                if (tw_v != null) tw_v.WriteLine($"{id}:{v}");
                i++;
            }

            if (tw_v != null) tw_v.Close();
            FileHelper.CreateDirectory(GetDirectoryName(AlgorithmStep.ImportFromMachineTranslate));
        }
        #endregion
        #region ImportFromMachineTranslate
        private void MakeImportFromMachineTranslate()
        {
            StartProcess(AlgorithmStep.ImportFromMachineTranslate.ToString(),3);
            ReloadElems();
            StartNextProcessStep();
            ReadMachineTranslatedFiles();
            StartNextProcessStep();
            SaveElems();
            StopProcess();
        }
        private void ReadMachineTranslatedFiles()
        {
            string[] files = FileHelper.GetFiles(GetDirectoryName(AlgorithmStep.ImportFromMachineTranslate));
            if (files == null) return;

            foreach (string file in files)
                ProcessMachineTranslatedFile(file);
        }
        private void ProcessMachineTranslatedFile(string file)
        {
            var lines = FileHelper.ReadFileLines(file);

            foreach (var line in lines)
            {
                var elems = line.Split(':');
                if (elems.Length <= 1) continue;
                var id = int.Parse(elems[0]);

                if (line == id + ":")
                    continue;

                var value = string.Join(":", elems, 1, elems.Length - 1);

                if (value[0] == ' ') value = value.Substring(1);

                var elem = GetElemById(id);
                if (elem == null) continue;
                elem.SetTranslated(value);
            }
        }
        #endregion
        #region ExportToSteam
        private void MakeExportToSteam()
        {
            StartProcess(AlgorithmStep.ExportToSteam.ToString(), 4);
            ReloadElems();
            StartNextProcessStep();
            DeleteFilesToSteam();
            StartNextProcessStep();
            SaveElems();
            StartNextProcessStep();
            SaveFilesToSteam();
            StopProcess();
        }
        private void DeleteFilesToSteam()
        {
            FileHelper.DeletePath(GetDirectoryName(AlgorithmStep.ExportToSteam));
        }
        private void SaveFilesToSteam()
        {
            string[] files = FileHelper.GetFiles(GetDirectoryName(AlgorithmStep.ImportFromSteam));
            if (files == null) return;
            int filesCount = files.Length;
            foreach (string file in files)
            {
                if (IsDataBaseCache) ProcessFileDataBase(file, true);
                if (IsModulesCache) ProcessFileModules(file, true);
                if (IsNamesCache) ProcessFileNames(file, true);
            }
        }
        #endregion
        private string GetMainNodesName(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            switch (fileName)
            {
                case "DATABASE_DES_LOCALIZATION": return "LOC_LIBRARY-DES";
                case "DATABASE_QUEST_LOCALIZATION": return "LOC_LIBRARY-QUEST";
                case "DATABASE_UI_LOCALIZATION": return "LOC_LIBRARY-UI";
            }

            return "";
        }
        private bool IsFileToProcess(string file)
        {
            return !string.IsNullOrEmpty(GetMainNodesName(file));
        }

        private void ProcessFileDataBase(string file, bool saveToFile)
        {
            if (!IsFileToProcess(file))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            var entrys = doc.SelectNodes($"//{GetMainNodesName(file)}/Entry");
            foreach (XmlNode entry in entrys)
            {
                if (entry.Attributes == null)
                    continue;

                var key = entry.Attributes["Key"]?.Value;

                if (!saveToFile)
                {
                    var val = entry.Attributes["Val"]?.Value;

                    var elem = GetElem(key);
                    if (elem != null)
                    {
                        elem.TryUpdateValue(val);
                        continue;
                    }

                    TryAddToCacheWithGroupAndGetId(key, val, TextHelper.GetGroupsFromKey(key, false));
                }
                else
                {
                    var elem = GetElem(key);
                    if (elem == null) continue;

                    entry.Attributes["Val"].Value = elem.OutputText;
                }
            }

            if (saveToFile)
            {
                string path = FileHelper.GetCreatedPath(GetDirectoryName(AlgorithmStep.ExportToSteam));
                string newFile = path + Path.GetFileName(file);
                doc.Save(newFile);
            }
        }
        private void ProcessFileModules(string file, bool saveToFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName == "sAbandoned lumbermil")
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            if (!saveToFile) Navigation.AddDocument(doc, fileName);
            var adventures = doc.DocumentElement.GetElementsByTagName("Adventure");
            foreach (XmlNode adventure in adventures)
            {
                if (adventure.Attributes == null)
                    continue;

                var adventureName = adventure.Attributes["name"]?.Value;
                var adventureId = adventure.Attributes["uniqueID"]?.Value;
                var adventureInfo = new NavigationElemAdventureInfo(fileName, adventureName, adventureId);

                var nodes = adventure.SelectNodes("nodes");
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null)
                        continue;

                    var xsi_type = node.Attributes["xsi:type"]?.Value;
                    if (string.IsNullOrEmpty(xsi_type) || xsi_type != "NodeAdventure")
                        continue;

                    var adventureNodeId = node.Attributes["ID"]?.Value;
                    var group = adventureInfo.GetGroupName(adventureNodeId);
                    var inputText = node.InnerText;
                    if (!saveToFile)
                    {
                        var nodeId = TryAddToCacheWithGroupAndGetId(inputText, TextHelper.GetGroupsFromKey(group, true));
                        Navigation.SetNodeElementId(nodeId, adventureInfo, adventureNodeId);
                    }
                    else
                    {
                        var key = inputText;
                        var elem = GetElem(key);
                        if (elem != null)
                        {
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                if (child.Name != "#text") continue;
                                child.InnerText = TextHelper.ReplacePolishChars(elem.OutputText);
                            }
                        }
                    }
                    
                    var outputs = node.SelectNodes("outputs");
                    foreach (XmlNode output in outputs)
                    {
                        if (output.Attributes == null)
                            continue;

                        var inputTextName = output.Attributes["name"]?.Value.ToString();
                        var ownerID = output.Attributes["ownerID"]?.Value.ToString();
                        var targetID = output.Attributes["targetID"]?.Value.ToString();

                        if (!saveToFile)
                        {
                            var outputId = TryAddToCacheWithGroupAndGetId(inputTextName, TextHelper.GetGroupsFromKey(group, true));
                            Navigation.SetOutputElementId(outputId, adventureInfo, adventureNodeId, targetID);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(inputTextName))
                            {
                                var key = inputTextName;
                                var elem = GetElem(key);
                                if (elem != null)
                                    output.Attributes["name"].Value = TextHelper.ReplacePolishChars(elem.OutputText);
                            }
                        }

                    }
                }
            }

            if (saveToFile)
            {
                string path = FileHelper.GetCreatedPath(GetDirectoryName(AlgorithmStep.ExportToSteam));
                string newFile = path + Path.GetFileName(file);
                doc.Save(newFile);
            }
        }
        private void ProcessFileNames(string file, bool saveToFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName != "DATABASE_DES_NAMES")
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            var collections = doc.DocumentElement.ChildNodes;
            foreach (XmlNode collection in collections)
            {
                var collectionName = collection.Name;
                collectionName = collectionName.Replace("NAME_COLLECTION-", "");
                var collectionElems = collection.ChildNodes;
                foreach (XmlNode collectionElem in collectionElems)
                {
                    var collectionElemName = collectionElem.Name;
                    if (collectionElemName != "CharacterMale" && collectionElemName != "CharacterFemale")
                        continue;

                    collectionElemName = $"{collectionName}_{collectionElemName}";

                    if (collectionElem.Attributes == null)
                        continue;

                    var collectionElemValue = collectionElem.Attributes["Value"]?.Value;
                    var key = $"{collectionElemValue}_{collectionElemName}";
                    if (!saveToFile)
                    {
                        var elem = GetElem(key);
                        if (elem != null)
                        {
                            elem.TryUpdateValue(collectionElemValue);
                            continue;
                        }
                        
                        var groups = new List<string>() { collectionName, collectionElemName };
                        TryAddToCacheWithGroupAndGetId(key, collectionElemValue, groups);
                    }
                    else
                    {
                        var elem = GetElem(key);
                        if (elem == null) continue;

                        collectionElem.Attributes["Value"].Value = elem.OutputText;
                    }
                }
            }

            if (saveToFile)
            {
                string path = FileHelper.GetCreatedPath(GetDirectoryName(AlgorithmStep.ExportToSteam));
                string newFile = path + Path.GetFileName(file);
                doc.Save(newFile);
            }
        }

        private CacheElem GetElem(string key)
        {
            CacheElem ret = null;

            foreach (var elem in CacheElems)
            {
                if (elem.EqualsTexts(key)) return elem;
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

        private CacheElem CreateElem(string key, string value, List<string> groups)
        {
            var elem = new CacheElem(Type, ++CurrentId, key, value);
            elem.AddGroups(groups);
            return elem;
        }

        private string GetFileName(string sufix = "")
        {
            var file = "DataBase";
            if (IsModulesCache) file = "Modules";
            if (IsNamesCache) file = "Names";
            return file += sufix;
        }

        private int TryAddToCacheWithGroupAndGetId(string value, List<string> groups)
        {
            return TryAddToCacheWithGroupAndGetId(value, value, groups);
        }

        private int TryAddToCacheWithGroupAndGetId(string key, string value, List<string> groups)
        {
            if (string.IsNullOrEmpty(key))
                return 0;

            var elem = GetElem(key);
            if (elem != null)
            {
                elem.AddGroups(groups);
                return elem.Id;
            }

            elem = CreateElem(key, value, groups);
            CacheElems.Add(elem);
            return elem.Id;
        }

        public string GetDirectoryName(AlgorithmStep step)
        {
            var dir = "DataBase";
            if (IsModulesCache) dir = "Modules";
            if (IsNamesCache) dir = "Names";
            string sufix = step.ToString();
            switch (step)
            {
                case AlgorithmStep.ImportFromSteam:
                    if (IsNamesCache) dir = "DataBase";
                    sufix = ""; break;
                case AlgorithmStep.PrepareToMachineTranslate: sufix = "ToMachineTranslate"; break;
                case AlgorithmStep.ImportFromMachineTranslate: sufix = "FromMachineTranslate"; break;
                case AlgorithmStep.ExportToSteam: sufix = "ToSteam"; break;
            }
            dir += sufix;
            return dir;
        }

        public static bool HasConflicts(FilesType type)
        {
            var dataCache = new DataCache(type);
            if (type == FilesType.Vocabulary)
                return Vocabulary.HasConflicts();

            return dataCache.HasConflicts();
        }

        protected bool HasConflicts()
        {
            LoadFromFile();
            foreach (var elem in CacheElems)
            {
                if (elem.HasConflict) return true;
            }

            return false;
        }

        public static void MergeCache(FilesType type)
        {
            if (type == FilesType.Vocabulary)
            {
                Vocabulary.MergeCache();
                return;
            }

            var original = new DataCache(type, DirectoryType.Original);
            var originalOld = new DataCache(type, DirectoryType.OriginalOld);
            var cacheOld = new DataCache(type, DirectoryType.CacheOld);

            original.LoadFromFile();
            originalOld.LoadFromFile();
            cacheOld.LoadFromFile();

            var cacheNew = new DataCache(type, DirectoryType.Cache);

            //-----------------------------------
            //  	O   |   OO	|	CO	|	CN	|
            //-----------------------------------
            //0. 	A	|	-	|	-	|	A	|
            //1.	A	|	A	|	A	|	A	|
            //2.	A	|	A	|	B	|	B	|
            //3.	A	|	B	|	B	|	A	|
            //4.	A	|	B	|	A	|	A	|
            //5.    A   |   B   |   C   |   ?   |
            //-----------------------------------

            foreach (var originalElem in original.CacheElems)
            {
                var id = originalElem.Id;
                var originalOldElem = originalOld.GetElemById(id);
                var cacheOldElem = cacheOld.GetElemById(id);

                if ((originalOldElem == null && cacheOldElem != null) || (cacheOldElem == null && originalOldElem != null))
                    throw new Exception($"Cos nie tak z ID={id}");

                //0. 	A	|	-	|	-	|	A	|
                if (originalOldElem == null && cacheOldElem == null)
                {
                    cacheNew.AddElem(originalElem);
                    continue;
                }

                //1.	A	|	A	|	A	|	A	|
                //2.	A	|	A	|	B	|	B	|
                if (CacheElem.IsEquals(originalElem, originalOldElem))
                {
                    cacheNew.AddElem(cacheOldElem);
                    originalOld.RemoveElem(originalOldElem);
                    cacheOld.RemoveElem(cacheOldElem);
                    continue;
                }

                //3.	A	|	B	|	B	|	A	|
                // !CacheElem.IsEquals(originalElem, originalOldElem)
                if (CacheElem.IsEquals(originalOldElem, cacheOldElem))
                {
                    cacheNew.AddElem(originalElem);
                    originalOld.RemoveElem(originalOldElem);
                    cacheOld.RemoveElem(cacheOldElem);
                    continue;
                }

                //4.	A	|	B	|	A	|	A	|
                // !CacheElem.IsEquals(originalElem, originalOldElem)
                // !CacheElem.IsEquals(originalOldElem, cacheOldElem)
                if (CacheElem.IsEquals(originalElem, cacheOldElem))
                {
                    cacheNew.AddElem(originalElem);
                    originalOld.RemoveElem(originalOldElem);
                    cacheOld.RemoveElem(cacheOldElem);
                    continue;
                }

                //5.    A   |   B   |   C   |   ?   |
                // !CacheElem.IsEquals(originalElem, originalOldElem)
                // !CacheElem.IsEquals(originalOldElem, cacheOldElem)
                // !CacheElem.IsEquals(originalElem, cacheOldElem))
                originalElem.SetConlfictWith(cacheOldElem);
                cacheNew.AddElem(originalElem);
                originalOld.RemoveElem(originalOldElem);
                cacheOld.RemoveElem(cacheOldElem);                
            }

            foreach (var oldElem in cacheOld.CacheElems)
            {
                var id = oldElem.Id;
                var originalOldElem = originalOld.GetElemById(id);

                if (originalOldElem != null)
                    throw new Exception($"Cos nie tak z ID={id} (2 petla)");
                                
                cacheNew.AddElem(oldElem);
            }

            cacheNew.SaveToFile();
        }

        public FilesType GetFileType()
        {
            return Type;
        }
    }
}
