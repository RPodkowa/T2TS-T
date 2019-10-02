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
            if (filesType == FilesType.Names) return true;
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
            if (filesType == FilesType.Names) return Settings.Default.BookmarksNames;
            return null;
        }

        private static void SetBookmarks(FilesType filesType, string bookmarks)
        {
            if (filesType == FilesType.DataBase) Settings.Default.BookmarksDatabase = bookmarks;
            if (filesType == FilesType.Modules) Settings.Default.BookmarksModules = bookmarks;
            if (filesType == FilesType.Names) Settings.Default.BookmarksNames = bookmarks;
            UserHelper.SetUserBookmarks(filesType, bookmarks);
            Settings.Default.Save();
        }

        public static void ReadUserBookmarks(FilesType filesType)
        {
            if (!CanUserBookmarks(filesType))
                return;
            
            UserHelper.SetUserBookmarks(filesType, GetBookmarks(filesType));
        }

        public static string GetModTitle(ModType modType)
        {
            if (modType == ModType.Translation) return Settings.Default.ModTitle;
            if (modType == ModType.TranslationDebug) return Settings.Default.ModTitle_Translation_Debug;
            if (modType == ModType.Names) return Settings.Default.ModTitle_Names;
            return "";
        }

        public static string GetModBody(ModType modType)
        {
            if (modType == ModType.Translation) return Settings.Default.ModBody;
            if (modType == ModType.TranslationDebug) return Settings.Default.ModBody_Translation_Debug;
            if (modType == ModType.Names) return Settings.Default.ModBody_Names;
            return "";
        }

        public static void SetModTexts(ModType modType, string title, string body)
        {
            if (modType == ModType.Translation)
            {
                Settings.Default.ModTitle = title;
                Settings.Default.ModBody = body;
                Settings.Default.Save();
            }
            if (modType == ModType.TranslationDebug)
            {
                Settings.Default.ModTitle_Translation_Debug = title;
                Settings.Default.ModBody_Translation_Debug = body;
                Settings.Default.Save();
            }
            if (modType == ModType.Names)
            {
                Settings.Default.ModTitle_Names = title;
                Settings.Default.ModBody_Names = body;
                Settings.Default.Save();
            }
        }
    }
}
