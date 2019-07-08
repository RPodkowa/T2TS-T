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
                
        public static bool DirectoryExists(string dir)
        {
            return Directory.Exists(dir);
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
            Console.WriteLine($"Pobieranie {Path.GetFileName(fileDestonationLocation)} ...");
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

        public static string GetServerFtpDirectoryPatch(string directory)
        {
            return GetServerFtpFilePatch(directory);
        }

        private static string GetServerFtpFilePatch(string fileName)
        {
            return $"{ftpServerAdres}/{fileName}";
        }
        #endregion
        #region Http
        
        public static string GetServerHttpFilePatch(string fileName)
        {
            return $"{httpServerAdres}/{fileName}";
        }
        #endregion
        #region Local
        
        #endregion
        #endregion
                
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
