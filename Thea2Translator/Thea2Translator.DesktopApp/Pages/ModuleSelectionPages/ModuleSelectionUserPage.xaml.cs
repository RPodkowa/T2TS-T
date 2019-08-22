﻿using System;
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
                NavigationService.Navigate(new TranslatePage(Logic.FilesType.DataBase, false));
            };

            btnChooseModulus.Click += (o, e) =>
            {
                NavigationService.Navigate(new TranslatePage(Logic.FilesType.Modules, false));
            };

            btnChooseNames.Click += (o, e) =>
            {
                NavigationService.Navigate(new TranslatePage(Logic.FilesType.Names, false));
            };

            this.SetLanguageDictinary();
        }

        private void btnDownloadFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(SynchronizationMode.Download);
        }

        private void btnRefreshFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(SynchronizationMode.Refresh);
        }

        private void btnUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(SynchronizationMode.Upload);
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
            btnRefreshFiles.IsEnabled = isEnable;
            btnUploadFiles.IsEnabled = isEnable;

            btnChooseDataBase.IsEnabled = isEnable;
            btnChooseModulus.IsEnabled = isEnable;
            btnChooseNames.IsEnabled = isEnable;
            btnVocabulary.IsEnabled = isEnable;
        }

        private void ProcessSynhronization(SynchronizationMode synchronizationMode)
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
                                    result = synchronization.DownloadCache();
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
