using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
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

        public static bool FileExists(string fileFullPath)
        {
            return File.Exists(fileFullPath);
        }

        public static void DeleteFileIfExists(string fileFullPath)
        {
            if (FileExists(fileFullPath)) File.Delete(fileFullPath);
        }

        public static void DeletePath(string path)
        {
            path = GetDirName(path);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static void CreateDirectory(string dir)
        {
            dir = GetDirName(dir);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        public static string GetCreatedPath(string path)
        {
            CreateDirectory(path);
            path = GetDirName(path);

            return path + @"\";
        }

        public static string GetDirName(string dir)
        {
            return MainDir + @"\" + dir;
        }

        public static void SaveElemsToFile(IEnumerable<CacheElem> elems, string path)
        {
            CreatedPathIfNotExists(path);
            DeleteFileIfExists(path);
            TextWriter tw = new StreamWriter(path, true);
            foreach (var elem in elems)
            {
                tw.WriteLine(elem.ToString());
            }

            tw.Close();
        }

        public static void SaveElemsToFile(IEnumerable<object> elems, string path)
        {
            CreatedPathIfNotExists(path);
            DeleteFileIfExists(path);
            TextWriter tw = new StreamWriter(path, true);
            foreach (var elem in elems)
            {
                tw.WriteLine(elem.ToString());
            }

            tw.Close();
        }
    }
}
