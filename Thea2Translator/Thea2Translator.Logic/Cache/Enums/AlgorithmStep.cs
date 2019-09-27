﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public enum AlgorithmStep
    {
        ImportFromSteam,
        PrepareToMachineTranslate,
        ImportFromMachineTranslate,
        ExportToSteam,
        ExportToSteamDebug
    }
}
