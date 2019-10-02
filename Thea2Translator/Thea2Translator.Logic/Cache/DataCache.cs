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
        public IList<string> Authors { get; private set; }        
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

        public string GetFullPath()
        {
            return FullPath;
        }

        public string GetSummary(bool forPublication)
        {
            LoadFromFile();
            var statistic = LogicProvider.Statistic;
            statistic.Reload(this);

            string header = Type.ToString();
            if (forPublication)
            {
                if (IsDataBaseCache) header = "Nazwy, opisy, przyciski itp.";
                if (IsModulesCache) header = "Zdarzenia";
                if (IsNamesCache) header = "Imiona";
            }

            return $"{header}:\r\n{statistic.GetSummary(forPublication)}";
        }

        private void ResetElems()
        {
            CurrentId = 0;
            CacheElems = new List<CacheElem>();
        }        
        public void ReloadElems(bool withGroups = false, bool withVocabulary = false, bool withNavigation = false)
        {
            LoadFromFile();
            ReloadAuthors();

            if (withGroups) ReloadGroups();
            if (withVocabulary) ReloadVocabulary();
            if (withNavigation)
            {
                ReloadNavigation();
                UpdateAdventureNodeElems();
            }
        }

        private void LoadFromFile()
        {
            ResetElems();

            if (!FileHelper.FileExists(FullPath))
                return;

            var bookmarks = UserHelper.GetUserBookmarks(Type);
            XmlDocument doc = new XmlDocument();
            doc.Load(FullPath);
            var elements = doc.DocumentElement.GetElementsByTagName("Element");
            foreach (XmlNode element in elements)
            {
                var elem = new CacheElem(Type, element, bookmarks);
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

        private void ReloadAuthors()
        {
            Authors = new List<string>();

            foreach (var elem in CacheElems)
            {

                if (string.IsNullOrWhiteSpace(elem.ConfirmationUser) || Authors.Contains(elem.ConfirmationUser))
                    continue;

                Authors.Add(elem.ConfirmationUser);

            }

            ((List<string>)Authors).Sort();
        }

        private void ReloadNavigation()
        {
            if (Type != FilesType.Modules)
                return;

            Navigation = new Navigation();
            Navigation.Reload();
        }

        private void UpdateAdventureNodeElems()
        {
            if (Navigation == null)
                return;

            if (Type != FilesType.Modules)
                return;

            Navigation.UpdateAdventureNodeElems(this);
        }

        private void ReloadVocabulary()
        {
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
                cacheElem.SetChanged(false);
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
            if (step == AlgorithmStep.ExportToSteam) MakeExportToSteam(step);
            if (step == AlgorithmStep.ExportToSteamDebug) MakeExportToSteam(step);
        }

        #region ImportFromSteam
        private void MakeImportFromSteam()
        {
            StartProcess(AlgorithmStep.ImportFromSteam.ToString(), 3);
            ReloadElems();
            StartNextProcessStep();
            ReadSteamFiles(true);
            StartNextProcessStep();
            SaveElems();
            StopProcess();
        }
        private void ReadSteamFiles(bool withInactivation)
        {
            if (withInactivation)
            {
                foreach (var cacheElem in CacheElems)
                {
                    cacheElem.SetActivation(false);
                }
            }

            AlgorithmStep step = AlgorithmStep.ImportFromSteam;
            string[] files = FileHelper.GetFiles(GetDirectoryName(step));
            if (files == null) return;

            if (IsDataBaseCache) ReadSteamFilesDataBase(files, step);
            if (IsModulesCache) ReadSteamFileModules(files, step);
            if (IsNamesCache) ReadSteamFileNames();
        }
        private void ReadSteamFilesDataBase(string[] files, AlgorithmStep step)
        {
            foreach (string file in files)
            {
                ProcessFileDataBase(file, step);
            }
        }
        private void ReadSteamFileModules(string[] files, AlgorithmStep step)
        {
            Navigation = new Navigation();
            foreach (string file in files)
            {
                ProcessFileModules(file, step);
            }
            Navigation.SaveElems();
        }
        private void ReadSteamFileNames()
        {
            string path = FileHelper.GetCreatedPath(GetDirectoryName(AlgorithmStep.ImportFromSteam));

            string fileNames = path + "DATABASE_DES_NAMES.xml";
            string fileSubraces = path + "DATABASE_SUBRACE.xml";

            ProcessFileNames(fileNames);
            ProcessFileSubraces(fileSubraces);
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
            StartProcess(AlgorithmStep.ImportFromMachineTranslate.ToString(), 3);
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
        private void MakeExportToSteam(AlgorithmStep step)
        {
            StartProcess(step.ToString(), 4);
            ReloadElems();
            StartNextProcessStep();
            DeleteFilesToSteam(step);
            StartNextProcessStep();
            SaveElems();
            StartNextProcessStep();
            SaveFilesToSteam(step);
            StopProcess();
        }
        private void DeleteFilesToSteam(AlgorithmStep step)
        {
            FileHelper.DeletePath(GetDirectoryName(step));
        }
        private void SaveFilesToSteam(AlgorithmStep step)
        {
            if (IsNamesCache)
            {
                ProcessFileNamesSave();
                return;
            }

            string[] files = FileHelper.GetFiles(GetDirectoryName(AlgorithmStep.ImportFromSteam));
            if (files == null) return;
            int filesCount = files.Length;
            foreach (string file in files)
            {
                if (IsDataBaseCache) ProcessFileDataBase(file, step);
                if (IsModulesCache) ProcessFileModules(file, step);
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

        private void ProcessFileDataBase(string file, AlgorithmStep step)
        {
            if (!IsFileToProcess(file))
                return;

            bool saveToFile = (step == AlgorithmStep.ExportToSteam || step == AlgorithmStep.ExportToSteamDebug);
            bool debugMode = (step == AlgorithmStep.ExportToSteamDebug);

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
                        elem.TryUpdateValue(val, true);
                        continue;
                    }

                    GetUpdatedOrCreatedElem(key, val, TextHelper.GetGroupsFromKey(key, false), true);
                }
                else
                {
                    var elem = GetElem(key);
                    if (elem == null) continue;

                    var text = elem.OutputText;
                    if (debugMode) text = $"[{elem.Id}] {text}";

                    entry.Attributes["Val"].Value = text;
                }
            }

            if (saveToFile)
            {
                string path = FileHelper.GetCreatedPath(GetDirectoryName(step));
                string newFile = path + Path.GetFileName(file);
                doc.Save(newFile);
            }
        }
        private void ProcessFileModules(string file, AlgorithmStep step)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName == "sAbandoned lumbermil")
                return;

            bool saveToFile = (step == AlgorithmStep.ExportToSteam || step == AlgorithmStep.ExportToSteamDebug);
            bool debugMode = (step == AlgorithmStep.ExportToSteamDebug);
            if (saveToFile)
            {
                string newFile = FileHelper.GetCopiedFile(file, GetDirectoryName(step));

                string text = File.ReadAllText(newFile);
                text = text.Replace("encoding=\"Windows-1252\"", "encoding =\"UTF-8\"");
                File.WriteAllText(newFile, text);
                file = newFile;
            }

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
                        var nodeElem = GetUpdatedOrCreatedElem(inputText, TextHelper.GetGroupsFromKey(group, true), true);
                        nodeElem.TryUpdateValue(inputText, false);
                        Navigation.SetNodeElementId(nodeElem.Id, adventureInfo, adventureNodeId);
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
                                var text = elem.OutputText;
                                if (debugMode) text = $"[{elem.Id}] {text}";
                                child.InnerText = text;
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
                            var outputElem = GetUpdatedOrCreatedElem(inputTextName, TextHelper.GetGroupsFromKey(group, true), true);
                            outputElem.TryUpdateValue(inputTextName, false);
                            Navigation.SetOutputElementId(outputElem.Id, adventureInfo, adventureNodeId, targetID);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(inputTextName))
                            {
                                var key = inputTextName;
                                var elem = GetElem(key);
                                if (elem != null)
                                {
                                    var text = elem.OutputText;
                                    if (debugMode) text = $"[{elem.Id}] {text}";
                                    output.Attributes["name"].Value = text;
                                }
                            }
                        }
                    }
                }
            }

            if (saveToFile)
            {
                doc.Save(file);
            }
        }
        private void ProcessFileNames(string file)
        {
            ((List<CacheElem>)(CacheElems)).RemoveAll(x => x.IsGenericName);

            var newCacheElems = new List<CacheElem>();

            var nameGenerator = new NameGenerator();
            nameGenerator.LoadFromFile();

            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            var collections = doc.DocumentElement.ChildNodes;
            foreach (XmlNode collection in collections)
            {
                var collectionName = collection.Name;
                var nameGeneratorElem = nameGenerator.GetElemByCollectionName(collectionName);
                var collectionElems = collection.ChildNodes;

                bool elemActivation = true;
                bool groupConfirmation = false;
                if (nameGeneratorElem != null)
                {
                    elemActivation = !nameGeneratorElem.IsDeactivation;
                    groupConfirmation = nameGeneratorElem.IsConfirmation;
                }

                var raceNames = new List<string>();
                var subRaceNames = new List<string>();

                foreach (XmlNode collectionElem in collectionElems)
                {
                    var collectionElemName = collectionElem.Name;

                    if (collectionElem.Attributes == null)
                        continue;

                    var name = collectionElem.Attributes["Value"]?.Value;
                    var gender = "";

                    switch (collectionElemName)
                    {
                        case "Race": raceNames.Add(name); break;
                        case "Subrace": subRaceNames.Add(name); break;
                        case "CharacterMale":
                        case "CharacterFemale":
                            gender = collectionElemName;
                            break;
                        default:
                            throw new Exception($"Cos nie tak '{collectionElemName}'");
                    }

                    if (string.IsNullOrEmpty(gender))
                        continue;

                    var key = CacheElem.GetNameKey(collectionName, string.Join("_", raceNames.ToArray()), subRaceNames, gender, name);

                    var elem = GetElem(key);
                    if (elem != null)
                    {
                        elem.TryUpdateValue(name, elemActivation);
                    }
                    else
                    {
                        var groups = CacheElem.GetNameGroups(collectionName, string.Join("_", raceNames.ToArray()), subRaceNames, gender);
                        elem = GetUpdatedOrCreatedElem(key, name, groups, elemActivation);
                    }
                    if (elem == null) throw new Exception($"Cos nie tak :-( '{collectionElemName}'");

                    elem.SetActivation(elemActivation);
                    elem.SetGender(gender);
                    if (!elem.IsCorrectedByHuman) elem.SetConfirmedtion(groupConfirmation);

                    newCacheElems.Add(elem);
                }

                // Na koniec dodaje z generatora
                if (nameGeneratorElem != null)
                {
                    nameGeneratorElem.InsertCacheElems(newCacheElems);
                    continue;
                }
            }

            CacheElems = newCacheElems;
        }
        private void ProcessFileSubraces(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            var subraces = doc.DocumentElement.ChildNodes;
            var subraceInfos = new List<SubraceInfo>();
            var excludedRaces = new List<RaceType> { RaceType.HUMAN, RaceType.ELF, RaceType.ORC, RaceType.GOBLIN, RaceType.DWARF, RaceType.CONCEPT };
            foreach (XmlNode subrace in subraces)
            {
                var subraceInfo = SubraceInfo.ReadFromXml(subrace);
                if (subraceInfo == null)
                    continue;

                if (subraceInfo.HasRace(excludedRaces))
                    continue;

                if (subraceInfo.UseAllCharacters())
                    continue;

                subraceInfos.Add(subraceInfo);
            }

            var subracesElems = new Dictionary<string, List<CacheElem>>();
            foreach (var cacheElem in CacheElems)
            {
                if (cacheElem.IsGenericName) continue;
                if (cacheElem.IsInactive) continue;
                var elemSubraces = cacheElem.GetNameSubraces();
                foreach (var elemSubrace in elemSubraces)
                {
                    if (!subracesElems.ContainsKey(elemSubrace))
                        subracesElems.Add(elemSubrace, new List<CacheElem>());

                    subracesElems[elemSubrace].Add(cacheElem);
                }
            }

            var errors = new List<string>();
            foreach (var subraceInfo in subraceInfos)
            {
                bool hideFemale = !subraceInfo.UseCharacterFemale();
                bool hideMale = !subraceInfo.UseCharacterMale();

                if (!subracesElems.ContainsKey(subraceInfo.Name))
                {
                    errors.Add(subraceInfo.Name);
                    continue;
                }

                var elems = subracesElems[subraceInfo.Name];
                if (elems == null || elems.Count == 0)
                    continue;

                foreach (var elem in elems)
                {
                    if (elem.IsFemale && hideFemale) elem.SetActivation(false);
                    if (elem.IsMale && hideMale) elem.SetActivation(false);
                }
            }

            var errorsString = string.Join("\r\n", errors.ToArray());
            SaveElems();
        }
                
        private void ProcessFileNamesSave()
        {            
            string path = FileHelper.GetCreatedPath(GetDirectoryName(AlgorithmStep.ExportToSteam));
            string newFile = path + "DATABASE_DES_NAMES.xml";

            FileHelper.DeleteFileIfExists(newFile);

            XmlDocument doc = new XmlDocument();
            XmlNode mainNode = doc.CreateElement("CHARACTER_NAMES");
            doc.AppendChild(mainNode);

            var nameSaver = new NameSaver(CacheElems);
                        
            foreach (var elem in nameSaver.NameSaverElems)
            {
                mainNode.AppendChild(elem.ToXmlNode(doc));
            }

            doc.Save(newFile);
        }

        private CacheElem GetElem(string key)
        {
            CacheElem ret = null;

            var keyToCompare = TextHelper.PrepereToCompare(key);
            foreach (var elem in CacheElems)
            {
                if (elem.EqualsPreparedTexts(keyToCompare)) return elem;
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

        private CacheElem GetUpdatedOrCreatedElem(string value, List<string> groups, bool withActivation)
        {
            return GetUpdatedOrCreatedElem(value, value, groups, withActivation);
        }

        private CacheElem GetUpdatedOrCreatedElem(string key, string value, List<string> groups, bool withActivation)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            var elem = GetElem(key);
            if (elem != null)
                elem.AddGroups(groups);
            else
            {
                elem = CreateElem(key, value, groups);
                CacheElems.Add(elem);
            }
            if (withActivation) elem.SetActivation(true);
            return elem;
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
                case AlgorithmStep.ExportToSteamDebug: sufix = "ToSteamDebug"; break;
            }
            dir += sufix;
            return dir;
        }

        public static bool FilesExists(FilesType type)
        {
            if (type == FilesType.DataBase || type == FilesType.Modules || type == FilesType.Names)
            {
                var dataCache = new DataCache(type);
                return dataCache.FilesExists();
            }

            return false;
        }

        public bool FilesExists()
        {
            return FileHelper.FileExists(FullPath);
        }

        public static bool HasConflicts(FilesType type)
        {
            if (type == FilesType.Vocabulary)
                return Vocabulary.HasConflicts();

            var dataCache = new DataCache(type);
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

            if (type == FilesType.NamesGenerator)
            {
                NameGenerator.MergeCache();
                return;
            }

            var original = new DataCache(type, DirectoryType.Original);
            var originalOld = new DataCache(type, DirectoryType.OriginalOld);
            var cacheOld = new DataCache(type, DirectoryType.CacheOld);

            if (!original.FilesExists() && !originalOld.FilesExists() && !cacheOld.FilesExists())
                return;

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

        public IList<string> GetStartingGroups()
        {
            if (Navigation == null)
                return Groups;

            return Navigation.GetStartingGroups();
        }
    }
}
