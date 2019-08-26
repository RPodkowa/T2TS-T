using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Thea2Translator.Logic.Helpers;

namespace Thea2Translator.Logic.Cache
{
    public enum SynchronizationMode
    {
        Download,
        Refresh,
        Upload
    }

    public class Synchronization: ProcessHelper
    {
        public string WorkingNow()
        {
            var working = FileHelper.GetFtpFilesList(DirectoryType.Working, true).Where(x => x.Contains(".work"));
            return string.Join(", ", working.Select(s => s.Replace(".work", "")).ToList());
        }

        public static bool HasFiles(FilesType filesType)
        {
            return DataCache.FilesExists(filesType);
        }

        private bool HasConflictsInCacheFiles()
        {
            bool? result = null;
            result = HasConflictsInCacheFiles(FilesType.DataBase, !result.HasValue);
            if (result.HasValue && result.Value) return true;

            result = HasConflictsInCacheFiles(FilesType.Modules, !result.HasValue);
            if (result.HasValue && result.Value) return true;

            result = HasConflictsInCacheFiles(FilesType.Names, !result.HasValue);
            if (result.HasValue && result.Value) return true;

            return false;
        }

        private bool? HasConflictsInCacheFiles(FilesType filesType, bool checkVocabulary)
        {
            if (!DataCache.FilesExists(filesType))
                return null;

            if (DataCache.HasConflicts(filesType)) return true;
            if (checkVocabulary && DataCache.HasConflicts(FilesType.Vocabulary)) return true;

            return false;
        }


        #region Download
        public ProcessResult DownloadCache(SynchronizationMode forMode = SynchronizationMode.Download, FilesType? filesType = null)
        {
            if (HasConflictsInCacheFiles())
                return new ProcessResult(false, "Przed ściągnięciem plików z serwera należy rozwiązać konflikty!");

            bool forUpload = forMode == SynchronizationMode.Upload;
            bool forRefresh = forMode == SynchronizationMode.Refresh;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (forUpload || forRefresh) StartNextProcessStep();
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
            DownloadCacheFiles(filesType);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            string LogText = "Download";
            if (forUpload) LogText = "DownloadForUpload";
            if (forRefresh) LogText = "DownloadForRefresh";
            SendToLogs(filesType, LogText);
            MergeFiles();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            RemoveFilesAfterDownload();

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (!forUpload) AddWorkingInfo();
            if (!forUpload && !forRefresh) StopProcess();

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

        private void DownloadCacheFiles(FilesType? filesType)
        {
            bool forceDownloadDatabase = (filesType.HasValue && filesType.Value == FilesType.DataBase);
            bool forceDownloadModule = (filesType.HasValue && filesType.Value == FilesType.Modules);
            bool forceDownloadNames = (filesType.HasValue && filesType.Value == FilesType.Names);

            if (filesType.HasValue && filesType.Value == FilesType.All)
            {
                forceDownloadDatabase = true;
                forceDownloadModule = true;
                forceDownloadNames = true;
            }

            DownloadCacheFile(FilesType.DataBase, !forceDownloadDatabase);

            DownloadCacheFile(FilesType.Modules, !forceDownloadModule);
            DownloadCacheFile(FilesType.Navigation, !forceDownloadModule);

            DownloadCacheFile(FilesType.Names, !forceDownloadNames);
            DownloadCacheFile(FilesType.NamesGenerator, !forceDownloadNames);

            DownloadCacheFile(FilesType.Vocabulary, false);
        }

        private void DownloadCacheFile(FilesType filesType, bool withCheckOlds)
        {
            if (withCheckOlds)
            {
                var oldFileLocation = FileHelper.GetLocalFilePatch(DirectoryType.CacheOld, filesType);
                if (!FileHelper.FileExists(oldFileLocation))
                    return;
            }

            var fileSourceLocation = FileHelper.GetServerHttpFilePatch(DirectoryType.Cache, filesType);
            var fileDestonationLocation = FileHelper.GetLocalFilePatch(DirectoryType.Original, filesType);
            FileHelper.DownloadFile(fileSourceLocation, fileDestonationLocation);
        }
        #endregion
        #region Refresh
        public ProcessResult RefreshCache()
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartProcess("Refresh cache", 5 + 2);
            var downloadResult = DownloadCache(SynchronizationMode.Refresh);
            if (!downloadResult.Result)
                return downloadResult;

            if (HasConflictsInCacheFiles())
                return new ProcessResult(false, "Przed wysłaniem plików na serwer należy rozwiązać konflikty!");

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            UploadCacheFiles();
            SendToLogs(null, "Refresh");
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StopProcess();
            return new ProcessResult(true, "Odświeżenie plików zakończone sukcesem!");
        }
        #endregion
        #region Upload
        public ProcessResult UploadCache()
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartProcess("Upload cache", 5 + 3);
            var downloadResult = DownloadCache(SynchronizationMode.Upload);
            if (!downloadResult.Result)
                return downloadResult;

            if (HasConflictsInCacheFiles())
                return new ProcessResult(false, "Przed wysłaniem plików na serwer należy rozwiązać konflikty!");

            ////////////////////////////////////////////////////////////////////////////////////////////////////////
            StartNextProcessStep();
            UploadCacheFiles();
            SendToLogs(null, "Upload");
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
            string directory = $"{FileHelper.GetDirectoryName(DirectoryType.History)}/T_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{UserHelper.UserName}";
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
            if (FileHelper.FileExists(fileSourceLocation))
                FileHelper.UploadFile(fileSourceLocation, fileDestonationLocation);
        }
        #endregion

        private void SendToLogs(FilesType? filesType, string text)
        {
            string moduleName = "ALL";
            if (filesType.HasValue) moduleName = filesType.Value.ToString();
            string fileName = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{UserHelper.UserName}_{moduleName}_{text}.log";
            FileHelper.UploadEmptyFile(DirectoryType.Logs, fileName);
        }

        private void AddWorkingInfo()
        {
            string fileName = $"{UserHelper.UserName}.work";
            FileHelper.UploadEmptyFile(DirectoryType.Working, fileName);
        }

        private void DeleteWorkingInfo()
        {
            string fileName = $"{UserHelper.UserName}.work";
            FileHelper.DeleteFtpFile(DirectoryType.Working, fileName);
        }
    }
}
