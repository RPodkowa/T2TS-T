namespace Thea2Translator.Logic
{
    public class RaceInfo
    {
        public RaceType Race { get; private set; }

        public RaceInfo(string name)
        {
            name = name.Replace("RACE-", "");
            RaceType race;
            if (RaceType.TryParse(name, out race))
                Race = race;
        }
    }
}
