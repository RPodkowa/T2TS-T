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
using Thea2Translator.Logic.Filter;

namespace Thea2Translator.DesktopApp.Windows
{
    /// <summary>
    /// Interaction logic for AdvancedFiterWindow.xaml
    /// </summary>
    public partial class AdvancedFiterWindow : Window
    {

        FilterModel filter;

        public AdvancedFiterWindow(FilterModel filter, IEnumerable<string> authors)
        {
            InitializeComponent();
            SetAuthorsInComboBox(authors);
            SetControlsValues(filter);
            SetClearButtonEnable(filter);

            this.filter = filter;
        }

        private void SetClearButtonEnable(FilterModel filter)
        {
            btnClearFilters.IsEnabled = filter.From.HasValue || filter.To.HasValue || !string.IsNullOrWhiteSpace(filter.Author);
        }

        private void SetControlsValues(FilterModel filter)
        {
            cbAuthor.SelectedItem = string.IsNullOrWhiteSpace(filter.Author) ? null : filter.Author;
            dpFrom.SelectedDate = filter.From;
            dpTo.SelectedDate = filter.To;
        }

        private void SetAuthorsInComboBox(IEnumerable<string> authors)
        {
            foreach(var author in authors)
            {
                cbAuthor.Items.Add(author);
            }
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var from = dpFrom.SelectedDate;
            var to = dpTo.SelectedDate;
            var author = cbAuthor.SelectedItem;

            filter.From = from;
            filter.To = to;
            filter.Author = author?.ToString() ;

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            dpFrom.SelectedDate = null;
            dpTo.SelectedDate = null;
            cbAuthor.SelectedItem = null;

            filter.From = null;
            filter.To = null;
            filter.Author = null;

            SetClearButtonEnable(filter);
        }
    }
}
