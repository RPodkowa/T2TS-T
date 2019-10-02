using System.Collections.Generic;
using System.Xml;

namespace Thea2Translator.Logic
{
    public class SubraceInfo
    {
        public string Name { get; private set; }
        public RaceInfo Race { get; private set; }
        public Dictionary<GenderType, int> GenderChance { get; private set; }

        private SubraceInfo(XmlNode xmlNode)
        {
            Name = xmlNode.Name;
            InitGenderChances();

            var subraceElems = xmlNode.ChildNodes;

            foreach (XmlNode subraceElem in subraceElems)
            {
                var subraceElemName = subraceElem.Name;
                if (subraceElemName != "CommonSubraceAttributes" && subraceElemName != "Race")
                    continue;

                if (subraceElem.Attributes == null)
                    continue;

                if (subraceElemName == "Race")
                {
                    Race = new RaceInfo(subraceElem.Attributes["Value"]?.Value);
                    continue;
                }

                var countedTags = subraceElem.ChildNodes;

                foreach (XmlNode countedTag in countedTags)
                {
                    if (countedTag.Attributes == null)
                        continue;

                    var tag = countedTag.Attributes["Tag"]?.Value;
                    if (tag.StartsWith("TAG-GENDER_"))
                    {
                        SetGenderChances(tag, int.Parse(subraceElem.Attributes["Chance"]?.Value));
                        break;
                    }
                }
            }
        }

        private void InitGenderChances()
        {
            GenderChance = new Dictionary<GenderType, int>();
            GenderChance.Add(GenderType.MALE, 100);
        }

        private void SetGenderChances(string tag, int chance)
        {
            GenderChance = new Dictionary<GenderType, int>();

            //RACE-BEAST:SUBRACE-PET_BOAR_STRONG_WILD:100:TAG-GENDER_OTHER
            //RACE-DEMON:SUBRACE-CRATE_ALKONOST:100:TAG-GENDER_FEMALE
            //RACE-DEMON:SUBRACE-DZIEVANNAS_FERAL:50:TAG-GENDER_FEMALE

            var genderType = GenderInfo.GetTypeFromTag(tag);
            GenderChance.Add(genderType, chance);

            if (chance != 100)
            {
                genderType = GenderInfo.GetOppositeType(genderType);
                chance = 100 - chance;
                GenderChance.Add(genderType, chance);
            }
        }

        public bool UseAllCharacters()
        {
            return UseCharacterMale() && UseCharacterFemale();
        }

        public bool UseCharacterMale()
        {
            if (GenderChance.ContainsKey(GenderType.MALE)) return true;
            if (GenderChance.ContainsKey(GenderType.OTHER)) return true;
            return false;
        }

        public bool UseCharacterFemale()
        {
            return GenderChance.ContainsKey(GenderType.FEMALE);
        }

        public static SubraceInfo ReadFromXml(XmlNode xmlNode)
        {
            if (xmlNode.NodeType == XmlNodeType.Comment)
                return null;

            return new SubraceInfo(xmlNode);
        }
        
        public bool HasRace(List<RaceType> raceTypes)
        {
            foreach (var raceType in raceTypes)
            {
                if (HasRace(raceType))
                    return true;
            }

            return false;
        }

        public bool HasRace(RaceType raceType)
        {
            return (Race.Race == raceType);
        }
    }
}
