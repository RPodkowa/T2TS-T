using System.Windows;
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.Logic;

namespace Thea2Translator.DesktopApp.Windows
{
    /// <summary>
    /// Interaction logic for DictinaryWindow.xaml
    /// </summary>
    public partial class DictinaryWindow : Window
    {
        Vocabulary vocabulary;
        VocabularyElem elem;

        public DictinaryWindow(Vocabulary _vocabulary, VocabularyElem _vocabularyElem)
        {
            InitializeComponent();
            vocabulary = _vocabulary;
            elem = _vocabularyElem;

            lblUsageCount.Content = $"Database: {elem.UsageCountDataBase} Modules: {elem.UsageCountModules}";
            checkBoxIsActive.IsChecked = elem.IsActive;
            checkBoxIsConflict.IsChecked = elem.HasConflict;
            txtOriginalWord.Text = elem.OriginalWord;
            txtTranslation.Text = elem.Translation;

            this.SetLanguageDictinary();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            elem.IsActive = checkBoxIsActive.IsChecked.HasValue ? checkBoxIsActive.IsChecked.Value : true;
            elem.HasConflict = checkBoxIsConflict.IsChecked.HasValue ? checkBoxIsConflict.IsChecked.Value : true;
            elem.Translation = txtTranslation.Text;
            elem.OriginalWord = txtOriginalWord.Text;

            vocabulary.SaveElems();
            this.Close();
        }
    }
}
