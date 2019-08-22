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
using Thea2Translator.DesktopApp.Pages;
using Thea2Translator.Logic.Helpers;

namespace Thea2Translator.DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double startHeight;
        double startSmallFontSize;
        double startMediumFontSize;
        double startBigFontSize;
        double startVeryBigFontSize;

        double maxSmallFontSize;

        public MainWindow()
        {
            InitializeComponent();          
            startHeight = this.Height;
            startSmallFontSize = (double)Application.Current.Resources["smallFontSize"];
            startMediumFontSize = (double)Application.Current.Resources["mediumFontSize"];
            startBigFontSize = (double)Application.Current.Resources["bigFontSize"];
            startVeryBigFontSize = (double)Application.Current.Resources["veryBigFontSize"];

            maxSmallFontSize = (double)Application.Current.Resources["maxSmallFontSize"];       

            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();

            var args = Environment.GetCommandLineArgs();
            if (!args.Contains("START"))
            {
                MessageBox.Show($"Niepoprawne parametry! ({string.Join(",", args)})");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            if (!args.Contains("OFFLINE"))
            {
                try
                {
                    UpdateHelper.TryUpdate(ApplicationType.Updater);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Blad aktualizacji Updater'a! ({ex.ToString()})");
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            }


            navigationFrame.Navigate(new HomePage());
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeFontSize(startSmallFontSize, "smallFontSize", maxSmallFontSize);
            ChangeFontSize(startMediumFontSize, "mediumFontSize");
            ChangeFontSize(startBigFontSize, "bigFontSize");
            ChangeFontSize(startVeryBigFontSize, "veryBigFontSize");
        }

        private void ChangeFontSize(double startFontSize, string resourceName, double? maxSize = null)
        {
            double heightPerSizePoint = startHeight / startFontSize;

            var height = this.ActualHeight;
            double newFontSize = height / heightPerSizePoint;

            if (maxSize.HasValue && newFontSize > maxSize)
                newFontSize = maxSize.Value;

            Application.Current.Resources[resourceName] = newFontSize;
        }
    }
}
