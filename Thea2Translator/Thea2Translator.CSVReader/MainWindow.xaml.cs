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

        public MainWindow()
        {
            InitializeComponent();

            startHeight = this.Height;
            startSmallFontSize = (double)Application.Current.Resources["smallFontSize"];
            startMediumFontSize = (double)Application.Current.Resources["mediumFontSize"];
            startBigFontSize = (double)Application.Current.Resources["bigFontSize"];
            startVeryBigFontSize = (double)Application.Current.Resources["veryBigFontSize"];

            NavigationCommands.BrowseBack.InputGestures.Clear();
            NavigationCommands.BrowseForward.InputGestures.Clear();
            navigationFrame.Navigate(new HomePage());
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeFontSize(startSmallFontSize, "smallFontSize");
            ChangeFontSize(startMediumFontSize, "mediumFontSize");
            ChangeFontSize(startBigFontSize, "bigFontSize");
            ChangeFontSize(startVeryBigFontSize, "veryBigFontSize");
        }

        private void ChangeFontSize(double startFontSize, string resourceName)
        {
            double heightPerSizePoint = startHeight / startFontSize;

            var height = this.ActualHeight;
            double newFontSize = height / heightPerSizePoint;
            Application.Current.Resources[resourceName] = newFontSize;
        }
    }
}
