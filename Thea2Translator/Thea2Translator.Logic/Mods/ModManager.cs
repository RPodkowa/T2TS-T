using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Thea2Translator.Logic.Mods
{
    public class ModManager
    {
        private const string EVENT = "-- [EVENT] --";
        private const string NODE = "+[NODE]";
        private const string STORY = "[STORY]";
        private const string OUT = "[OUT]";
        private const string EVENT_END = "[/EVENT]";
        private const string NODE_END = "[/NODE]";
        private const string STORY_END = "[/STORY]";

        public string Title { get; private set; }
        public string Body { get; private set; }

        private readonly string PatternPath;
        private readonly string OutputPath;

        public ModManager(string title, string body, string summary)
        {
            Title = title;
            Body = $"v{DateTime.Now.ToString("yyyyMMdd.HHmmss")}\r\n\r\n{body}\r\n\r\n-----------------\r\n{summary}";

            PatternPath = "Mods\\Pattern";
            OutputPath = "Mods\\New";
        }

        public void PrepareMod()
        {
            FileHelper.DeletePath(OutputPath);
            FileHelper.CreateDirectory(OutputPath);
            var files = FileHelper.GetFiles(PatternPath);
            var infoFile = "";
            foreach (string file in files)
            {
                var newFile = FileHelper.GetCopiedFile(file, OutputPath);
                var fileName = Path.GetFileName(newFile);
                if (fileName == "info.txt")
                    infoFile = newFile;
            }

            var dbFiles = PrapareDatabase();
            var moduleFiles = PrapareModules();
            PrepareInfoFile(infoFile, dbFiles, moduleFiles);
        }

        private void PrepareInfoFile(string infoFile, List<string> dbFiles, List<string> moduleFiles)
        {
            string text = File.ReadAllText(infoFile);
            text = InsertFilesLists(text, "\t", dbFiles, moduleFiles);
            text = text.Replace("[NAME]", Title);
            text = text.Replace("[BODY]", Body);
            File.WriteAllText(infoFile, text);
        }

        private string InsertFilesLists(string text, string formatString, List<string> dbFiles, List<string> moduleFiles)
        {
            var lines = new List<string>();
            //<DB>DATABASE_SUBRACE.xml</DB>
            foreach (var dbFile in dbFiles)
            {
                lines.Add($"{formatString}<DB>{dbFile}</DB>");
            }

            //<LOCALIZATION>TestsModule.txt</LOCALIZATION>
            foreach (var moduleFile in moduleFiles)
            {
                lines.Add($"{formatString}<LOCALIZATION>{moduleFile}</LOCALIZATION>");
            }

            var filesTxt = string.Join("\r\n", lines);
            return text.Replace($"{formatString}[FILES]", filesTxt);
        }

        private List<string> PrapareDatabase()
        {
            var ret = new List<string>();
            LogicProvider.DataBase.MakeStep(AlgorithmStep.ExportToSteam);
            var toSteamPath = LogicProvider.DataBase.GetDirectoryName(AlgorithmStep.ExportToSteam);
            var files = FileHelper.GetFiles(toSteamPath);
            foreach (string file in files)
            {
                FileHelper.CopyFile(file, OutputPath);
                var preparedFile = Path.GetFileName(file);
                if (string.IsNullOrEmpty(preparedFile))
                    continue;

                ret.Add(preparedFile);
            }

            return ret;
        }

        private List<string> PrapareModules()
        {
            var ret = new List<string>();
            //LogicProvider.Modules.MakeStep(AlgorithmStep.ExportToSteam);
            var toSteamPath = LogicProvider.Modules.GetDirectoryName(AlgorithmStep.ExportToSteam);
            var moduleFiles = FileHelper.GetFiles(toSteamPath);
            foreach (string moduleFile in moduleFiles)
            {
                var preparedFile = PrapareModuleFile(moduleFile, OutputPath);
                if (string.IsNullOrEmpty(preparedFile))
                    continue;

                ret.Add(preparedFile);
            }

            return ret;
        }

        private static string PrapareModuleFile(string moduleFile, string OutputPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(moduleFile);
            if (fileName == "sAbandoned lumbermil")
                return null;

            if (fileName == "demon encounters") fileName = "Demon encounters";

            fileName = fileName + ".txt";
            OutputPath = FileHelper.GetDirName(OutputPath);
            StreamWriter file = new StreamWriter(OutputPath + "/" + fileName);
            StringBuilder sb = new StringBuilder();
            int i = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(moduleFile);
            var adventures = doc.DocumentElement.GetElementsByTagName("Adventure");
            foreach (XmlNode adventure in adventures)
            {
                if (adventure.Attributes == null)
                    continue;

                var adventureName = adventure.Attributes["name"]?.Value;
                sb.AppendLine($"{EVENT}{adventureName}({i++})");

                var nodes = adventure.SelectNodes("nodes");
                foreach (XmlNode node in nodes)
                {
                    if (node.Attributes == null)
                        continue;

                    var xsi_type = node.Attributes["xsi:type"]?.Value;
                    if (string.IsNullOrEmpty(xsi_type) || xsi_type != "NodeAdventure")
                        continue;

                    var adventureNodeId = node.Attributes["ID"]?.Value;
                    var inputText = node.InnerText;

                    sb.AppendLine($"{NODE}{adventureNodeId}");

                    sb.AppendLine(STORY);
                    sb.AppendLine(inputText);
                    sb.AppendLine(STORY_END);

                    var outputs = node.SelectNodes("outputs");
                    foreach (XmlNode output in outputs)
                    {
                        if (output.Attributes == null)
                            continue;

                        var inputTextName = output.Attributes["name"]?.Value.ToString();
                        sb.AppendLine($"{OUT}{inputTextName}");
                    }

                    sb.AppendLine(NODE_END);
                    sb.AppendLine("");
                }

                sb.AppendLine(EVENT_END);
                sb.AppendLine("");
            }

            file.Write(sb.ToString().ToCharArray());
            file.Close();
            return fileName;
        }
    }
}
