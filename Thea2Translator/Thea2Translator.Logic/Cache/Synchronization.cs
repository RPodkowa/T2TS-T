using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thea2Translator.Logic.Cache
{
    public class Synchronization
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
            if (DataCache.HasConflicts(FilesType.Vocabulary)) return true;
            return false;
        }

        #region Download
        public bool DownloadCache(bool forUpload = false)
        {
            if (HasConflictsInCacheFiles())
            {
                MessageBox.Show("Przed ściągnięciem plików z serwera należy rozwiązać konflikty!");
                return false;
            }

            if (FileHelper.LocalDirectoryExists(DirectoryType.Original))
                FileHelper.MoveFiles(DirectoryType.Original, DirectoryType.OriginalOld);

            if (FileHelper.LocalDirectoryExists(DirectoryType.Cache))
                FileHelper.MoveFiles(DirectoryType.Cache, DirectoryType.CacheOld);

            DownloadCacheFiles();
            SendToLogs(forUpload ? "DownloadForUpload" : "Download");
            MergeFiles();
            RemoveFilesAfterDownload();
            if (!forUpload) AddWorkingInfo();
            return true;
        }

        private void MergeFiles()
        {
            MergeFile(FilesType.DataBase);
            MergeFile(FilesType.Modules);
            MergeFile(FilesType.Names);
            MergeFile(FilesType.Vocabulary);
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
            DownloadCacheFile(FilesType.Vocabulary);
        }

        private void DownloadCacheFile(FilesType filesType)
        {
            var fileSourceLocation = FileHelper.GetServerHttpFilePatch(DirectoryType.Cache, filesType);
            var fileDestonationLocation = FileHelper.GetLocalFilePatch(DirectoryType.Original, filesType);
            FileHelper.DownloadFile(fileSourceLocation, fileDestonationLocation);
        }
        #endregion
        #region Upload
        public bool UploadCache()
        {
            if (!DownloadCache(true))
                return false;

            if (HasConflictsInCacheFiles())
            {
                MessageBox.Show("Przed wysłaniem plików na serwer należy rozwiązać konflikty!");
                return false;
            }

            UploadCacheFiles();
            SendToLogs("Upload");
            RemoveFilesAfterUpload();
            DeleteWorkingInfo();
            return true;
        }

        private void RemoveFilesAfterUpload()
        {
            FileHelper.DeletePath(DirectoryType.Backup);
            FileHelper.MoveFiles(DirectoryType.Cache, DirectoryType.Backup);
            FileHelper.DeletePath(DirectoryType.Original);
            FileHelper.DeletePath(DirectoryType.Cache);
        }

        private void UploadCacheFiles()
        {
            UploadCacheFile(FilesType.DataBase);
            UploadCacheFile(FilesType.Modules);
            UploadCacheFile(FilesType.Names);
            UploadCacheFile(FilesType.Vocabulary);
            UploadToHistory();
        }

        private void UploadToHistory()
        {
            string directory = $"{FileHelper.GetDirectoryName(DirectoryType.History)}/{LogicProvider.UserName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";            
            FileHelper.CreateFTPDirectory(FileHelper.GetServerFtpDirectoryPatch(directory));
            UploadCacheFileOtherDirectory(FilesType.DataBase, directory);
            UploadCacheFileOtherDirectory(FilesType.Modules, directory);
            UploadCacheFileOtherDirectory(FilesType.Names, directory);
            UploadCacheFileOtherDirectory(FilesType.Vocabulary, directory);
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
