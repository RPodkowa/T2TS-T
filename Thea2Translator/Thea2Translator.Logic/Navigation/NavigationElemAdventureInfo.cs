using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thea2Translator.Logic
{
    public class NavigationElemAdventureInfo
    {
        public string FileName { get; private set; }        
        public string AdventureName { get; private set; }
        public string AdventureId { get; private set; }

        public NavigationElemAdventureInfo(string fileName, string adventureName, string adventureId)
        {
            FileName = fileName;
            AdventureName = adventureName;
            AdventureId = adventureId;
        }

        public string GetGroupName(string adventureNodeId)
        {
            return $"{FileName}_{AdventureName}_{adventureNodeId}";
        }

        public override string ToString()
        {
            return $"[{FileName}]: {AdventureName} ({AdventureId})";
        }

        public static bool IsEquals(NavigationElemAdventureInfo elem1, NavigationElemAdventureInfo elem2)
        {
            if (elem1 == null && elem2 == null) return true;

            if (elem1 == null && elem2 != null) return false;
            if (elem1 != null && elem2 == null) return false;
            if (elem1.FileName != elem2.FileName) return false;
            if (elem1.AdventureName != elem2.AdventureName) return false;
            if (elem1.AdventureId != elem2.AdventureId) return false;
            return true;
        }
    }
}
