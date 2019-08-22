using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public class FileHelper
    {
        private const string httpServerAdres = @"http://www.thea2pl.webd.pro/thea2pl.webd.pro/translator";
        private const string ftpServerAdres = @"ftp://ftp.thea2pl.webd.pro";
        private const string ftpUser = "translator@thea2pl.webd.pro";
        private const string ftpPassword = "vh+4{zBE=}69";

        public static bool TestMode = false;
        public static string MainDir = "";
        private static string BackupDirectoryName = "";

        public static List<string> ReadFileLines(string file)
        {
            var ret = new List<string>();

            if (File.Exists(file))
                ret = File.ReadAllLines(file).ToList();

            return ret;
        }

        public static string ReadFileString(string file)
        {
            var ret = "";
            if (File.Exists(file))
                ret = File.ReadAllText(file);

            return ret;
        }

        public static string[] GetFiles(string dir)
        {
            var path = GetDirName(dir);
            if (!Directory.Exists(path))
                return null;

            return Directory.GetFiles(GetDirName(dir));
        }

        public static string GetCopiedFile(string file, string destinationDirectory)
        {
            if (!File.Exists(file))
                return "";

            destinationDirectory = GetDirName(destinationDirectory);
            var fileName = Path.GetFileName(file);
            var newFile = Path.Combine(destinationDirectory, fileName);
            DeleteFileIfExists(newFile);
            if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);
            File.Copy(file, newFile);
            return newFile;
        }

        public static void CopyFile(string file, string destinationDirectory)
        {
            var newFile = GetCopiedFile(file, destinationDirectory);
        }

        public static void CopyFile(string file, DirectoryType destinationDirectoryType)
        {
            var newFile = GetCopiedFile(file, GetDirectoryName(destinationDirectoryType));
        }

        public static void PrepareBackupDirectory(bool forDownload)
        {
            BackupDirectoryName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (forDownload) BackupDirectoryName += "_Download";

            string backupDirectory = GetLocalFilePatch(DirectoryType.Backup.ToString());
            if (!Directory.Exists(backupDirectory)) Directory.CreateDirectory(backupDirectory);
            backupDirectory = GetLocalDirectoryPatch(DirectoryType.Backup);
            if (!Directory.Exists(backupDirectory)) Directory.CreateDirectory(backupDirectory);
        }

        public static void MoveFiles(DirectoryType sourceDirectoryType, DirectoryType destinationDirectoryType)
        {
            var files = GetLocalFilesList(sourceDirectoryType, false);
            MoveFiles(files, destinationDirectoryType);
        }

        public static void MoveFiles(List<string> files, DirectoryType destinationDirectoryType)
        {
            foreach (var file in files)
            {
                CopyFile(file, destinationDirectoryType,true);
            }
        }

        public static void CopyFiles(DirectoryType sourceDirectoryType, DirectoryType destinationDirectoryType)
        {
            var files = GetLocalFilesList(sourceDirectoryType, false);
            CopyFiles(files, destinationDirectoryType);
        }

        public static void CopyFiles(List<string> files, DirectoryType destinationDirectoryType)
        {
            foreach (var file in files)
            {
                CopyFile(file, destinationDirectoryType, false);
            }
        }

        private static void CopyFile(string file, DirectoryType destinationDirectoryType, bool withDelete)
        {
            CopyFile(file, destinationDirectoryType);
            if (withDelete) DeleteFileIfExists(file);
        }

        public static void CopyFile(FilesType filesType, DirectoryType sourceDirectoryType, DirectoryType destinationDirectoryType)
        {
            string sourceFile = GetLocalFilePatch(sourceDirectoryType, filesType);
            if (!File.Exists(sourceFile))
                return;
            
            var destinationFile = GetLocalFilePatch(destinationDirectoryType, filesType);
            CreatedPathIfNotExists(destinationFile);
            File.Copy(sourceFile, destinationFile);
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

        public static void DeletePath(DirectoryType directoryType)
        {
            var path= GetLocalDirectoryPatch(directoryType);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static void DeletePath(string path)
        {
            path = GetDirName(path);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static bool DirectoryExists(string dir)
        {
            return Directory.Exists(dir);
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

        public static void SaveElemsToFile(IEnumerable<VocabularyElem> elems, string path)
        {
            CreatedPathIfNotExists(path);
            DeleteFileIfExists(path);
            TextWriter tw = new StreamWriter(path, true);
            foreach (var elem in elems)
            {
                tw.WriteLine(elem.GetStringToSave());
            }

            tw.Close();
        }

        public static void WriteFileString(string file, string txt)
        {
            TextWriter tw = new StreamWriter(file);
            tw.WriteLine(txt);
            tw.Close();
        }

        public static string ReadHttpFileString(string file)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(file);
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();
            return content;
        }

        public static void DownloadFile(string fileSourceLocation, string fileDestonationLocation)
        {
            CreatedPathIfNotExists(fileDestonationLocation);
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(fileSourceLocation, fileDestonationLocation);
            }        
        }

        public static void UploadFile(string fileSourceLocation, string fileDestonationLocation)
        {
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                client.UploadFile(fileDestonationLocation, "STOR", fileSourceLocation);
            }
        }

        public static void UploadEmptyFile(DirectoryType directoryType, string fileName)
        {
            var tmpFile = GetLocalFilePatch(fileName);
            var fileDestonationLocation = GetServerFtpFilePatch(directoryType, fileName);
            File.Create(tmpFile).Dispose();
            UploadFile(tmpFile, fileDestonationLocation);
            DeleteFileIfExists(tmpFile);
        }
        
        public static void DeleteFtpFile(DirectoryType directoryType, string fileName)
        {
            var fileLocation = GetServerFtpFilePatch(directoryType, fileName);
            DeleteFtpFile(fileLocation);
        }

        public static void DeleteFtpFile(string fileLocation)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileLocation);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(ftpUser, ftpPassword);

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                response.Close();
            }
        }
        #region Patch
        #region Ftp
        public static string GetServerFtpDirectoryPatch(DirectoryType directoryType)
        {
            return GetServerFtpFilePatch(GetDirectoryName(directoryType));
        }
        public static string GetServerFtpDirectoryPatch(string directory)
        {
            return GetServerFtpFilePatch(directory);
        }

        public static string GetServerFtpFilePatch(DirectoryType directoryType, FilesType filesType)
        {
            return GetServerFtpFilePatch(GetDirectoryName(directoryType), filesType);
        }

        public static string GetServerFtpFilePatch(string directory, FilesType filesType)
        {
            return GetServerFtpFilePatch($"{directory}/{GetFileName(filesType)}");
        }

        public static string GetServerFtpFilePatch(DirectoryType directoryType, string fileName)
        {
            return GetServerFtpFilePatch($"{GetDirectoryName(directoryType)}/{fileName}");
        }

        private static string GetServerFtpFilePatch(string fileName)
        {
            var mainAdres = ftpServerAdres;
            if (TestMode) mainAdres += "/Test";
            return $"{mainAdres}/{fileName}";
        }
        #endregion
        #region Http
        public static string GetServerHttpFilePatch(DirectoryType directoryType, FilesType filesType)
        {            
            return GetServerHttpFilePatch($"{GetDirectoryName(directoryType)}/{GetFileName(filesType)}");
        }

        public static string GetServerHttpFilePatch(string fileName)
        {
            var mainAdres = httpServerAdres;
            if (TestMode) mainAdres += "/Test";
            return $"{mainAdres}/{fileName}";
        }
        #endregion
        #region Local
        public static string GetLocalDirectoryPatch(DirectoryType directoryType)
        {
            return GetLocalFilePatch(GetDirectoryName(directoryType));
        }

        public static string GetLocalFilePatch(DirectoryType directoryType, FilesType filesType)
        {
            return GetLocalFilePatch($"{GetDirectoryName(directoryType)}\\{GetFileName(filesType)}");
        }

        private static string GetLocalFilePatch(string fileName)
        {
            return $"{MainDir}\\{fileName}";
        }

        public static bool LocalDirectoryExists(DirectoryType directoryType)
        {
            return Directory.Exists(GetLocalDirectoryPatch(directoryType));
        }
        #endregion
        #endregion

        public static string GetDirectoryName(DirectoryType directoryType)
        {
            if (IsDirectoryInCahce(directoryType))
                return $"{DirectoryType.Cache.ToString()}\\{directoryType.ToString()}";

            if (directoryType == DirectoryType.Backup)
                return $"{DirectoryType.Backup.ToString()}\\{BackupDirectoryName}";
            
            var ret= directoryType.ToString();
            if (directoryType == DirectoryType.Www)
                ret = ret.ToLower();

            return ret;
        }

        private static bool IsDirectoryInCahce(DirectoryType directoryType)
        {
            if (directoryType == DirectoryType.Original) return true;
            if (directoryType == DirectoryType.OriginalOld) return true;
            if (directoryType == DirectoryType.CacheOld) return true;
            return false;
        }

        public static string GetFileName(FilesType filesType, bool withExtention = true)
        {
            var fileName = filesType.ToString();
            if (filesType == FilesType.Status) fileName = fileName.ToLower();
            if (withExtention)
                fileName += GetFileExtention(filesType);

            return fileName;
        }

        public static string GetFileExtention(FilesType filesType)
        {
            switch (filesType)
            {
                case FilesType.DataBase:
                case FilesType.Modules:
                case FilesType.Names:
                case FilesType.Navigation:
                case FilesType.NamesGenerator:
                    return ".xml";
                case FilesType.Vocabulary:
                    return ".cache";
                case FilesType.Status:
                    return ".txt";
            }

            return "";
        }

        public static bool CreateFTPDirectory(string directory)
        {
            try
            {
                //create the directory
                FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(new Uri(directory));
                requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                requestDir.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                requestDir.UsePassive = true;
                requestDir.UseBinary = true;
                requestDir.KeepAlive = false;
                FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();

                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    response.Close();
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }
            }
        }

        public static List<string> GetFtpFilesList(DirectoryType directoryType, bool withCreate)
        {
            var requestUriString = GetServerFtpDirectoryPatch(directoryType);
            return GetFtpFilesList(requestUriString, withCreate);
        }

        public static List<string> GetFtpFilesList(string requestUriString, bool withCreate)
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(requestUriString);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(ftpUser, ftpPassword);

            var ret = new List<string>();
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            var readedString = (reader.ReadToEnd());
            reader.Close();
            response.Close();

            readedString = readedString.Replace("\r\n", "\r");
            var list = readedString.Split('\r').ToList();
            foreach (var elem in list)
            {
                if (elem.Length <= 0) continue;
                ret.Add(elem.Split(' ').Last());
            }

            if (ret.Count == 0 && withCreate)
            {
                FtpWebRequest request2 = (FtpWebRequest)WebRequest.Create(requestUriString);
                request2.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                request2.Method = WebRequestMethods.Ftp.MakeDirectory;
                var resp = (FtpWebResponse)request2.GetResponse();
                resp.Close();
            }

            return ret;
        }

        public static void DownloadAllFiles(string serverFtpDirectory, string serverHttpDirectory, string localDirectory)
        {
    //        CreatedPathIfNotExists(localDirectory);
            var files = GetFtpFilesList(serverFtpDirectory, false);
            foreach (var file in files)
            {
                if (file == "." || file == "..") continue;
                var serverFile = serverHttpDirectory+"/"+(file);
                var localFile = localDirectory + "\\" + file;
                DownloadFile(serverFile, localFile);
            }
        }

        public static List<string> GetLocalFilesList(DirectoryType directoryType, bool withCreate)
        {
            var directory = GetLocalDirectoryPatch(directoryType);
            return GetLocalFilesList(directory, withCreate);
        }

        public static List<string> GetLocalFilesList(string directory, bool withCreate)
        {
            if (withCreate) CreatedPathIfNotExists(directory);
            var ret = new List<string>();
            if (!DirectoryExists(directory))
                return ret;

            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)            
                ret.Add(file);            

            if (ret.Count == 0) // sprawdze jeszce foldery
            {
                string[] directories = Directory.GetDirectories(directory);
                foreach (string dir in directories)
                    ret.Add(dir);
            }

            return ret;
        }
    }
}
