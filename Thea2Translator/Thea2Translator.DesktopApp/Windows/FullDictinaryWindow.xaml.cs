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
using System.Windows.Shapes;
using Thea2Translator.Logic;

namespace Thea2Translator.DesktopApp.Windows
{
    /// <summary>
    /// Interaction logic for FullDictinaryWindow.xaml
    /// </summary>
    public partial class FullDictinaryWindow : Window
    {
        Vocabulary vocabulary;
        VocabularyElem selectedElem;

        IEnumerable<VocabularyElem> filtredVocabularyElems;
        IEnumerable<VocabularyElem> vocabularyElems;


        public FullDictinaryWindow(bool adminMode)
        {
            InitializeComponent();
            vocabulary = new Vocabulary(FilesType.Vocabulary);
            vocabulary.ReadFromFile(DirectoryType.Cache);

            if (adminMode)
            {
                vocabulary.UpdateByCache(LogicProvider.DataBase);
                vocabulary.UpdateByCache(LogicProvider.Modules);
            }

            vocabularyElems = vocabulary.VocabularyElems;
            lbDictinaryItems.ItemsSource = vocabularyElems;
            saveChangesBtn.IsEnabled = false;
        }

        private void LbDictinaryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedElem = lbDictinaryItems.SelectedItem as VocabularyElem;
            SetDataOnLabels();
        }

        private void SetDataOnLabels()
        {
            if (selectedElem != null)
            {
                usageCountLabel.Content = $"Database: {selectedElem.UsageCountDataBase} Modules: {selectedElem.UsageCountModules}";
                checkBoxIsActive.IsChecked = selectedElem.IsActive;
                checkBoxIsConflict.IsChecked = selectedElem.HasConflict;
                originalWordLabel.Content = selectedElem.OriginalWord;
                txtTranslation.Text = selectedElem.Translation;

                saveChangesBtn.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            selectedElem.IsActive = checkBoxIsActive.IsChecked.Value;
            selectedElem.HasConflict = checkBoxIsConflict.IsChecked.Value;
            selectedElem.Translation = txtTranslation.Text;

            vocabulary.SaveElems();

            saveChangesBtn.IsEnabled = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchingText = txtSearch.Text.ToLower();

            filtredVocabularyElems = vocabularyElems.Where(el => el.OriginalWord.ToLower().Contains(searchingText)
            || el.Translation.ToLower().Contains(searchingText)).ToList();

            lbDictinaryItems.ItemsSource = null;
            lbDictinaryItems.ItemsSource = filtredVocabularyElems;
        }
    }
}
