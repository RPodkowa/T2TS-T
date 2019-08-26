using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
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
            btnChooseDataBase.Click += (o, e) => { btnChooseClick(FilesType.DataBase); };
            btnChooseModulus.Click += (o, e) => { btnChooseClick(FilesType.Modules); };
            btnChooseNames.Click += (o, e) => { btnChooseClick(FilesType.Names); };

            this.SetLanguageDictinary();
            SetButtonEnableProp(true);
        }

        private void btnChooseClick(FilesType filesType)
        {
            if (Synchronization.HasFiles(filesType))
                NavigationService.Navigate(new TranslatePage(filesType, false));
            else
                TryDownload(filesType);
        }

        private void btnDownloadFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(SynchronizationMode.Download, null);
        }

        private void btnRefreshFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(SynchronizationMode.Refresh, null);
        }

        private void btnUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(SynchronizationMode.Upload, null);
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
            btnChooseDataBase.IsEnabled = isEnable;
            btnChooseModulus.IsEnabled = isEnable;
            btnChooseNames.IsEnabled = isEnable;

            if (isEnable) isEnable = FileHelper.LocalDirectoryExists(DirectoryType.Cache);
            btnDownloadFiles.IsEnabled = isEnable;
            btnRefreshFiles.IsEnabled = isEnable;
            btnUploadFiles.IsEnabled = isEnable;
            btnVocabulary.IsEnabled = isEnable;
        }

        private void TryDownload(FilesType filesType)
        {
            if (Synchronization.HasFiles(filesType)) return;
            ProcessSynhronization(SynchronizationMode.Download, filesType);
        }

        private void ProcessSynhronization(SynchronizationMode synchronizationMode, FilesType? filesType)
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
                        switch (synchronizationMode)
                        {
                            case SynchronizationMode.Download:
                                {
                                    result = synchronization.DownloadCache(SynchronizationMode.Download, filesType);
                                    var workingNow = synchronization.WorkingNow();
                                    if (!string.IsNullOrEmpty(workingNow)) result.AddMessage($"\r\nAktualnie pracujący: {workingNow}");
                                }
                                break;
                            case SynchronizationMode.Refresh:
                                {
                                    result = synchronization.RefreshCache();
                                    var workingNow = synchronization.WorkingNow();
                                    if (!string.IsNullOrEmpty(workingNow)) result.AddMessage($"\r\nAktualnie pracujący: {workingNow}");
                                }
                                break;
                            case SynchronizationMode.Upload:
                                {
                                    result = synchronization.UploadCache();
                                }
                                break;
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
            FullDictinaryWindow dictinary = new FullDictinaryWindow(false);
            dictinary.Show();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
        }
    }
}

