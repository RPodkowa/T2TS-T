﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public static class UserHelper
    {
        public static string UserId;
        public static string UserName;

        public static int? UserRole;
        public static bool IsUserWithRole => UserRole.HasValue;
        public static bool IsTeamMemberUser => (UserRole.HasValue && FlagHelper.IsSettedBit(UserRole.Value, 0));
        public static bool IsTestUser => !IsTeamMemberUser;
        public static bool IsAdminUser => (UserRole.HasValue && FlagHelper.IsSettedBit(UserRole.Value, 1));

        public static Dictionary<FilesType, string> UserBookmarks;

        public static string GetUserRoleFileName()
        {
            if (string.IsNullOrEmpty(UserId)) return string.Empty;
            if (string.IsNullOrEmpty(UserName)) return string.Empty;

            return $"{UserId}_{UserName}";
        }

        public static void ReadUserRoleFromFtp()
        {
            UserRole = GetUserRoleFromFtp();
        }

        private static int? GetUserRoleFromFtp()
        {
            var userRoleFile = GetUserRoleFileName();
            if (string.IsNullOrEmpty(userRoleFile)) return null;
            if (!FileHelper.FtpFileExists(DirectoryType.Users, userRoleFile, WorkMode.Normal)) return null;

            string file = FileHelper.GetServerHttpFilePatch(DirectoryType.Users, userRoleFile, WorkMode.Normal);
            int role = 0;
            if (!int.TryParse(FileHelper.ReadHttpFileString(file), out role))
                return 0;

            return role;
        }

        public static void SendUserPetition()
        {
            var userRoleFile = GetUserRoleFileName();
            if (string.IsNullOrEmpty(userRoleFile)) return;
            FileHelper.UploadEmptyFile(DirectoryType.Users, userRoleFile, WorkMode.Normal);
        }

        public static void SetUserBookmarks(FilesType filesType, string bookmarks)
        {
            if (UserBookmarks == null) UserBookmarks = new Dictionary<FilesType, string>();
            UserBookmarks[filesType] = bookmarks;
        }

        public static List<string> GetUserBookmarks(FilesType filesType)
        {
            List<string> ret = null;
            if (UserBookmarks == null) return ret;
            if (!UserBookmarks.Keys.Contains(filesType)) return ret;

            var bookmarks = UserBookmarks[filesType];
            if (string.IsNullOrEmpty(bookmarks)) return ret;

            ret = bookmarks.Split(',').ToList();
            return ret;
        }
    }
}
