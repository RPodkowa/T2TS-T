using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.DesktopApp.Windows;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Cache;
using Thea2Translator.Logic.Helpers;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for WorkMenuSelector.xaml
    /// </summary>
    public partial class ModuleSelectionAdminPage : Page
    {
        private object _lockObject = new object();

        private static SolidColorBrush selectedButtonColor = Brushes.LightGreen;
        private static SolidColorBrush unSelectedButtonColor = Brushes.Orange;

        bool isDataBaseModuleSelected = false;
        bool isModulesModuleSelected = false;
        bool isNamesModuleSelected = false;


        public ModuleSelectionAdminPage()
        {
            InitializeComponent();
            SetStepsButtonVisibility(false);
            ClearProgressBar();

            ChangeButtonSelectColor(btnChooseDataBase, false);
            ChangeButtonSelectColor(btnChooseModulus, false);
            ChangeButtonSelectColor(btnChooseNames, false);

            btnChooseDataBase.Click += (o, e) =>
            {
                isDataBaseModuleSelected = !isDataBaseModuleSelected;

                SetStepsButtonVisibility(true);
                ChangeButtonSelectColor(o as Button, isDataBaseModuleSelected);

                if (!isDataBaseModuleSelected && !isModulesModuleSelected && !isNamesModuleSelected)
                    SetStepsButtonVisibility(false);
            };

            btnChooseModulus.Click += (o, e) =>
            {
                isModulesModuleSelected = !isModulesModuleSelected;

                SetStepsButtonVisibility(true);
                ChangeButtonSelectColor(o as Button, isModulesModuleSelected);

                if (!isDataBaseModuleSelected && !isModulesModuleSelected && !isNamesModuleSelected)
                    SetStepsButtonVisibility(false);
            };

            btnChooseNames.Click += (o, e) =>
            {
                TextHelper.PrepareNamesString();
                isNamesModuleSelected = !isNamesModuleSelected;

                SetStepsButtonVisibility(true);
                ChangeButtonSelectColor(o as Button, isNamesModuleSelected);

                if (!isDataBaseModuleSelected && !isModulesModuleSelected && !isNamesModuleSelected)
                    SetStepsButtonVisibility(false);
            };

            this.SetLanguageDictinary();
        }

        private void ChangeButtonSelectColor(Button button, bool isModuleSelected)
        {
            button.Background = isModuleSelected ? selectedButtonColor : unSelectedButtonColor;
        }

        private void SetStepsButtonVisibility(bool isVisible)
        {
            btnImportFromSteam.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            btnPrepareToMachineTranslate.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            btnImportFromMachineTranslate.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            btnTranslate.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            btnExportToSteam.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            actionTitleLabel.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
            btnOpenMod.Visibility = isVisible ? Visibility.Visible : Visibility.Hidden;
        }

        private void ClearProgressBar()
        {
            this.Dispatcher.Invoke(() =>
            {
                barTextBlock.Text = "";
                barStatus.Value = 0;
            });
        }

        private void BtnTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (!FileHelper.LocalDirectoryExists(DirectoryType.Original))
            {
                MessageBox.Show("Przed rozpoczęciem tłumaczenia należy pobrać chache z serwera!");
                return;
            }

            int selectetCount = 0;
            if (isDataBaseModuleSelected) selectetCount++;
            if (isModulesModuleSelected) selectetCount++;
            if (isNamesModuleSelected) selectetCount++;

            if (selectetCount == 0)
                return;

            if (selectetCount > 1)
            {
                MessageBox.Show(this.Resources["transSelOneModuleSelection"].ToString(), this.Resources["transWarSelection"].ToString());
                return;
            }

            FilesType filesType = FilesType.DataBase;
            if (isDataBaseModuleSelected) filesType = FilesType.DataBase;
            if (isModulesModuleSelected) filesType = FilesType.Modules;
            if (isNamesModuleSelected) filesType = FilesType.Names;

            NavigationService.Navigate(new TranslatePage(filesType, true));
        }

        private void ProcessFiles(FilesType filesType, AlgorithmStep step)
        {
            Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        IDataCache cache = null;
                        switch (filesType)
                        {
                            case FilesType.DataBase: cache = LogicProvider.DataBase; break;
                            case FilesType.Modules: cache = LogicProvider.Modules; break;
                            case FilesType.Names: cache = LogicProvider.Names; break;
                        }

                        ClearProgressBar();
                        cache.StatusChanged += (s, p) => UpdateStatus(s, p);

                        this.Dispatcher.Invoke(() =>
                        {
                            SetButtonEnableProp(false);
                        });

                        cache.MakeStep(step);

                        cache.StatusChanged -= UpdateStatus;

                        this.Dispatcher.Invoke(() =>
                        {
                            SetButtonEnableProp(true);
                        });
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

        private void SetButtonEnableProp(bool isEnable)
        {
            btnDownloadFiles.IsEnabled = isEnable;
            btnUploadFiles.IsEnabled = isEnable;

            btnChooseDataBase.IsEnabled = isEnable;
            btnChooseModulus.IsEnabled = isEnable;
            btnChooseNames.IsEnabled = isEnable;
            btnVocabulary.IsEnabled = isEnable;

            btnImportFromSteam.IsEnabled = isEnable;
            btnImportFromMachineTranslate.IsEnabled = isEnable;
            btnPrepareToMachineTranslate.IsEnabled = isEnable;
            btnTranslate.IsEnabled = isEnable;
            btnExportToSteam.IsEnabled = isEnable;
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

        private void BtnImportFromSteam_Click(object sender, RoutedEventArgs e)
        {
            Process(AlgorithmStep.ImportFromSteam);
        }

        private void BtnPrepareToMachineTranslate_Click(object sender, RoutedEventArgs e)
        {
            Process(AlgorithmStep.PrepareToMachineTranslate);
        }

        private void BtnImportFromMachineTranslate_Click(object sender, RoutedEventArgs e)
        {
            Process(AlgorithmStep.ImportFromMachineTranslate);
        }

        private void BtnExportToSteam_Click(object sender, RoutedEventArgs e)
        {
            Process(AlgorithmStep.ExportToSteam);
        }

        private void Process(AlgorithmStep step)
        {
            if (isDataBaseModuleSelected) ProcessFiles(FilesType.DataBase, step);
            if (isModulesModuleSelected) ProcessFiles(FilesType.Modules, step);
            if (isNamesModuleSelected) ProcessFiles(FilesType.Names, step);
        }

        private void btnDownloadFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(true);
        }

        private void btnUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            ProcessSynhronization(false);
        }

        private void btnVocabulary_Click(object sender, RoutedEventArgs e)
        {
            FullDictinaryWindow dictinary = new FullDictinaryWindow(true);
            dictinary.Show();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage());
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
                            result = synchronization.DownloadCache(SynchronizationMode.Download, FilesType.All);
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

        private void BtnOpenMod_Click(object sender, RoutedEventArgs e)
        {
            ModWindow window = new ModWindow();
            window.Show();
        }
    }
}
