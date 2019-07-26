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

        public string Color
        {
            get
            {
                if (HasConflict) return Colors.DarkRed.ToString();

                return CacheElem.ToConfirm ? 
                    (CacheElem.ToTranslate? Colors.Red.ToString()
                    :Colors.Black.ToString())
                    : Colors.DarkGreen.ToString();
            }
        }

        public string Weight
        {
            get
            {
                if (CacheElem.IsAdventureNodeRecord)
                    return "Bold";

                return "Normal";
            }
        }

        public CacheElemViewModel(CacheElem CacheElem)
        {
            this.CacheElem = CacheElem;
            IsConfirm = !CacheElem.ToConfirm;
            HasConflict = CacheElem.HasConflict;
        }


    }
}
