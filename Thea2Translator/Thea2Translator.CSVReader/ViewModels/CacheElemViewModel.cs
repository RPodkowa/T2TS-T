﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Thea2Translator.Logic.Cache;

namespace Thea2Translator.DesktopApp.ViewModels
{
    internal class CacheElemViewModel
    {
        public CacheElem CacheElem { get; set; }

        public string Color
        {
            get
            {
                return CacheElem.ToTranslate ? "Black" : Colors.DarkGreen.ToString();
            }
        }

        public CacheElemViewModel(CacheElem CacheElem)
        {
            this.CacheElem = CacheElem;
        }


    }
}
