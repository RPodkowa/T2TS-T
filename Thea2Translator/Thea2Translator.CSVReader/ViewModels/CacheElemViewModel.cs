using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Thea2Translator.Logic;

namespace Thea2Translator.DesktopApp.ViewModels
{
    internal class CacheElemViewModel
    {
        public CacheElem CacheElem { get; set; }
        public bool IsConfirm { get; set; }
        public bool HasConflict { get; set; }
        public bool IsActive { get; set; }
        public string Color { get { return GetColor().ToString(); } }
        public string BackgroundColor { get { return GetBackgroundColor().ToString(); } }

        
        private Color GetColor()
        {
            if (CacheElem.IsInactive) return Colors.Black;
            if (CacheElem.HasConflict) return Colors.DarkRed;
            if (CacheElem.IsGenericName) return Colors.DarkBlue;
            if (CacheElem.ToTranslate) return Colors.Red;
            if (CacheElem.IsCorrectedByHuman) return Colors.DarkGreen;

            return Colors.Black;
        }
        private Color GetBackgroundColor()
        {
            if (CacheElem.IsInactive) return System.Windows.Media.Color.FromArgb(64, 255, 0, 0);

            return System.Windows.Media.Color.FromArgb(0, 255, 255, 255);
        }

        public string Weight
        {
            get
            {
                if (CacheElem.IsInactive) return "Bold";
                if (CacheElem.IsAdventureNodeRecord) return "Bold";
                return "Normal";
            }
        }

        public CacheElemViewModel(CacheElem CacheElem)
        {
            this.CacheElem = CacheElem;
            IsConfirm = !CacheElem.ToConfirm;
            HasConflict = CacheElem.HasConflict;
            IsActive = CacheElem.IsActive;
        }
    }
}
