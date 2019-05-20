using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Thea2Translator.Helpers
{
    public class FileHelper
    {
        public static string MainDir = "";

        public static List<string> ReadFileLines(string file)
        {
            var ret = new List<string>();

            if (File.Exists(file))
                ret = File.ReadAllLines(file).ToList();

            return ret;
        }

        public static string[] GetFiles(string dir)
        {
            var path = GetDirName(dir);
            if (!Directory.Exists(path))
                return null;

            return Directory.GetFiles(GetDirName(dir));
        }

        public static void CreatedPathIfNotExists(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public static void DeleteFileIfExists(string fileFullPath)
        {
            if (File.Exists(fileFullPath)) File.Delete(fileFullPath);
        }

        public static void DeletePath(string path)
        {
            path = GetDirName(path);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static string GetCreatedPath(string path)
        {
            path = GetDirName(path);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + @"\";
        }

        public static string GetDirName(string dir)
        {
            return MainDir + @"\" + dir;
        }
    }
}
