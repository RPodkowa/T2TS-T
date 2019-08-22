using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Thea2Translator.Logic.Helpers;

namespace Thea2Translator.Logic.Cache
{
    public class Synchronization: ProcessHelper
    {
        public string WorkingNow()
        {
            var working = FileHelper.GetFtpFilesList(DirectoryType.Working, true).Where(x => x.Contains(".work"));
            return string.Join(", ", working.Select(s => s.Replace(".work", "")).ToList());
        }

        private bool HasConflictsInCacheFiles()
        {
            if (DataCache.HasConflicts(FilesType.DataBase)) return true;
            if (DataCache.HasConflicts(FilesType.Modules)) return true;
            if (DataCache.HasConflicts(FilesType.Names)) return true;
            if (DataCache.HasConflicts(FilesType.NamesGenerator)) return true;
            if (DataCache.HasConflicts(FilesType.Vocabulary)) return true;
            return false;
        }

        #region Download
        public ProcessResult DownloadCache(bool forUpload = false)
        {
            if (HasConflictsInCacheFiles())            
                return new ProcessResult(false, "Przed ściągnięciem plików z serwera należy rozwiązać konflikty!");            
                        
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (forUpload) StartNextProcessStep();
            else StartProcess("Download cache", 5);
            if (FileHelper.LocalDirectoryExists(DirectoryType.Original))
                FileHelper.MoveFiles(DirectoryType.Original, DirectoryType.OriginalOld);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            if (FileHelper.LocalDirectoryExists(DirectoryType.Cache))
            {
                FileHelper.PrepareBackupDirectory(true);
                FileHelper.CopyFiles(DirectoryType.Cache, DirectoryType.Backup);
                FileHelper.MoveFiles(DirectoryType.Cache, DirectoryType.CacheOld);
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            DownloadCacheFiles();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            SendToLogs(forUpload ? "DownloadForUpload" : "Download");
            MergeFiles();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            RemoveFilesAfterDownload();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (!forUpload)
            {
                AddWorkingInfo();
                StopProcess();
            }

            return new ProcessResult(true, "Pobieranie plików zakończone sukcesem!");
        }

        private void MergeFiles()
        {
            MergeFile(FilesType.DataBase);
            MergeFile(FilesType.Modules);
            MergeFile(FilesType.Names);
            MergeFile(FilesType.NamesGenerator);
            MergeFile(FilesType.Vocabulary);
            FileHelper.CopyFile(FilesType.Navigation, DirectoryType.Original, DirectoryType.Cache);
        }

        private void MergeFile(FilesType filesType)
        {
            DataCache.MergeCache(filesType);
        }

        private void RemoveFilesAfterDownload()
        {
            FileHelper.DeletePath(DirectoryType.OriginalOld);
            FileHelper.DeletePath(DirectoryType.CacheOld);
        }

        private void DownloadCacheFiles()
        {
            DownloadCacheFile(FilesType.DataBase);
            DownloadCacheFile(FilesType.Modules);
            DownloadCacheFile(FilesType.Names);
            DownloadCacheFile(FilesType.NamesGenerator);
            DownloadCacheFile(FilesType.Vocabulary);
            DownloadCacheFile(FilesType.Navigation);
        }

        private void DownloadCacheFile(FilesType filesType)
        {
            var fileSourceLocation = FileHelper.GetServerHttpFilePatch(DirectoryType.Cache, filesType);
            var fileDestonationLocation = FileHelper.GetLocalFilePatch(DirectoryType.Original, filesType);
            FileHelper.DownloadFile(fileSourceLocation, fileDestonationLocation);
        }
        #endregion
        #region Upload
        public ProcessResult UploadCache()
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartProcess("Upload cache", 5 + 3);
            var downloadResult = DownloadCache(true);
            if (!downloadResult.Result)
                return downloadResult;

            if (HasConflictsInCacheFiles())
                return new ProcessResult(false, "Przed wysłaniem plików na serwer należy rozwiązać konflikty!");

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            UploadCacheFiles();
            SendToLogs("Upload");
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            RemoveFilesAfterUpload();
            DeleteWorkingInfo();
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StopProcess();
            return new ProcessResult(true, "Wysyłanie plików zakończone sukcesem!");
        }

        private void RemoveFilesAfterUpload()
        {
            FileHelper.PrepareBackupDirectory(false);
            FileHelper.MoveFiles(DirectoryType.Cache, DirectoryType.Backup);
            FileHelper.DeletePath(DirectoryType.Original);
            FileHelper.DeletePath(DirectoryType.Cache);
        }

        private void UploadCacheFiles()
        {
            UploadCacheFile(FilesType.DataBase);
            UploadCacheFile(FilesType.Modules);
            UploadCacheFile(FilesType.Names);
            UploadCacheFile(FilesType.NamesGenerator);
            UploadCacheFile(FilesType.Vocabulary);
            CreateStatusFile();
            UploadCacheFile(FilesType.Status);
            UploadToHistory();
            UploadToWww();
        }

        private void CreateStatusFile()
        {
            var status = LogicProvider.GetWwwStatus();
            var json = JsonHelper.ToJson(status);
            var fullPath = FileHelper.GetLocalFilePatch(DirectoryType.Cache, FilesType.Status);
            FileHelper.WriteFileString(fullPath, json);
        }
        private void UploadToWww()
        {
            UploadCacheFileOtherDirectory(FilesType.Status, FileHelper.GetDirectoryName(DirectoryType.Www));
        }

        private void UploadToHistory()
        {
            string directory = $"{FileHelper.GetDirectoryName(DirectoryType.History)}/T_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{LogicProvider.UserName}";
            FileHelper.CreateFTPDirectory(FileHelper.GetServerFtpDirectoryPatch(directory));
            UploadCacheFileOtherDirectory(FilesType.DataBase, directory);
            UploadCacheFileOtherDirectory(FilesType.Modules, directory);
            UploadCacheFileOtherDirectory(FilesType.Names, directory);
            UploadCacheFileOtherDirectory(FilesType.NamesGenerator, directory);
            UploadCacheFileOtherDirectory(FilesType.Vocabulary, directory);
            UploadCacheFileOtherDirectory(FilesType.Status, directory);
        }

        private void UploadCacheFileOtherDirectory(FilesType filesType, string directory)
        {
            var fileDestonationLocation = FileHelper.GetServerFtpFilePatch(directory, filesType);
            UploadCacheFile(filesType, fileDestonationLocation);
        }

        private void UploadCacheFile(FilesType filesType)
        {
            var fileDestonationLocation = FileHelper.GetServerFtpFilePatch(DirectoryType.Cache, filesType);
            UploadCacheFile(filesType, fileDestonationLocation);
        }

        private void UploadCacheFile(FilesType filesType, string fileDestonationLocation)
        {
            var fileSourceLocation = FileHelper.GetLocalFilePatch(DirectoryType.Cache, filesType);
            FileHelper.UploadFile(fileSourceLocation, fileDestonationLocation);
        }
        #endregion

        private void SendToLogs(string text)
        {
            string fileName = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{LogicProvider.UserName}_{text}.log";            
            FileHelper.UploadEmptyFile(DirectoryType.Logs, fileName);
        }

        private void AddWorkingInfo()
        {
            string fileName = $"{LogicProvider.UserName}.work";
            FileHelper.UploadEmptyFile(DirectoryType.Working, fileName);
        }

        private void DeleteWorkingInfo()
        {
            string fileName = $"{LogicProvider.UserName}.work";
            FileHelper.DeleteFtpFile(DirectoryType.Working, fileName);
        }
    }
}
