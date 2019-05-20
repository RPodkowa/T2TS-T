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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV (*.CSV)|*.CSV;";
            openFileDialog.ShowDialog();

            var recordsToTranslateFromFile = File.ReadAllLines(openFileDialog.FileName);
            CreateModel(recordsToTranslateFromFile);
        }

        private void CreateModel(string[] recordsToTranslateFromFile)
        {
            recordsToTranslate = recordsToTranslateFromFile.Select(rs =>
            {
                var spllitedText = rs.Split(';');

                return new TextRecord
                {
                    Id = spllitedText[0],
                    IsCorrectedByHuman = bool.Parse(spllitedText[1]),
                    OriginalText = spllitedText[2],
                    TranslatedText = spllitedText[3]
                };
            }).ToList();
        }
    }
}
