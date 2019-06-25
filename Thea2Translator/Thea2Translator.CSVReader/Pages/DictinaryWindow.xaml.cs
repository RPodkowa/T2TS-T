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
using Thea2Translator.DesktopApp.Helpers;
using Thea2Translator.Logic;

namespace Thea2Translator.DesktopApp.Pages
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
            lblOriginalWord.Content = elem.OriginalWord;
            txtTranslation.Text = elem.Translation;

            this.SetLanguageDictinary();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            elem.IsActive = checkBoxIsActive.IsChecked.HasValue ? checkBoxIsActive.IsChecked.Value : true;
            elem.Translation = txtTranslation.Text;

            vocabulary.SaveElems();
            this.Close();
        }
    }
}
