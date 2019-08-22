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
        public static RoutedCommand SaveCommand = new RoutedCommand(); //Ctrl+S
        public static RoutedCommand ConfirmCommand = new RoutedCommand(); //Ctrl+P
        public static RoutedCommand ConflictCommand = new RoutedCommand(); //Ctrl+K
        public static RoutedCommand GoogleCommand = new RoutedCommand(); //Ctrl+G
        public static RoutedCommand NextItemCommand = new RoutedCommand(); //Ctrl+Down
        public static RoutedCommand PrevItemCommand = new RoutedCommand(); //Ctrl+Up
        public static RoutedCommand NextQuestItemCommand = new RoutedCommand(); //Ctrl+Right
        public static RoutedCommand PrevQuestItemCommand = new RoutedCommand(); //Ctrl+Left        
        public static RoutedCommand OpenDictinaryCommand = new RoutedCommand(); //Ctrl+D
        public static RoutedCommand FunctionsCommand = new RoutedCommand(); //F1
        public static RoutedCommand HomeCommand = new RoutedCommand(); //ESC

        private IList<System.Windows.Forms.ToolStripMenuItem> menuItems = new List<System.Windows.Forms.ToolStripMenuItem>();

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
            filtredElements = allElements.Where(x => x.IsActive).ToList();
            groups = dataCache.Groups;

            foreach (var group in groups)
            {
                cbGroups.Items.Add(group);
            }

            lbItemsToTranslate.ItemsSource = filtredElements;
            lblCount.Content = $"Elementów: {filtredElements.Count.ToString()}";

            this.SetLanguageDictinary();
            AddComands();
        }

        private void LbItemsToTranslate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedCacheElement != null && txtTranslatedText.Text != selectedCacheElement.CacheElem.OriginalText)
            {
                selectedCacheElement.CacheElem.SetTranslated(txtTranslatedText.Text, true);
            }

            selectedCacheElement = lbItemsToTranslate.SelectedItem as CacheElemViewModel;

            txtOriginalText.Text = selectedCacheElement?.CacheElem?.OriginalText;

            if (selectedCacheElement?.CacheElem != null)
            {
                var text = selectedCacheElement.CacheElem.TranslatedText;
                if (selectedCacheElement.CacheElem.HasConflict)
                    text += CacheElem.ConflictSeparator + selectedCacheElement.CacheElem.ConflictTranslatedText;

                txtTranslatedText.Text = text;
            }

            btnOpenGoogle.IsEnabled = selectedCacheElement == null ? false : true;

            RefreshVocabularyList();
            RealodStatistic();
            SetAvaibleGroups();
        }

        private void GoToSelectedItem()
        {
            lbItemsToTranslate.ScrollIntoView(lbItemsToTranslate.SelectedItem);
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

        private void CbItemsToTranslateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterItems();
        }

        private void FilterItems(bool withTryChooseFirstAdventure = false)
        {
            if (cbItemsToTranslateFilter != null && cbGroups != null && txtStartRange != null && txtEndRange != null)
            {
                var start = (txtStartRange.Text != "" ?
                    int.Parse(txtStartRange.Text) : 1);
                start--;

                var end = txtEndRange.Text != "" ?
                    int.Parse(txtEndRange.Text) : allElements.Count;

                filtredElements = allElements.Skip(start).Take(end - start).Where(x => x.IsActive).ToList();
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
                    case 4: filtredElements = allElements.Where(c => c.CacheElem.IsInactive).ToList(); break;                        
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

                    if (withTryChooseFirstAdventure)
                        TryChooseFirstAdventureItem();
                }

                //lbItemsToTranslate.ItemsSource = null;
                lbItemsToTranslate.ItemsSource = filtredElements;
                lblCount.Content = $"Elementów: {filtredElements.Count.ToString()}";

                GoToSelectedItem();
            }
        }
        
        private void TryChooseFirstAdventureItem()
        {
            if (filtredElements == null) return;
            if (filtredElements.Count == 0) return;

            CacheElemViewModel firstElem = null;
            int? index = null;
            for (int i = 0; i < filtredElements.Count && i < 20; i++)
            {
                if (filtredElements[i].CacheElem.IsAdventureNodeRecord)
                {
                    index = i;
                    break;
                }
            }

            if (index.HasValue) lbItemsToTranslate.SelectedIndex = index.Value;
        }

        private void CbGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool withTryChooseFirstAdventure = false;
            if (
                (cbGroups.SelectedIndex == 0 || cbGroups.SelectedIndex == 1) &&
                (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0) &&
                (e.AddedItems[0].ToString() != e.RemovedItems[0].ToString())
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

            FilterItems(true);
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
            if (e.Key == Key.Delete)
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

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile(false);
        }

        private void BtnOpenGoogle_Click(object sender, RoutedEventArgs e)
        {
            OpenGoogleTranslate();
        }

        private void BtnFunctions_Click(object sender, RoutedEventArgs e)
        {
            ShowMainMenu();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            BackToPrevPage();
        }

        private void BtnVocabulary_Click(object sender, RoutedEventArgs e)
        {
            OpenVocabulary();
        }

        private void BtnNavigationPrev_Click(object sender, RoutedEventArgs e)
        {
            UseNavigationFromButton(false);
        }

        private void BtnNavigationNext_Click(object sender, RoutedEventArgs e)
        {
            UseNavigationFromButton(true);
        }

        #region Commands
        private void AddComands()
        {
            AddCommand(SaveCommand, new KeyGesture(Key.S, ModifierKeys.Control, "Ctrl+S"), "Zapis");
            AddCommand(ConfirmCommand, new KeyGesture(Key.P, ModifierKeys.Control, "Ctrl+P"), "Potwierdź");
            AddCommand(ConflictCommand, new KeyGesture(Key.K, ModifierKeys.Control, "Ctrl+K"), "Rozwiązany konflikt");
            AddCommand(GoogleCommand, new KeyGesture(Key.G, ModifierKeys.Control, "Ctrl+G"), "Google");
            AddCommand(PrevItemCommand, new KeyGesture(Key.Down, ModifierKeys.Alt, "Alt+Góra"), "Poprzednia fraza");
            AddCommand(NextItemCommand, new KeyGesture(Key.Up, ModifierKeys.Alt, "Alt+Dół"), "Następna fraza");
            AddCommand(PrevQuestItemCommand, new KeyGesture(Key.Left, ModifierKeys.Alt, "Alt+Lew"), "Przygoda - Wstecz");
            AddCommand(NextQuestItemCommand, new KeyGesture(Key.Right, ModifierKeys.Alt, "Alt+Prawo"), "Przygoda - Dalej");            
            AddCommand(OpenDictinaryCommand, new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D"), "Słownik");
            AddCommand(FunctionsCommand, new KeyGesture(Key.F1, ModifierKeys.None, "F1"), "Funkcje");
            AddCommand(HomeCommand, new KeyGesture(Key.Escape, ModifierKeys.None, "Esc"), "Wyjście");
        }

        private void AddCommand(RoutedCommand command, KeyGesture keyGesture, string description)
        {
            command.InputGestures.Add(keyGesture);
            CommandBindings.Add(new CommandBinding(command, (d, e) => { RunCommand(keyGesture); }));

            var item = new System.Windows.Forms.ToolStripMenuItem();
            item.Text = $"{description}";
            item.ShortcutKeyDisplayString = keyGesture.DisplayString;
            item.ShowShortcutKeys = true;
            item.Click += (s, e) => RunCommand(keyGesture);
            menuItems.Add(item);
        }

        private void RunCommand(KeyGesture keyGesture)
        {
            if (keyGesture.Key == Key.S && keyGesture.Modifiers == ModifierKeys.Control) SaveToFile(false);
            if (keyGesture.Key == Key.P && keyGesture.Modifiers == ModifierKeys.Control) ChangeConfirmation();
            if (keyGesture.Key == Key.K && keyGesture.Modifiers == ModifierKeys.Control) ChangeConflictResolved();
            if (keyGesture.Key == Key.G && keyGesture.Modifiers == ModifierKeys.Control) OpenGoogleTranslate();
            if (keyGesture.Key == Key.Up && keyGesture.Modifiers == ModifierKeys.Alt) ChooseItem(false);
            if (keyGesture.Key == Key.Down && keyGesture.Modifiers == ModifierKeys.Alt) ChooseItem(true);
            if (keyGesture.Key == Key.Left && keyGesture.Modifiers == ModifierKeys.Alt) UseNavigationFromMenu(false);
            if (keyGesture.Key == Key.Right && keyGesture.Modifiers == ModifierKeys.Alt) UseNavigationFromMenu(true);
            if (keyGesture.Key == Key.D && keyGesture.Modifiers == ModifierKeys.Control) OpenVocabulary();
            if (keyGesture.Key == Key.F1) ShowMainMenu();
            if (keyGesture.Key == Key.Escape) BackToPrevPage();
        }

        private void SaveToFile(bool withChangeConfirmation, bool withResolveConflict = false)
        {
            if (txtTranslatedText.Text != null && selectedCacheElement?.CacheElem != null)
            {
                if (withResolveConflict)
                {
                    selectedCacheElement.CacheElem.ResolveConflict(true, txtTranslatedText.Text);
                    selectedCacheElement.HasConflict = false;
                }
                else
                {
                    selectedCacheElement.CacheElem.SetTranslated(txtTranslatedText.Text, true);
                    if (withChangeConfirmation)
                    {
                        selectedCacheElement.CacheElem.ChangeConfirmation();
                        selectedCacheElement.IsConfirm = selectedCacheElement.CacheElem.IsCorrectedByHuman;
                    }
                }

                var index = lbItemsToTranslate.SelectedIndex;

                FilterItems();
                lbItemsToTranslate.SelectedIndex = index;
            }

            dataCache.UpdateVocabulary(vocabulary);
            dataCache.SaveElems(true);

            if (withChangeConfirmation) ChooseItem(true);
        }
        private void ChangeConfirmation()
        {
            SaveToFile(true);
        }
        private void ChangeConflictResolved()
        {
            SaveToFile(false, true);
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
        private void ChooseItem(bool next)
        {
            if (selectedCacheElement == null)
            {
                lbItemsToTranslate.SelectedIndex = 0;
                return;
            }

            int index = lbItemsToTranslate.SelectedIndex;
            if (next) index++;
            else index--;

            if (index >= filtredElements.Count) index = 0;
            if (index < 0) index = filtredElements.Count - 1;

            lbItemsToTranslate.SelectedIndex = index;
            FocusManager.SetFocusedElement(this, txtTranslatedText);
        }
        private void UseNavigationFromButton(bool next)
        {
            UseNavigation(true, next);
        }
        private void UseNavigationFromMenu(bool next)
        {
            UseNavigation(false, next);
        }
        private void UseNavigation(bool fromButton, bool next)
        {
            if (next) NavigationNext(fromButton);
            else NavigationPrev();
        }
        private void NavigationPrev()
        {
            GoToGroup(false, "");
        }
        private void NavigationNext(bool fromButton)
        {
            if (selectedCacheElement == null)
                return;

            var actualGroup = cbGroups.SelectedValue.ToString();

            var relations = selectedCacheElement.CacheElem.GetNextElemRelations(dataCache, actualGroup);
            if (relations == null || relations.Count == 0)
                return;

            if (relations.Count == 1)
            {
                GoToGroup(true, relations[0].NextElemGroup);
                return;
            }

            System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();

            foreach (var relation in relations)
            {
                AddMenuItem(contextMenu, relation.GetMenuItemName(), relation.NextElemGroup);
            }

            if (fromButton) contextMenu.Show(System.Windows.Forms.Cursor.Position);
            else
            {
                var point = lbItemsToTranslate.PointToScreen(new Point(0, 0));
                contextMenu.Show(new System.Drawing.Point((int)point.X, (int)point.Y));
            }            
        }
        private void AddMenuItem(System.Windows.Forms.ContextMenuStrip menu, string name, string group)
        {
            var item = new System.Windows.Forms.ToolStripMenuItem();
            item.Text = name;
            item.Click += (s, e) => GoToGroup(true, group);
            menu.Items.Add(item);
        }

        private void GoToGroup(bool moveForward, string group)
        {
            if (moveForward)
                groupsHistory.Add(cbGroups.SelectedValue.ToString());
            else
            {
                if (groupsHistory == null || groupsHistory.Count == 0)
                    return;

                var lastIndex = groupsHistory.Count - 1;
                group = groupsHistory[lastIndex];
                groupsHistory.RemoveAt(lastIndex);                
            }

            SetFilteGroups(group);            
        }
        private static void OpenVocabulary()
        {
            FullDictinaryWindow fullDictinary = new FullDictinaryWindow(false);
            fullDictinary.Show();
        }
        private void ShowMainMenu()
        {
            System.Windows.Forms.ContextMenuStrip contextMenu = new System.Windows.Forms.ContextMenuStrip();

            foreach (var menuItem in menuItems)
            {
                contextMenu.Items.Add(menuItem);
            }

            contextMenu.Show(System.Windows.Forms.Cursor.Position);
        }
        private void BackToPrevPage()
        {
            if (isAdmin)
                this.NavigationService.Navigate(new ModuleSelectionAdminPage());
            else
                this.NavigationService.Navigate(new ModuleSelectionUserPage());
        }
        #endregion
    }
}
