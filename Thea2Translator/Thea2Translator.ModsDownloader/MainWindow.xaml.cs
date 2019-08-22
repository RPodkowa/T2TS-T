using System;
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
using Thea2Translator.ModsDownloader.Model.ViewModel;

namespace Thea2Translator.ModsDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IEnumerable<UploadViewModel> uploadItemList;

        public MainWindow()
        {
            InitializeComponent();

            uploadItemList = PrepareUploadItems();

            lbItemToUpload.ItemsSource = uploadItemList;
        }

        private IEnumerable<UploadViewModel> PrepareUploadItems()
        {
            return new List<UploadViewModel>
            {
                new UploadViewModel{Name="Database", IsChecked = false},
                new UploadViewModel{Name="Modules", IsChecked = true},
                new UploadViewModel{Name="Names", IsChecked = false},
                new UploadViewModel{Name="NamesSpecial", IsChecked = false},
                new UploadViewModel{Name="DatabaseSpecial", IsChecked = false},
                new UploadViewModel{Name="ModulesSpecial", IsChecked = false},
                new UploadViewModel{Name="Test", IsChecked = false}
            };
        }
    }
}
