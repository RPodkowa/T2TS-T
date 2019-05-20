using System;
using System.Collections.Generic;
using System.IO;
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
using Thea2Translator.CSVReader.Model;

namespace Thea2Translator.CSVReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IList<TextRecord> recordsToTranslate;
        TextRecord choosenRecord;

        public MainWindow()
        {
            InitializeComponent();
            lblStepInfo.Content = "Wybierz plik CSV do przetłumaczenia";

            lbTranlationItems.Visibility = Visibility.Hidden;
            txtOriginalText.Visibility = Visibility.Hidden;
            txtTranslatedText.Visibility = Visibility.Hidden;
            btnTranslate.Visibility = Visibility.Hidden;
            btnSave.Visibility = Visibility.Hidden;

            txtOriginalText.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV (*.CSV)|*.CSV;";
            openFileDialog.ShowDialog();

            var recordsToTranslateFromFile = File.ReadAllLines(openFileDialog.FileName);
            CreateModel(recordsToTranslateFromFile);

            lblStepInfo.Visibility = Visibility.Hidden;
            btnLoadFile.Visibility = Visibility.Hidden;

            foreach(var record in recordsToTranslate)
            {
                lbTranlationItems.Items.Add(record.OriginalText);
            }

            lbTranlationItems.Visibility = Visibility.Visible;
            txtOriginalText.Visibility = Visibility.Visible;
            txtTranslatedText.Visibility = Visibility.Visible;
            btnTranslate.Visibility = Visibility.Visible;
            btnSave.Visibility = Visibility.Visible;
        }

        private void CreateModel(string[] recordsToTranslateFromFile)
        {
            recordsToTranslate = recordsToTranslateFromFile.Select(rs =>
            {
                var spllitedText = rs.Split(';');

                return new TextRecord
                {
                    Id = spllitedText[0],
                    IsCorrectedByHuman = bool.TryParse(spllitedText[1], out bool isCorrected) ? 
                        isCorrected : false,
                    OriginalText = spllitedText[2],
                    TranslatedText = spllitedText[3]
                };
            }).ToList();
        }

        private void LbTranlationItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = sender as System.Windows.Controls.ListBox;
            choosenRecord = recordsToTranslate.FirstOrDefault(r => r.OriginalText == control.SelectedValue.ToString());

            txtOriginalText.Text = choosenRecord.OriginalText;
            txtTranslatedText.Text = choosenRecord.TranslatedText;
        }

        private void BtnTranslate_Click(object sender, RoutedEventArgs e)
        {
            choosenRecord.TranslatedText = txtTranslatedText.Text;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            using (var textWritter = new StreamWriter(File.Create("translated.csv")))
            {
                foreach (var record in recordsToTranslate)
                {
                    textWritter.WriteLine($"{record.Id};{record.IsCorrectedByHuman};{record.OriginalText};{record.TranslatedText}");
                }
            }
        }
    }
}
