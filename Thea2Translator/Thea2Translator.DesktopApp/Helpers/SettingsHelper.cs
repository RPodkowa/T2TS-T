using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Thea2Translator.DesktopApp.Properties;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Languages;

namespace Thea2Translator.DesktopApp.Helpers
{
    static class SettingsHelper
    {
        private static bool CanUserBookmarks(FilesType filesType)
        {
            if (filesType == FilesType.DataBase) return true;
            if (filesType == FilesType.Modules) return true;
            return false;
        }
        public static void ToggleUserBookmark(FilesType filesType, string bookmark)
        {
            if (!CanUserBookmarks(filesType))
                return;

            string bookmarks = GetBookmarks(filesType);
            if (bookmarks == null) bookmarks = "";
            List<string> bookmarksList = bookmarks.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (bookmarksList == null) bookmarksList = new List<string>();
            if (bookmarksList.Contains(bookmark)) bookmarksList.RemoveAll(x => x == bookmark);
            else bookmarksList.Add(bookmark);

            bookmarks = string.Join(",", bookmarksList.ToArray());
            SetBookmarks(filesType, bookmarks);
        }

        private static string GetBookmarks(FilesType filesType)
        {
            if (filesType == FilesType.DataBase) return Settings.Default.BookmarksDatabase;
            if (filesType == FilesType.Modules) return Settings.Default.BookmarksModules;
            return null;
        }

        private static void SetBookmarks(FilesType filesType, string bookmarks)
        {
            if (filesType == FilesType.DataBase) Settings.Default.BookmarksDatabase = bookmarks;
            if (filesType == FilesType.Modules) Settings.Default.BookmarksModules = bookmarks;
            UserHelper.SetUserBookmarks(filesType, bookmarks);
            Settings.Default.Save();
        }

        public static void ReadUserBookmarks(FilesType filesType)
        {
            if (!CanUserBookmarks(filesType))
                return;
            
            UserHelper.SetUserBookmarks(filesType, GetBookmarks(filesType));
        }
    }
}
