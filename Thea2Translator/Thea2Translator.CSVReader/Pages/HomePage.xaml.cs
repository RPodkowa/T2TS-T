using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.DesktopApp.Properties;
using Thea2Translator.Logic;

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
            txtFolderDir.Text = Settings.Default.FolderSrc;

            LogicProvider.Language.SetLanguage(Settings.Default.Language);

            currentLangDictinary = LanguageHelper.GetLanguageDictinary(LogicProvider.Language.CurrentLanguage);
            this.Resources.MergedDictionaries.Add(currentLangDictinary);
        }

        private void BtnStartTranslate_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.MainDir = txtFolderDir.Text;
            Settings.Default.FolderSrc = txtFolderDir.Text;
            Settings.Default.Save();

            this.NavigationService.Navigate(new ModuleSelectionPage());
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
