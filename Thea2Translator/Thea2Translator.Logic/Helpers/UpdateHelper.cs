using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Helpers
{
    public enum ApplicationType
    {
        Translator,
        Updater
    }
    public class UpdateHelper
    {

        public static void TryUpdate(ApplicationType applicationType)
        {
            if (!CheckNeedForUpdate(applicationType))
                return;

            UpdateIt(applicationType);
        }

        private static void UpdateIt(ApplicationType applicationType)
        {
            if (applicationType == ApplicationType.Updater) UpdateUpdater();
            if (applicationType == ApplicationType.Translator) UpdateTranslator();
        }

        private static void UpdateUpdater()
        {
            FileHelper.DownloadAllFiles(GetApplicationServerFtpPatch(ApplicationType.Updater),GetApplicationServerHttpPatch(ApplicationType.Updater), GetApplicationLocalPatch(ApplicationType.Updater));
        }

        private static void UpdateTranslator()
        {
            var app = GetApplicationLocalPatch(ApplicationType.Updater) + "//Thea2translator.Updater.exe";
            System.Diagnostics.Process.Start(app);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private static bool CheckNeedForUpdate(ApplicationType applicationType)
        {
            var serverVersion = GetServerVersionInfo(applicationType);
            var localVersion = GetLocalVersionInfo(applicationType);

            return serverVersion!= localVersion;
        }

        private static string GetLocalVersionInfo(ApplicationType applicationType)
        {
            var file = GetApplicationLocalPatch(applicationType) + "\\version.info";
            var version = FileHelper.ReadFileString(file);
            return version;
        }

        private static string GetServerVersionInfo(ApplicationType applicationType)
        {
            var file = GetApplicationServerHttpPatch(applicationType) + "/version.info";
            var version = FileHelper.ReadHttpFileString(file);
            return version;
        }

        public static string GetApplicationBaseParentPath()
        {
            var a = AppDomain.CurrentDomain.BaseDirectory;
            var b = System.IO.Directory.GetParent(System.IO.Directory.GetParent(a).FullName);
            return b.FullName;
        }

        public static string GetApplicationLocalPatch(ApplicationType applicationType)
        {
            string localDir = GetApplicationBaseParentPath();
            if (applicationType == ApplicationType.Translator) localDir += "\\Translator";
            if (applicationType == ApplicationType.Updater) localDir += "\\Updater";
            if (!System.IO.Directory.Exists(localDir)) System.IO.Directory.CreateDirectory(localDir);
            return localDir;
        }

        public static string GetApplicationServerHttpPatch(ApplicationType applicationType)
        {
            string serverDir = FileHelper.GetServerHttpFilePatch("Release");
            if (applicationType == ApplicationType.Translator) serverDir += "/Translator";
            if (applicationType == ApplicationType.Updater) serverDir += "/Updater";
            return serverDir;
        }

        public static string GetApplicationServerFtpPatch(ApplicationType applicationType)
        {
            string serverDir = FileHelper.GetServerFtpDirectoryPatch("Release");
            if (applicationType == ApplicationType.Translator) serverDir += "/Translator";
            if (applicationType == ApplicationType.Updater) serverDir += "/Updater";
            return serverDir;
        }
    }
}
