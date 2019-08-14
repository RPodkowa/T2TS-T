using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.DesktopApp.Pages.ModuleSelectionPages;
using Thea2Translator.DesktopApp.ViewModels;
using Thea2Translator.DesktopApp.Windows;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Cache.Interfaces;

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
        public static RoutedCommand OpenDictinaryCommandItemCommand = new RoutedCommand();
        public static RoutedCommand HomeCommandItemCommand = new RoutedCommand();

        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        private string oldStartRange = "";
        private string oldEndRange = "";

        private DictinaryWindow dictinaryWindow;

        private Vocabulary vocabulary;
        private IDataCache dataCache;
        private IStatistic statistic;
        private bool isAdmin;


        IList<string> groupsHistory = new List<string>();

        IList<string> groups;
        IList<CacheElemViewModel> allElements;
        IList<CacheElemViewModel> filtredElements;
        CacheElemViewModel selectedCacheElement;

        public TranslatePage(FilesType fileType, bool isAdmin)
        {
            InitializeComponent();

            this.isAdmin = isAdmin;

            dataCache = null;
            switch (fileType)
            {
                case FilesType.DataBase: dataCache = LogicProvider.DataBase; break;
                case FilesType.Modules: dataCache = LogicProvider.Modules; break;
                case FilesType.Names: dataCache = LogicProvider.Names; break;
            }

            cbItemsToTranslateFilter.SelectedIndex = 0;
            btnOpenGoogle.IsEnabled = false;

            dataCache.ReloadElems(true, true, true);
            vocabulary = dataCache.Vocabulary;
            statistic = LogicProvider.Statistic;

            statistic.Reload(dataCache);
            RealodStatistic();
            
            allElements = dataCache.CacheElems.Select(c => new CacheElemViewModel(c)).ToList();
            filtredElements = allElements;
            groups = dataCache.Groups;

            foreach (var group in groups)
            {
                cbGroups.Items.Add(group);
            }

            lbItemsToTranslate.ItemsSource = filtredElements;
            lblCount.Content = $"Elementów: {filtredElements.Count.ToString()}";

            this.SetLanguageDictinary();

            SaveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            GoogleCommand.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control));
            NextItemCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            OpenDictinaryCommandItemCommand.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
            HomeCommandItemCommand.InputGestures.Add(new KeyGesture(Key.Escape));

            CommandBindings.Add(new CommandBinding(SaveCommand, (d, e) =>
            {
                SaveToFile();
            }));

            CommandBindings.Add(new CommandBinding(GoogleCommand, (d, e) =>
            {
                OpenGoogleTranslate();
            }));

            CommandBindings.Add(new CommandBinding(NextItemCommand, (d, e) =>
            {
                ChooseNextItem();
            }));

            CommandBindings.Add(new CommandBinding(OpenDictinaryCommandItemCommand, (d, e) =>
            {
                OpenVocabulary();
            }));

            CommandBindings.Add(new CommandBinding(HomeCommandItemCommand, (d, e) =>
            {
                BackToPrevPage();
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
            if (selectedCacheElement != null
                && txtTranslatedText.Text != selectedCacheElement.CacheElem.OriginalText)
            {
                selectedCacheElement.CacheElem.SetTranslated(txtTranslatedText.Text);
            }

            selectedCacheElement = lbItemsToTranslate.SelectedItem as CacheElemViewModel;

            txtOriginalText.Text = selectedCacheElement?.CacheElem?.OriginalText;

            if (selectedCacheElement?.CacheElem !=null)
            {
                var text = selectedCacheElement.CacheElem.TranslatedText;
                if (selectedCacheElement.CacheElem.HasConflict)                
                    text+="\r\n===============\r\n"+ selectedCacheElement.CacheElem.ConflictTranslatedText;
                
                txtTranslatedText.Text = text;
            }

            btnOpenGoogle.IsEnabled = selectedCacheElement == null ? false : true;

            RefreshVocabularyList();
            RealodStatistic();
            SetAvaibleGroups();
        }

        private void SetAvaibleGroups()
        {
            if (selectedCacheElement != null)
            {
                groups = selectedCacheElement.CacheElem.Groups;
                var allGroup = cbGroups.Items[0];
                var startingGroups = cbGroups.Items[1];
                var selectedGroup = cbGroups.SelectedItem;

                cbGroups.Items.Clear();
                cbGroups.Items.Add(allGroup);
                cbGroups.Items.Add(startingGroups);

                foreach (var group in groups)
                {
                    cbGroups.Items.Add(group);
                }

                var index = cbGroups.Items.IndexOf(selectedGroup);

                cbGroups.SelectedIndex = index != -1 ? index : 0;
            }
        }

        private void RealodStatistic()
        {
            statistic.Reload(dataCache);

            lblState.Content = statistic.GetSummary();
        }

        private void SaveToFile()
        {
            if (txtTranslatedText.Text != null && selectedCacheElement?.CacheElem != null)
            {
                selectedCacheElement.CacheElem.SetTranslated(txtTranslatedText.Text);

                var index = lbItemsToTranslate.SelectedIndex;

                FilterItems();
                lbItemsToTranslate.SelectedIndex = index;
            }

            dataCache.UpdateVocabulary(vocabulary);
            dataCache.SaveElems(true);

            if (cbItemsToTranslateFilter.SelectedIndex != 1)
                ChooseNextItem();
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

                filtredElements = allElements.Skip(start).Take(end - start).ToList();
                foreach (var elem in filtredElements)
                {
                    elem.CacheElem.ResetAdventureNodeRecord();
                }

                switch (cbItemsToTranslateFilter.SelectedIndex)
                {
                    case 0: filtredElements = filtredElements.ToList(); break;
                    case 1: filtredElements = filtredElements.Where(c => c.CacheElem.ToTranslate).ToList(); break;
                    case 2: filtredElements = filtredElements.Where(c => c.CacheElem.ToConfirm).ToList(); break;
                    case 3: filtredElements = filtredElements.Where(c => c.CacheElem.HasConflict).ToList(); break;
                }

                if (txtSearch.Text != "")
                {
                    var text = txtSearch.Text.ToLower();
                    filtredElements = filtredElements.Where(e => e.CacheElem.OriginalText.ToLower().Contains(text)
                    || e.CacheElem.TranslatedText.ToLower().Contains(text))
                    .ToList();
                }

                if (cbGroups.SelectedIndex != 0 && cbGroups.SelectedIndex != -1)
                {
                    var filterGroups = new List<string>();
                    if (cbGroups.SelectedIndex == 1)
                    {
                        filterGroups = dataCache.GetStartingGroups().ToList();
                        filtredElements = filtredElements.Where(e => e.CacheElem.OccursInStartingGroup(filterGroups)).ToList();
                    }
                    else
                    {
                        filterGroups.Add(cbGroups.SelectedValue.ToString());
                        filtredElements = filtredElements.Where(e => e.CacheElem.Groups.Contains(cbGroups.SelectedValue)).ToList();
                    }

                    foreach (var elem in filtredElements)
                    {
                        elem.CacheElem.SetGroupContext(filterGroups);
                    }
                }

                //lbItemsToTranslate.ItemsSource = null;
                lbItemsToTranslate.ItemsSource = filtredElements;
                lblCount.Content = $"Elementów: {filtredElements.Count.ToString()}";
            }
        }

        private void OpenGoogleTranslate()
        {
            if (selectedCacheElement != null)
            {
                var link = selectedCacheElement.CacheElem.GetTranslateLink();

                wbGoogleTranslate.Address = link;
                btnOpenGoogle.IsEnabled = false;
            }
        }

        private void CbGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                (cbGroups.SelectedIndex == 0 || cbGroups.SelectedIndex == 1)  &&
                (e.AddedItems.Count>0 && e.RemovedItems.Count>0 ) &&
                (e.AddedItems[0].ToString()!= e.RemovedItems[0].ToString())
                )
            {
                var oldSelectedIndex = cbGroups.SelectedIndex;
                groups = dataCache.Groups;
                if (cbGroups.SelectedIndex == 1) groups = dataCache.GetStartingGroups();
                var allGroup = cbGroups.Items[0];
                var startingGroups = cbGroups.Items[1];

                cbGroups.Items.Clear();
                cbGroups.Items.Add(allGroup);
                cbGroups.Items.Add(startingGroups);

                foreach (var group in groups)
                {
                    cbGroups.Items.Add(group);
                }

                cbGroups.SelectedIndex = oldSelectedIndex;
            }

            FilterItems();
        }
        private void SetFilteGroups(string group)
        {
            if (string.IsNullOrEmpty(group))
                return;

            var allGroup = cbGroups.Items[0];
            var startingGroups = cbGroups.Items[1];

            cbGroups.Items.Clear();
            cbGroups.Items.Add(allGroup);
            cbGroups.Items.Add(startingGroups);
            cbGroups.Items.Add(group);

            var index = cbGroups.Items.IndexOf(group);

            cbGroups.SelectedIndex = index != -1 ? index : 0;

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

                if (isGreater && currentValue < secondValue)
                {
                    txtSecond.Text = (currentValue - 1).ToString();
                }
                else if (!isGreater && currentValue > secondValue)
                {
                    txtSecond.Text = (currentValue + 1).ToString();
                }
            }
        }

        private void LbDictinary_SelectionChange(object sender, SelectionChangedEventArgs e)
        {
        }

        private void lbDictinaryItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                DeactivationVocabularyItem();
            }

            if (e.Key == Key.Enter)
            {
                ShowSelectedVocabularyDialog();
            }
        }

        private void DeactivationVocabularyItem()
        {
            if (lbDictinaryItems.SelectedItem == null)
                return;

            var vocabularyElem = lbDictinaryItems.SelectedItem as VocabularyElem;
            if (vocabularyElem == null)
                return;

            vocabularyElem.IsActive = false;
            RefreshVocabularyList();
        }

        private void RefreshVocabularyList()
        {
            if (vocabulary is null) return;
            if (selectedCacheElement?.CacheElem?.OriginalText == null)
                return;

            var vocabularyElems = vocabulary.GetElemsForText(selectedCacheElement?.CacheElem?.OriginalText);

            var selectedIndex = lbDictinaryItems.SelectedIndex;
            lbDictinaryItems.ItemsSource = vocabularyElems;
            lbDictinaryItems.SelectedIndex = Math.Min(selectedIndex, lbDictinaryItems.Items.Count - 1);
        }

        private void lbDictinaryItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowSelectedVocabularyDialog();
        }

        private void ShowSelectedVocabularyDialog()
        {
            if (lbDictinaryItems.SelectedItem == null)
                return;

            if (dictinaryWindow != null)
                dictinaryWindow.Close();

            dictinaryWindow = new DictinaryWindow(vocabulary, lbDictinaryItems.SelectedItem as VocabularyElem);
            dictinaryWindow.ShowDialog();
            RefreshVocabularyList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterItems();
        }

        private void SetConfirmOnCache(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var index = lbItemsToTranslate.Items.IndexOf(checkBox.DataContext);

            filtredElements[index].CacheElem.SetConfirmation(checkBox.IsChecked.Value);
            dataCache.SaveElems();
            FilterItems();
        }
        private void SetResolvedOnCache(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var index = lbItemsToTranslate.Items.IndexOf(checkBox.DataContext);

            var cacheElem = filtredElements[index].CacheElem;
            if (!cacheElem.WithConflictText())
            {
                checkBox.IsChecked = false;
                return;
            }

            filtredElements[index].CacheElem.ResolveConflict(!checkBox.IsChecked.Value, txtTranslatedText.Text);
            dataCache.SaveElems();
            FilterItems();
        }

        private void AddMenuItem(System.Windows.Forms.ContextMenuStrip menu, string name, string group)
        {
            var item = new System.Windows.Forms.ToolStripMenuItem();
            item.Text = name;
            item.Click += (s, e) => MenuClick(group, s, e);
            menu.Items.Add(item);
        }

        private void MenuClick(string group, object sender, EventArgs e)
        {
            groupsHistory.Add(cbGroups.SelectedValue.ToString());
            SetFilteGroups(group);
        }  

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile();
        }

        private void BtnOpenGoogle_Click(object sender, RoutedEventArgs e)
        {
            OpenGoogleTranslate();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            BackToPrevPage();
        }

        private void BackToPrevPage()
        {
            if (isAdmin)
                this.NavigationService.Navigate(new ModuleSelectionAdminPage());
            else
                this.NavigationService.Navigate(new ModuleSelectionUserPage());
        }

        private void BtnVocabulary_Click(object sender, RoutedEventArgs e)
        {
            OpenVocabulary();
        }

        private static void OpenVocabulary()
        {
            FullDictinaryWindow fullDictinary = new FullDictinaryWindow(false);
            fullDictinary.Show();
        }

        private void BtnNavigationPrev_Click(object sender, RoutedEventArgs e)
        {
            if (groupsHistory == null || groupsHistory.Count == 0)
                return;
            var lastIndex = groupsHistory.Count - 1;
            var lastGroup = groupsHistory[lastIndex];
            groupsHistory.RemoveAt(lastIndex);
            SetFilteGroups(lastGroup);
        }

        private void BtnNavigationNext_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCacheElement == null)
                return;

            var actualGroup = cbGroups.SelectedValue.ToString();

            var relations = selectedCacheElement.CacheElem.GetNextElemRelations(dataCache, actualGroup);
            if (relations == null || relations.Count == 0)
                return;

            System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();

            foreach (var relation in relations)
            {
                AddMenuItem(contextMenu, relation.GetMenuItemName(), relation.NextElemGroup);
            }

            contextMenu.Show(System.Windows.Forms.Cursor.Position);
        }
    }
}
