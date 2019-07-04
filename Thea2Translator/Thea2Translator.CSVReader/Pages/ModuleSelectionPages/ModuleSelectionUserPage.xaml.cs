using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.Logic.Cache;

namespace Thea2Translator.DesktopApp.Pages.ModuleSelectionPages
{
    /// <summary>
    /// Interaction logic for ModuleSelectionUserPage.xaml
    /// </summary>
    public partial class ModuleSelectionUserPage : Page
    {
        private object _lockObject = new object();

        public ModuleSelectionUserPage()
        {
            InitializeComponent();          

            btnChooseDataBase.Click += (o, e) =>
            {
                NavigationService.Navigate(new TranslatePage(Logic.FilesType.DataBase));
            };

            btnChooseModulus.Click += (o, e) =>
            {
                NavigationService.Navigate(new TranslatePage(Logic.FilesType.Modules));
            };

            btnChooseNames.Click += (o, e) =>
            {
                NavigationService.Navigate(new TranslatePage(Logic.FilesType.Names));
            };

            this.SetLanguageDictinary();
        }

        private void btnDownloadFiles_Click(object sender, RoutedEventArgs e)
        {
            var synchronization = new Synchronization();
            var workingNow = synchronization.WorkingNow();
            if (synchronization.DownloadCache())
            {
                string txt = "Pobieranie plików zakończone sukcesem!";
                if (!string.IsNullOrEmpty(workingNow)) txt += $"\r\nAktualnie pracujacy: {workingNow}";
                MessageBox.Show(txt);
            }
        }

        private void btnUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            var synchronization = new Synchronization();
            if (synchronization.UploadCache())
                MessageBox.Show("Wysyłanie plików zakończone sukcesem!");
        }

        private void btnVocabulary_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService.Navigate(new VocabularyPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
        }
    }
}

