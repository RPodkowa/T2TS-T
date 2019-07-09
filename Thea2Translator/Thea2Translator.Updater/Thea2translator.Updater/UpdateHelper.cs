using System;
using System.Collections.Generic;
using System.IO;
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
        public static bool CheckNeedForUpdate(ApplicationType applicationType)
        {
            var serverVersion = GetServerVersionInfo(applicationType);
            var localVersion = GetLocalVersionInfo(applicationType);

            return serverVersion!= localVersion;
        }

        public static void CreateIfNotExistsVersionFile(string version)
        {
            var file = GetApplicationLocalPatch(ApplicationType.Updater) + "\\version.info";
            if (File.Exists(file))
                return;

            using (StreamWriter writer = new StreamWriter(file))
            {
                writer.Write(version);
            }
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
        
        public static string GetApplicationLocalPatch(ApplicationType applicationType)
        {
            string localDir = AppDomain.CurrentDomain.BaseDirectory;
            if (applicationType == ApplicationType.Translator) localDir += "\\Translator";
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
