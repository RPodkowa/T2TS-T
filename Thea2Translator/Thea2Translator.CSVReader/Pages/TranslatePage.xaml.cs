using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.DesktopApp.ViewModels;
using Thea2Translator.Logic;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for WorkPage.xaml
    /// </summary>
    public partial class TranslatePage : Page
    {
        public static RoutedCommand SaveCommand = new RoutedCommand();
        public static RoutedCommand GoogleCommand = new RoutedCommand();
        public static RoutedCommand NextItemCommand = new RoutedCommand();

        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        
        private string oldStartRange = "";
        private string oldEndRange = "";

        IDataCache dataCache;

        IList<string> groups;
        IList<CacheElemViewModel> allElements;
        IList<CacheElemViewModel> filtredElements;
        CacheElemViewModel selectedCacheElement;

        public TranslatePage(FilesType fileType)
        {
            InitializeComponent();

            dataCache = null;
            switch(fileType)
            {
                case FilesType.DataBase: dataCache = LogicProvider.DataBase; break;
                case FilesType.Modules: dataCache = LogicProvider.Modules; break;
                case FilesType.Names: dataCache = LogicProvider.Names; break;
            }           

            cbItemsToTranslateFilter.SelectedIndex = 0;
            btnGoogle.IsEnabled = false;

            dataCache.ReloadElems(true, true);
            allElements = dataCache.CacheElems.Select(c => new CacheElemViewModel(c)).ToList();
            filtredElements = allElements;
            groups = dataCache.Groups;

            foreach (var group in groups)
            {
                cbGroups.Items.Add(group);
            }

            lbItemsToTranslate.ItemsSource = filtredElements;

            this.SetLanguageDictinary();

            SaveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            GoogleCommand.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control));
            NextItemCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));

            CommandBindings.Add(new CommandBinding(SaveCommand, (d, e) => {
                SaveToFile();
            }));

            CommandBindings.Add(new CommandBinding(GoogleCommand, (d, e) => {
                OpenGoogleTranslate();
            }));

            CommandBindings.Add(new CommandBinding(NextItemCommand, (d, e) => {
                ChooseNextItem();
            }));
        }

        private void ChooseNextItem()
        {
            if (selectedCacheElement == null)
            {
                lbItemsToTranslate.SelectedIndex = 0;
            }
            else if (lbItemsToTranslate.SelectedIndex + 1 < filtredElements.Count)
            {
                lbItemsToTranslate.SelectedIndex++;
            }
        }

        private void LbItemsToTranslate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCacheElement = lbItemsToTranslate.SelectedItem as CacheElemViewModel;

            txtOriginalText.Text = selectedCacheElement?.CacheElem?.OriginalText;
            txtTranslatedText.Text = selectedCacheElement?.CacheElem?.TranslatedText;

            btnGoogle.IsEnabled = selectedCacheElement == null ? false : true; ;
        }

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile();
        }

        private void SaveToFile()
        {
            selectedCacheElement.CacheElem.SetTranslated(txtTranslatedText.Text);
            selectedCacheElement.CacheElem.IsCorrectedByHuman = true;

            dataCache.SaveElems(true);
        }

        private void CbItemsToTranslateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterItems();
        }

        private void FilterItems()
        {
            if (cbItemsToTranslateFilter != null && cbGroups != null && txtStartRange != null && txtEndRange != null)
            {
                var start = (txtStartRange.Text != "" ? 
                    int.Parse(txtStartRange.Text) : 1);
                start--;

                var end = txtEndRange.Text != "" ?
                    int.Parse(txtEndRange.Text) : allElements.Count;

                switch (cbItemsToTranslateFilter.SelectedIndex)
                {
                    case 0: filtredElements = allElements .ToList(); break;
                    case 1: filtredElements = allElements.Where(c => c.CacheElem.ToTranslate) .ToList(); break;
                    case 2: filtredElements = allElements.Where(c => c.CacheElem.ToConfirm).ToList(); break;
                }

                if (cbGroups.SelectedIndex != 0)
                {
                    filtredElements = filtredElements.Where(e =>
                    e.CacheElem.Groups.Contains(cbGroups.SelectedValue))
                        .Skip(start)
                        .Take(end - start)
                        .ToList();
                }
                else
                {
                    filtredElements = filtredElements
                        .Skip(start)
                        .Take(end - start)
                        .ToList();
                }

                lbItemsToTranslate.ItemsSource = null;
                lbItemsToTranslate.ItemsSource = filtredElements;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ModuleSelectionPage());
        }

        private void btnGoogle_Click(object sender, RoutedEventArgs e)
        {
            OpenGoogleTranslate();
        }

        private void OpenGoogleTranslate()
        {
            if (selectedCacheElement != null)
            {
                var link = selectedCacheElement.CacheElem.GetTranslateLink();

                wbGoogleTranslate.Address = link;
                btnGoogle.IsEnabled = false;
            }
        }

        private void CbGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterItems();
        }

        private void TxtStartRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckRangeTextbox(txtStartRange, ref oldStartRange);
        }

        private void TxtEndRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckRangeTextbox(txtEndRange, ref oldEndRange);
        }

        private void CheckRangeTextbox(TextBox textBox, ref string oldValue)
        {
            if (textBox.Text == "")
                return;

            textBox.Text = IsTextAllowed(textBox.Text) ? textBox.Text : oldValue;
            oldValue = textBox.Text;
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void TxtStartRange_LostFocus(object sender, RoutedEventArgs e)
        {

            CheckRangeValues(txtStartRange, txtEndRange, false);
            FilterItems();
        }

        private void TxtEndRange_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckRangeValues(txtEndRange, txtStartRange, true);
            FilterItems();
        }

        private void CheckRangeValues(TextBox txtCurrent, TextBox txtSecond, bool isGreater)
        {
            if (txtCurrent.Text != "" && txtSecond.Text != "")
            {
                var currentValue = int.Parse(txtCurrent.Text);
                var secondValue = int.Parse(txtSecond.Text);

                if(isGreater && currentValue < secondValue)
                {
                    txtSecond.Text = (currentValue - 1).ToString();
                }
                else if(!isGreater && currentValue > secondValue)
                {
                    txtSecond.Text = (currentValue + 1).ToString();
                }
            }
        }     
    }
}
