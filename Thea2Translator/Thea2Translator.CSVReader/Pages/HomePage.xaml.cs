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
using Thea2Translator.Helpers;

namespace Thea2Translator.CSVReader.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            txtFolderDir.IsEnabled = false;
            btnStartTranslate.IsEnabled = false;
        }

        private void BtnStartTranslate_Click(object sender, RoutedEventArgs e)
        {
            FileHelper.MainDir = txtFolderDir.Text;
            this.NavigationService.Navigate(new WorkPage());
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
    }
}
