namespace Thea2Translator.Logic
{
    public static class GenderInfo
    {
        public static GenderType GetTypeFromTag(string tag)
        {
            tag = tag.Replace("TAG-GENDER_", "");
            GenderType type;
            if (RaceType.TryParse(tag, out type))
                return type;

            return GenderType.OTHER;
        }

        public static GenderType GetOppositeType(GenderType type)
        {
            if (type == GenderType.FEMALE) return GenderType.MALE;
            if (type == GenderType.MALE) return GenderType.FEMALE;
            return GenderType.OTHER;
        }
    }
}
