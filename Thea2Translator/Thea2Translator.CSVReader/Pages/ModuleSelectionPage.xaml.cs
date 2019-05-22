﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Cache;
using Thea2Translator.Logic.Cache.Interfaces;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for WorkMenuSelector.xaml
    /// </summary>
    public partial class ModuleSelectionPage : Page
    {
        private static SolidColorBrush selectedButtonColor = Brushes.LightGreen;
        private static SolidColorBrush unSelectedButtonColor = Brushes.Orange;

        public int XYZ;
        bool isDataBaseModuleSelected = false;
        bool isModulesModuleSelected = false;

        public ModuleSelectionPage()
        {
            InitializeComponent();
            SetStepsButtonVisibility(false);
            ClearProgressBar();
            ChangeButtonSelectColor(btnChooseDataBase, false);
            ChangeButtonSelectColor(btnChooseModulus, false);

            btnChooseDataBase.Click += (o, e) =>
            {
                isDataBaseModuleSelected = !isDataBaseModuleSelected;

                SetStepsButtonVisibility(true);
                ChangeButtonSelectColor(o as Button, isDataBaseModuleSelected);

                if(!isDataBaseModuleSelected && !isModulesModuleSelected)
                    SetStepsButtonVisibility(false);
            };

            btnChooseModulus.Click += (o, e) =>
            {
                isModulesModuleSelected = !isModulesModuleSelected;

                SetStepsButtonVisibility(true);
                ChangeButtonSelectColor(o as Button, isModulesModuleSelected);

                if (!isDataBaseModuleSelected && !isModulesModuleSelected)
                    SetStepsButtonVisibility(false);
            };
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
            barTextBlock.Text = "";
            barStatus.Value = 0;
        }

        private void BtnTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (isDataBaseModuleSelected && isModulesModuleSelected)
            {
                MessageBox.Show("Choose only one module!", "Warning");
            }
            else if (isDataBaseModuleSelected)
            {
                this.NavigationService.Navigate(new TranslatePage(FilesType.DataBase));
            }
            else
            {
                this.NavigationService.Navigate(new TranslatePage(FilesType.Modules));
            }
        }

        private void ProcessFiles(FilesType filesType, AlgorithmStep step)
        {
            IDataCache cache = filesType == FilesType.DataBase ?
                LogicProvider.DataBase : LogicProvider.Modules;

            ClearProgressBar();
            cache.StatusChanged += (s, p) => UpdateStatus(s, p); ;
            cache.MakeStep(step);
            cache.StatusChanged -= UpdateStatus;
        }

        private void UpdateStatus(string s, double p)
        {
            barTextBlock.Text = s;
            barStatus.Value = p * 100;
        }

        private void BtnImportFromSteam_Click(object sender, RoutedEventArgs e)
        {
            if (isDataBaseModuleSelected) ProcessFiles(FilesType.DataBase, AlgorithmStep.ImportFromSteam);
            if (isModulesModuleSelected) ProcessFiles(FilesType.Modules, AlgorithmStep.ImportFromSteam);
        }

        private void BtnPrepareToMachineTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (isDataBaseModuleSelected) ProcessFiles(FilesType.DataBase, AlgorithmStep.PrepareToMachineTranslate);
            if (isModulesModuleSelected) ProcessFiles(FilesType.Modules, AlgorithmStep.PrepareToMachineTranslate);
        }

        private void BtnImportFromMachineTranslate_Click(object sender, RoutedEventArgs e)
        {
            if (isDataBaseModuleSelected) ProcessFiles(FilesType.DataBase, AlgorithmStep.ImportFromMachineTranslate);
            if (isModulesModuleSelected) ProcessFiles(FilesType.Modules, AlgorithmStep.ImportFromMachineTranslate);
        }

        private void BtnExportToSteam_Click(object sender, RoutedEventArgs e)
        {
            if (isDataBaseModuleSelected) ProcessFiles(FilesType.DataBase, AlgorithmStep.ExportToSteam);
            if (isModulesModuleSelected) ProcessFiles(FilesType.Modules, AlgorithmStep.ExportToSteam);
        }
    }
}
