﻿using System.Windows;
using System.Windows.Controls;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.Logic;

namespace Thea2Translator.DesktopApp.Windows
{
    /// <summary>
    /// Interaction logic for ModWindow.xaml
    /// </summary>
    public partial class ModWindow : Window
    {
        private ModType actualType;
        public ModWindow()
        {
            InitializeComponent();
            cbModType.SelectedIndex = 0;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            SettingsHelper.SetModTexts(actualType, txtTitle.Text, txtBody.Text);

            var manager = new ModManager(actualType, txtTitle.Text, txtBody.Text, txtState.Text);
            manager.PrepareMod();
        }

        private void cbModType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetModText();
        }

        private void SetModText()
        {
            actualType = ModType.Translation;
            if (cbModType.SelectedIndex == 1) actualType = ModType.TranslationDebug;
            if (cbModType.SelectedIndex == 2) actualType = ModType.Names;
            SetModText(actualType);
        }

        private void SetModText(ModType type)
        {
            txtTitle.Text = SettingsHelper.GetModTitle(type);
            txtBody.Text = SettingsHelper.GetModBody(type);
            txtState.Text = "";
            if (type == ModType.Translation || type==ModType.TranslationDebug)
                txtState.Text = $"{LogicProvider.DataBase.GetSummary(true)}\r\n\r\n{LogicProvider.Modules.GetSummary(true)}";

            if (type == ModType.Names)
                txtState.Text = $"{LogicProvider.Names.GetSummary(true)}";
        }
    }
}
