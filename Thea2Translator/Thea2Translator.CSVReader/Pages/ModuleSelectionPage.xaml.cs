using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Cache;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for WorkMenuSelector.xaml
    /// </summary>
    public partial class ModuleSelectionPage : Page
    {
        private object _lockObject = new object();
        private TimeSpan? processTimeSpan;

        private static SolidColorBrush selectedButtonColor = Brushes.LightGreen;
        private static SolidColorBrush unSelectedButtonColor = Brushes.Orange;
                
        bool isDataBaseModuleSelected = false;
        bool isModulesModuleSelected = false;
        bool isNamesModuleSelected = false;


        public ModuleSelectionPage()
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
        }

        private void ClearProgressBar()
        {
            this.Dispatcher.Invoke(() => {
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
            
            NavigationService.Navigate(new TranslatePage(filesType));
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
                        switch(filesType)
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
                            txtCurrentModuleInProcess.Content = $"{step.ToString()} - {filesType.ToString()}";
                        });

                        var stopWatch = new Stopwatch();
                        stopWatch.Start();
                        cache.MakeStep(step);
                        stopWatch.Stop();

                        if (processTimeSpan == null) processTimeSpan = stopWatch.Elapsed;
                        else processTimeSpan += stopWatch.Elapsed;

                        cache.StatusChanged -= UpdateStatus;

                        this.Dispatcher.Invoke(() =>
                        {
                            SetButtonEnableProp(true);
                            string elapsedTime = string.Format(" [{0:00}:{1:00}:{2:00}.{3:00}]", processTimeSpan?.Hours, processTimeSpan?.Minutes, processTimeSpan?.Seconds, processTimeSpan?.Milliseconds / 10);
                            txtCurrentModuleInProcess.Content = $"{step.ToString()} done in {elapsedTime}";
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
            btnChooseDataBase.IsEnabled = isEnable;
            btnChooseModulus.IsEnabled = isEnable;
            btnChooseNames.IsEnabled = isEnable;

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
                barTextBlock.Text = s;
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
            processTimeSpan = null;
            if (isDataBaseModuleSelected) ProcessFiles(FilesType.DataBase, step);
            if (isModulesModuleSelected) ProcessFiles(FilesType.Modules, step);
            if (isNamesModuleSelected) ProcessFiles(FilesType.Names, step);
        }

        private void btnDownloadFiles_Click(object sender, RoutedEventArgs e)
        {
            var synchronization = new Synchronization();
            if (synchronization.DownloadCache())
                MessageBox.Show("Pobieranie plików zakończone sukcesem!");
        }

        private void btnUploadFiles_Click(object sender, RoutedEventArgs e)
        {
            var synchronization = new Synchronization();
            if (synchronization.UploadCache())
                MessageBox.Show("Wysyłanie plików zakończone sukcesem!");
        }

        private void btnVocabulary_Click(object sender, RoutedEventArgs e)
        {
           // NavigationService.Navigate(new VocabularyPage());
        }
    }
}
