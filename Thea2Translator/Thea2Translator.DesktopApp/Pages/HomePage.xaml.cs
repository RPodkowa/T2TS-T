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

            UserHelper.UserId = Settings.Default.UserId;
            UserHelper.UserName = Settings.Default.UserName;
            UserHelper.ReadUserRoleFromFtp();

            RefreshControls();
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
            SaveSettings();

            if (UserHelper.IsTestUser) FileHelper.MainWorkMode = WorkMode.Test;
            if (!UserHelper.IsAdminUser && admin) return;
            if (UserHelper.IsAdminUser)
            {
                var devMode = checkBox_Test.IsChecked;
                if (devMode.HasValue && devMode.Value) FileHelper.MainWorkMode = WorkMode.Developer;
            }
                        
            if (FileHelper.MainWorkMode != WorkMode.Normal)            
                System.Windows.MessageBox.Show($"Praca w trybie {FileHelper.MainWorkMode.ToString()}!", "Uwaga");            

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

        private void btnRole_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            UserHelper.ReadUserRoleFromFtp();

            string message = "Dane autoryzacyjne zostały pobrane";

            if (UserHelper.UserRole == null)
            {
                UserHelper.SendUserPetition();
                message = "Prośba o autoryzację została wysłana";
            }

            System.Windows.MessageBox.Show(message, "Sukces");
            RefreshControls();
        }

        private void SaveSettings()
        {
            if (string.IsNullOrEmpty(Settings.Default.UserId))
                Settings.Default.UserId = Guid.NewGuid().ToString();

            Settings.Default.WorkingDirectory = txtFolderDir.Text;
            Settings.Default.UserName = txtUserName.Text;
            Settings.Default.Save();

            FileHelper.MainDir = txtFolderDir.Text;
            UserHelper.UserId = Settings.Default.UserId;
            UserHelper.UserName = Settings.Default.UserName;
        }

        private void RefreshControls()
        {
            btnAdminPage.Visibility = UserHelper.IsAdminUser ? Visibility.Visible : Visibility.Hidden;
            checkBox_Test.Visibility = UserHelper.IsAdminUser ? Visibility.Visible : Visibility.Hidden;

            if (UserHelper.IsTestUser) btnStartTranslate.Content = "Test";
            else btnStartTranslate.Content = "Rozpocznij";
        }
    }
}
