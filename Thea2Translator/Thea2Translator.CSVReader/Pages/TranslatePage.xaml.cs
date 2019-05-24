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
using Thea2Translator.DesktopApp.ViewModels;
using Thea2Translator.Logic;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for WorkPage.xaml
    /// </summary>
    public partial class TranslatePage : Page
    {
        IDataCache dataCache;
        IList<CacheElemViewModel> allElements;
        IList<CacheElemViewModel> filtredElements;
        CacheElemViewModel selectedCacheElement;

        public TranslatePage(FilesType fileType)
        {
            InitializeComponent();

            dataCache = fileType == FilesType.DataBase ?
                LogicProvider.DataBase : LogicProvider.Modules;

            btnTranslate.IsEnabled = false;
            btnSaveToFile.IsEnabled = false;

            cbItemsToTranslateFilter.SelectedIndex = 0;

            dataCache.ReloadElems(true, true);
            allElements = dataCache.CacheElems.Select(c => new CacheElemViewModel(c)).ToList();
            filtredElements = allElements;

            lbItemsToTranslate.ItemsSource = filtredElements;
        }

        private void BtnTranslate_Click(object sender, RoutedEventArgs e)
        {
            selectedCacheElement.CacheElem.SetTranslated(txtTranslatedText.Text);
            selectedCacheElement.CacheElem.IsCorrectedByHuman = true;

            btnSaveToFile.IsEnabled = true;
        }

        private void LbItemsToTranslate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCacheElement = lbItemsToTranslate.SelectedItem as CacheElemViewModel;

            txtOriginalText.Text = selectedCacheElement?.CacheElem?.OriginalNormalizedText;
            txtTranslatedText.Text = selectedCacheElement?.CacheElem?.TranslatedNormalizedText;

            btnTranslate.IsEnabled = selectedCacheElement == null ? false : true;
        }

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            dataCache.SaveElems(true);
            btnSaveToFile.IsEnabled = false;
        }

        private void CbItemsToTranslateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbItemsToTranslateFilter.SelectedIndex)
            {
                case 0:
                    filtredElements = allElements;
                    break;
                case 1:
                    filtredElements = allElements.Where(c => c.CacheElem.ToTranslate).ToList();
                    break;
            }

            btnTranslate.IsEnabled = false;
            lbItemsToTranslate.ItemsSource = null;
            lbItemsToTranslate.ItemsSource = filtredElements;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ModuleSelectionPage());
        }

        private void btnGoogle_Click(object sender, RoutedEventArgs e)
        {
            var link = selectedCacheElement.CacheElem.GetTranslateLink();
            System.Diagnostics.Process.Start(link);
        }
    }
}
