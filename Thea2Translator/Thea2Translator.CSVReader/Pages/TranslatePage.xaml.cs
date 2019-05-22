﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Thea2Translator.Logic;
using Thea2Translator.Logic.Cache;
using Thea2Translator.Logic.Cache.Interfaces;

namespace Thea2Translator.DesktopApp.Pages
{
    /// <summary>
    /// Interaction logic for WorkPage.xaml
    /// </summary>
    public partial class TranslatePage : Page
    {
        IDataCache dataCache;
        IList<CacheElem> allElements;
        IList<CacheElem> filtredElements;
        CacheElem selectedCacheElement;

        public TranslatePage(FilesType fileType)
        {
            InitializeComponent();

            dataCache = fileType == FilesType.DataBase ?
                LogicProvider.DataBase : LogicProvider.Modules;

            btnTranslate.IsEnabled = false;
            btnSaveToFile.IsEnabled = false;

            cbItemsToTranslateFilter.SelectedIndex = 0;

            dataCache.ReloadElems();
            allElements = dataCache.CacheElems;
            filtredElements = allElements;

            lbItemsToTranslate.ItemsSource = filtredElements;
        }

        private void BtnTranslate_Click(object sender, RoutedEventArgs e)
        {
            selectedCacheElement.SetTranslated(txtTranslatedText.Text);
            selectedCacheElement.IsCorrectedByHuman = true;

            btnSaveToFile.IsEnabled = true;
        }

        private void LbItemsToTranslate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCacheElement = lbItemsToTranslate.SelectedItem as CacheElem;

            txtOriginalText.Text = selectedCacheElement?.OriginalNormalizedText;
            txtTranslatedText.Text = selectedCacheElement?.TranslatedNormalizedText;

            btnTranslate.IsEnabled = selectedCacheElement == null ? false : true;
        }

        private void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            dataCache.SaveElems();
            btnSaveToFile.IsEnabled = false;
        }

        private void CbItemsToTranslateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbItemsToTranslateFilter.SelectedIndex)
            {
                case 0:
                    filtredElements = allElements;
                    break;
                case 1:
                    filtredElements = allElements.Where(c => c.ToTranslate).ToList();
                    break;
            }

            btnTranslate.IsEnabled = false;
            lbItemsToTranslate.ItemsSource = null;
            lbItemsToTranslate.ItemsSource = filtredElements;
        }
    }
}
