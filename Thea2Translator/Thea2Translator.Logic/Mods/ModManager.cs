using System;
using System.IO;

namespace Thea2Translator.Logic.Mods
{
    public class ModManager
    {
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
            foreach (string file in files)
            {
                var newFile = FileHelper.GetCopiedFile(file, OutputPath);
                var fileName = Path.GetFileName(newFile);
                if (fileName != "info.txt")
                    continue;

                string text = File.ReadAllText(newFile);
                text = text.Replace("[NAME]", Title);
                text = text.Replace("[BODY]", Body);
                File.WriteAllText(newFile, text);
            }

            LogicProvider.DataBase.MakeStep(AlgorithmStep.ExportToSteam);
            var toSteamPath = LogicProvider.DataBase.GetDirectoryName(AlgorithmStep.ExportToSteam);
            files = FileHelper.GetFiles(toSteamPath);
            foreach (string file in files)
            {
                FileHelper.CopyFile(file, OutputPath);
            }
        }
    }
}
