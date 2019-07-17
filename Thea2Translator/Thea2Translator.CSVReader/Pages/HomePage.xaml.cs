using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.DesktopApp.Pages.ModuleSelectionPages;
using Thea2Translator.DesktopApp.Properties;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Helpers;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        ResourceDictionary currentLangDictinary;

        public HomePage()
        {
            InitializeComponent();
            txtFolderDir.IsEnabled = false;

            if (string.IsNullOrEmpty(Settings.Default.WorkingDirectory)) Settings.Default.WorkingDirectory = UpdateHelper.GetApplicationBaseParentPath();
            txtFolderDir.Text = Settings.Default.WorkingDirectory;

            if (string.IsNullOrEmpty(Settings.Default.UserName)) Settings.Default.UserName = Environment.UserName;
            txtUserName.Text = Settings.Default.UserName;

            LogicProvider.Language.SetLanguage(Settings.Default.Language);

            this.SetLanguageDictinary();

            btnAdminPage.Visibility = UpdateHelper.ApplicationInAdminMode() ? Visibility.Visible : Visibility.Hidden;
            checkBox_Test.Visibility = UpdateHelper.ApplicationInAdminMode() ? Visibility.Visible : Visibility.Hidden;
        }

        private void BtnAdminPage_Click(object sender, RoutedEventArgs e)
        {
            if (btnAdminPage.Visibility == Visibility.Visible)
                btnNavigeteClicked(true);
        }

        private void BtnStartTranslate_Click(object sender, RoutedEventArgs e)
        {
            btnNavigeteClicked(false);
        }

        private void btnNavigeteClicked(bool admin)
        {
            FileHelper.TestMode = false;
            var testMode = checkBox_Test.IsChecked;
            if (testMode.HasValue && testMode.Value) testMode = UpdateHelper.ApplicationInAdminMode();
            if (testMode.HasValue && testMode.Value) FileHelper.TestMode = true;
            FileHelper.MainDir = txtFolderDir.Text;

            if (string.IsNullOrEmpty(Settings.Default.UserId))
                Settings.Default.UserId = Guid.NewGuid().ToString();

            Settings.Default.WorkingDirectory = txtFolderDir.Text;
            Settings.Default.UserName = txtUserName.Text;
            Settings.Default.Save();

            LogicProvider.UserId = Settings.Default.UserId;
            LogicProvider.UserName = Settings.Default.UserName;

            if (admin)
                this.NavigationService.Navigate(new ModuleSelectionAdminPage());
            else
                this.NavigationService.Navigate(new ModuleSelectionUserPage());
        }

        private void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (!string.IsNullOrEmpty(txtFolderDir.Text))
                fbd.SelectedPath = txtFolderDir.Text;

            fbd.Description = "Dir";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtFolderDir.Text = fbd.SelectedPath;
                btnStartTranslate.IsEnabled = true;
                Settings.Default.WorkingDirectory = txtFolderDir.Text;
            }
        }

        private void BtnChangeLangToPolish_Click(object sender, MouseButtonEventArgs e)
        {
            this.Resources.MergedDictionaries.Remove(currentLangDictinary);
            LogicProvider.Language.SetLanguage(Logic.Languages.Languages.Polish);

            currentLangDictinary = LanguageHelper.GetLanguageDictinary(LogicProvider.Language.CurrentLanguage);
            this.Resources.MergedDictionaries.Add(currentLangDictinary);

            Settings.Default.Language = Logic.Languages.Languages.Polish;
            Settings.Default.Save();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Resources.MergedDictionaries.Remove(currentLangDictinary);
            LogicProvider.Language.SetLanguage(Logic.Languages.Languages.English);

            currentLangDictinary = LanguageHelper.GetLanguageDictinary(LogicProvider.Language.CurrentLanguage);
            this.Resources.MergedDictionaries.Add(currentLangDictinary);

            Settings.Default.Language = Logic.Languages.Languages.English;
            Settings.Default.Save();
        }
    }
}
