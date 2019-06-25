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
using Thea2Translator.DesktopApp.Properties;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Mods;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for ModWindow.xaml
    /// </summary>
    public partial class ModWindow : Window
    {
        public ModWindow()
        {
            InitializeComponent();
            txtTitle.Text = Settings.Default.ModTitle;
            txtBody.Text = Settings.Default.ModBody;
            txtState.Text = LogicProvider.DataBase.GetSummary();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ModTitle = txtTitle.Text;
            Settings.Default.ModBody = txtBody.Text;
            Settings.Default.Save();

            var manager = new ModManager(txtTitle.Text, txtBody.Text, txtState.Text);
            manager.PrepareMod();
        }
    }
}
