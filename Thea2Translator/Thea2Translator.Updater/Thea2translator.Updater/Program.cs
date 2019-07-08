using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Helpers;

namespace Thea2translator.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            UpdateIt();
            Console.ReadLine();
        }

        static void UpdateIt()
        {
            //1. Delete old files
            DeleteFiles();
            //2. Download new files
            DownloadFiles();
            //3. Run Translator            
            var app = UpdateHelper.GetApplicationLocalPatch(ApplicationType.Translator) + "//Thea2Translator.exe";
            Console.WriteLine($"Uruchamiam {app}");
            System.Diagnostics.Process.Start(app);
            //4. Close Updater
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        static void DeleteFiles()
        {
            try
            {
                var dir = UpdateHelper.GetApplicationLocalPatch(ApplicationType.Translator);
                if (Directory.Exists(dir))
                {
                    Console.WriteLine($"Usuwam stare pliki [{dir}]");
                    Directory.Delete(dir, true);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void DownloadFiles()
        {
            ApplicationType applicationType = ApplicationType.Translator;
            string serverFtpDirectory = UpdateHelper.GetApplicationServerFtpPatch(applicationType);
            string serverHttpDirectory = UpdateHelper.GetApplicationServerHttpPatch(applicationType);
            string localDirectory = UpdateHelper.GetApplicationLocalPatch(applicationType);
            if (!Directory.Exists(localDirectory)) Directory.CreateDirectory(localDirectory);
            FileHelper.DownloadAllFiles(serverFtpDirectory, serverHttpDirectory, localDirectory);
        }
    }
}
