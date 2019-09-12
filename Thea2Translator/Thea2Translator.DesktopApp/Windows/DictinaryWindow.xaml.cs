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

            var content = elem.GetUsagesText();
            lblUsageCount.Content = content;
            checkBoxIsConflict.IsChecked = elem.HasConflict;
            txtOriginalWord.Text = elem.OriginalWord;
            txtTranslation.Text = elem.Translation;

            if (!elem.NewElem)
                txtOriginalWord.IsEnabled = false;

            this.SetLanguageDictinary();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            elem.HasConflict = checkBoxIsConflict.IsChecked.HasValue ? checkBoxIsConflict.IsChecked.Value : true;
            elem.Translation = txtTranslation.Text;
            elem.OriginalWord = txtOriginalWord.Text;

            if (elem.NewElem)
            {
                elem.CalculateUsages();
                vocabulary.VocabularyElems.Add(elem);
            }

            vocabulary.SaveElems();
            this.Close();
        }
    }
}
