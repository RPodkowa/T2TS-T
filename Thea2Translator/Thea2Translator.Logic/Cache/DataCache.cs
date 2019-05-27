using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Thea2Translator.Logic
{
    internal class DataCache : IDataCache
    {
        internal const string DataSeparator = "[::]";
        internal const int LinesInFile = 6000;

        public IList<CacheElem> CacheElems { get; private set; }
        public IList<string> Groups { get; private set; }
        public Vocabulary Vocabulary { get; private set; }

        private readonly FilesType Type;
        private readonly string FullPath;
        private int CurrentId;
        private int PartsOfStep;
        private int CurrentPartOfStep;

        public event Action<string, double> StatusChanged;

        internal bool IsDataBaseCache { get { return Type == FilesType.DataBase; } }
        internal bool IsModulesCache { get { return Type == FilesType.Modules; } }

        public DataCache(FilesType type)
        {
            Type = type;
            FullPath = $"{FileHelper.MainDir}\\Cache\\{type}.cache";
            ResetElems();
        }

        private void ChangeStatus(string status, double progress)
        {
            StatusChanged?.Invoke(status, progress);
        }

        private void UpdateStatusWithPortion(string status, int elemNumber, int elemCount)
        {
            UpdateStatus(status, elemNumber, elemCount, true);
        }

        private void UpdateStatus(string status)
        {
            UpdateStatus(status, 1, 2, false);
        }

        private void UpdateStatus(string status, int elemNumber, int elemCount, bool statusWithPortion)
        {
            double progress = 0;
            if (elemCount != 0) progress = (double)elemNumber / elemCount;
            if (PartsOfStep != 0)
            {
                progress *= ((double)1 / PartsOfStep);
                if (CurrentPartOfStep > 0) progress += ((double)(CurrentPartOfStep - 1) / PartsOfStep);
            }

            if (statusWithPortion) status += $" ({elemNumber}/{elemCount})";

            ChangeStatus(status, progress);
        }

        private void StartAlgorithmStep(int parts)
        {
            PartsOfStep = parts;
            CurrentPartOfStep = 1;
        }

        private void StartNextPart()
        {
            CurrentPartOfStep++;
        }

        private void StopAlgorithmStep()
        {
            PartsOfStep = 0;
            CurrentPartOfStep = 0;
            UpdateStatus($"Done", 1, 1, false);
        }

        private void ResetElems()
        {
            CurrentId = 0;
            CacheElems = new List<CacheElem>();
        }

        public void ReloadElems(bool withGroups = false, bool withVocabulary = false)
        {
            ResetElems();
            var lines = FileHelper.ReadFileLines(FullPath);
            UpdateStatus($"Read elems from cache '{lines.Count}'");
            foreach (var line in lines)
            {
                LoadElem(line);
            }

            if (withGroups) ReloadGroups();
            if (withVocabulary) ReloadVocabulary();
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
        }

        private void ReloadVocabulary()
        {
            Vocabulary = new Vocabulary();
            Vocabulary.Reload(this);
        }

        private void LoadElem(string line)
        {
            var elem = new CacheElem(Type, line);
            CurrentId = Math.Max(CurrentId, elem.Id);
            CacheElems.Add(elem);
        }

        public void SaveElems(bool withVocabulary = false)
        {
            UpdateStatus($"SaveElemsToFile '{FullPath}'");
            FileHelper.SaveElemsToFile(CacheElems, FullPath);
            if (withVocabulary) SaveVocabulary();
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
            StartAlgorithmStep(3);
            ReloadElems();
            StartNextPart();
            ReadSteamFiles();
            StartNextPart();
            SaveElems();
            StopAlgorithmStep();
        }
        private void ReadSteamFiles()
        {
            string[] files = FileHelper.GetFiles(GetDirectoryName(AlgorithmStep.ImportFromSteam));
            if (files == null) return;
            int filesCount = files.Length;
            int currentFile = 1;
            foreach (string file in files)
            {
                UpdateStatusWithPortion($"Process file '{file}'", currentFile++, filesCount);
                if (IsDataBaseCache) ProcessFileDataBase(file, false);
                if (IsModulesCache) ProcessFileModules(file, false);
            }
        }
        #endregion
        #region PrepareToMachineTranslate
        private void MakePrepareToMachineTranslate()
        {
            StartAlgorithmStep(3);
            ReloadElems();
            StartNextPart();
            DeleteFilesToMachineTranslate();
            StartNextPart();
            SaveFilesToMachineTranslate();
            StopAlgorithmStep();
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
                var v = elem.OriginalNormalizedText;

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
            StartAlgorithmStep(3);
            ReloadElems();
            StartNextPart();
            ReadMachineTranslatedFiles();
            StartNextPart();
            SaveElems();
            StopAlgorithmStep();
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
            StartAlgorithmStep(4);
            ReloadElems();
            StartNextPart();
            DeleteFilesToSteam();
            StartNextPart();
            SaveElems();
            StartNextPart();
            SaveFilesToSteam();
            StopAlgorithmStep();
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
            int currentFile = 1;
            foreach (string file in files)
            {
                UpdateStatusWithPortion($"Process file '{file}'", currentFile++, filesCount);
                if (IsDataBaseCache) ProcessFileDataBase(file, true);
                if (IsModulesCache) ProcessFileModules(file, true);
            }
        }
        #endregion
        private string GetMainNodesName(string file)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            switch (fileName)
            {
                case "DATABASE_DES_LOCALIZATION": return "LOC_LIBRARY-EN_DES";
                case "DATABASE_QUEST_LOCALIZATION": return "LOC_LIBRARY-EN_QUEST";
                case "DATABASE_UI_LOCALIZATION": return "LOC_LIBRARY-EN_UI";
            }

            return "LOC_LIBRARY-EN_DES";
        }

        private void ProcessFileDataBase(string file, bool saveToFile)
        {
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
                    TryAddToCacheWithGroup(key, val, TextHelper.GetGroupsFromKey(key));
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
                string path = FileHelper.GetCreatedPath(GetDirectoryName(AlgorithmStep.ExportToSteam));
                string newFile = path + Path.GetFileName(file);
                doc.Save(newFile);
            }
        }
        private void ProcessFileModules(string file, bool saveToFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            var fileName = Path.GetFileNameWithoutExtension(file);
            var adventures = doc.DocumentElement.GetElementsByTagName("Adventure");
            foreach (XmlNode adventure in adventures)
            {
                if (adventure.Attributes == null)
                    continue;

                var adventureName = adventure.Attributes["name"]?.Value;

                var nodes = adventure.SelectNodes("nodes");
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null)
                        continue;

                    var xsi_type = node.Attributes["xsi:type"]?.Value;
                    if (string.IsNullOrEmpty(xsi_type) || xsi_type != "NodeAdventure")
                        continue;

                    var adventureNodeId = node.Attributes["ID"]?.Value;
                    var group = $"{fileName}_{adventureName}_{adventureNodeId}";
                    var text = TextHelper.Normalize(node.InnerText, true);
                    if (!saveToFile)
                        TryAddToCacheWithGroup(text, group);
                    else
                    {
                        var key = text;
                        var elem = GetElem(key);
                        if (elem != null)
                        {
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                if (child.Name != "#text") continue;
                                child.InnerText = elem.TranslatedText;
                            }
                        }
                    }

                    var outputs = node.SelectNodes("outputs");
                    foreach (XmlNode output in outputs)
                    {
                        if (output.Attributes == null)
                            continue;

                        var name = TextHelper.Normalize(output.Attributes["name"].Value.ToString(), true);
                        if (!saveToFile)
                            TryAddToCacheWithGroup(name, group);
                        else
                        {
                            if (!string.IsNullOrEmpty(name))
                            {
                                var key = name;
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
            return file += sufix;
        }

        private void TryAddToCacheWithGroup(string value, List<string> groups)
        {
            TryAddToCacheWithGroup(value, value, groups);
        }

        private void TryAddToCacheWithGroup(string value, string group)
        {
            var groups = new List<string>() { group };
            TryAddToCacheWithGroup(value, value, groups);
        }

        private void TryAddToCacheWithGroup(string key, string value, List<string> groups)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var elem = GetElem(key);
            if (elem != null)
            {
                elem.AddGroups(groups);
                return;
            }

            CacheElems.Add(CreateElem(key, value, groups));
        }

        private string GetDirectoryName(AlgorithmStep step)
        {
            var dir = "DataBase";
            if (IsModulesCache) dir = "Modules";
            string sufix = step.ToString();
            switch (step)
            {
                case AlgorithmStep.ImportFromSteam: sufix = ""; break;
                case AlgorithmStep.PrepareToMachineTranslate: sufix = "ToMachineTranslate"; break;
                case AlgorithmStep.ImportFromMachineTranslate: sufix = "FromMachineTranslate"; break;
                case AlgorithmStep.ExportToSteam: sufix = "ToSteam"; break;
            }
            dir += sufix;
            return dir;
        }
    }
}
