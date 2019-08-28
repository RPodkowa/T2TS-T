using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Filter
{
    public class FilterModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Author { get; set; }
    }
}
