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
using Thea2Translator.DesktopApp.Windows;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Cache;
using Thea2Translator.Logic.Helpers;

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
            ProcessSynhronization(true);
        }

        private void btnUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(false);
        }
        
        private void ClearProgressBar()
        {
            this.Dispatcher.Invoke(() =>
            {
                barTextBlock.Text = "";
                barStatus.Value = 0;
            });
        }
        
        private void UpdateStatus(string s, double p)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    txtCurrentModuleInProcess.Content = s;
                    barTextBlock.Text = s;
                }
                barStatus.Value = p * 100;
            });
        }

        private void SetButtonEnableProp(bool isEnable)
        {
            btnDownloadFiles.IsEnabled = isEnable;
            btnUploadFiles.IsEnabled = isEnable;

            btnChooseDataBase.IsEnabled = isEnable;
            btnChooseModulus.IsEnabled = isEnable;
            btnChooseNames.IsEnabled = isEnable;
            btnVocabulary.IsEnabled = isEnable;
        }

        private void ProcessSynhronization(bool download)
        {
            Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        var synchronization = new Synchronization();

                        ClearProgressBar();
                        synchronization.StatusChanged += (s, p) => UpdateStatus(s, p);

                        this.Dispatcher.Invoke(() =>
                        {
                            SetButtonEnableProp(false);
                        });

                        ProcessResult result = null;
                        if (download)
                        {
                            result = synchronization.DownloadCache();
                            var workingNow = synchronization.WorkingNow();
                            if (!string.IsNullOrEmpty(workingNow)) result.AddMessage($"\r\nAktualnie pracujacy: {workingNow}");
                        }
                        else
                        {
                            result = synchronization.UploadCache();
                        }

                        synchronization.StatusChanged -= UpdateStatus;


                        this.Dispatcher.Invoke(() =>
                        {
                            SetButtonEnableProp(true);
                        });

                        MessageBox.Show(result.Message);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                        this.Dispatcher.Invoke(() =>
                        {
                            SetButtonEnableProp(true);
                        });
                    }
                }
            });
        }

        private void btnVocabulary_Click(object sender, RoutedEventArgs e)
        {
            FullDictinaryWindow dictinary = new FullDictinaryWindow();
            dictinary.Show();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
        }
    }
}

