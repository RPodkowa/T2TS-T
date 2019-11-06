using System;

namespace Thea2Translator.Logic
{
    public class RaceInfo : IComparable<RaceInfo>
    {
        public RaceType Race { get; private set; }

        static public RaceInfo GetEmptyRaceInfo()
        {
            return new RaceInfo();
        }

        private RaceInfo()
        {
            Race = RaceType.NONE;
        }

        public RaceInfo(string name)
        {
            name = name.Replace("RACE-", "");
            RaceType race;
            if (RaceType.TryParse(name, out race))
                Race = race;
        }

        public string GetFullName()
        {
            return $"RACE-{Race}";
        }

        public string GetName(bool translated)
        {
            string ret = Race.ToString();
            if (translated)
            {
                ret = ret.Replace("HUMAN", "Człowiek");
                ret = ret.Replace("ELF", "Elf");
                ret = ret.Replace("ORC", "Ork");
                ret = ret.Replace("GOBLIN", "Goblin");
                ret = ret.Replace("DWARF", "Krasnolud");
                ret = ret.Replace("DEMON", "Demon");
                ret = ret.Replace("UNLIVING", "Nieumarły");
                ret = ret.Replace("BEAST", "Bestia");
                ret = ret.Replace("MYTHICAL", "Mityczny");
                ret = ret.Replace("CONCEPT", "Koncept");
            }

            return ret;
        }

        public int CompareTo(RaceInfo other)
        {
            if (Race == other.Race)
                return 0;

            if (Race == RaceType.NONE) return 1;
            if (other.Race == RaceType.NONE) return -1;
            
            if (Race > other.Race) return 1;
            return -1;
        }
    }
}
