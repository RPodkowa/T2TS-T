using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.CSVReader.Model
{
    public class TextRecord
    {
        public string Id { get; set; }
        public bool IsCorrectedByHuman { get; set; }
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
    }
}
