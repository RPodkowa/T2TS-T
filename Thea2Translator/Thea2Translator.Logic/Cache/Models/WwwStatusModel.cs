﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic.Cache
{
    public class Module
    {
        public string name { get; set; }
        public string allRecords { get; set; }
        public string translatedByGoogle { get; set; }
        public string translatedByGooglePercent { get; set; }
        public string correctedRecords { get; set; }
        public string correctedPercent { get; set; }
    }

    public class WwwStatusModel
    {
        public string modifiedDate { get; set; }
        public List<Module> modules { get; set; }
    }
}
