using Economy.ItemType;
using GameDatabase.Enums;

namespace Model
{
    //public enum ItemQuality
    //{
    //    Low5,
    //    Low4,
    //    Low3,
    //    Low2,
    //    Low1,
    //    Common,
    //    Medium,
    //    High,
    //    Perfect,
    //    Epic,
    //    Legend,
    //}

    public static class QualityTypeExtension
    {
        public static string Name(this ItemQuality type)
        {
            switch (type)
            {
                case ItemQuality.N1:
                    return "$QualityLow4";
                case ItemQuality.N2:
                    return "$QualityLow3";
                case ItemQuality.N3:
                    return "$QualityLow2";
                case ItemQuality.Low:
                    return "$QualityLow1";
                case ItemQuality.Common:
                    return "$QualityCommon";
                case ItemQuality.Medium:
                    return "$QualityMedium";
                case ItemQuality.High:
                    return "$QualityHigh";
                case ItemQuality.Perfect:
                    return "$QualityPerfect";
                case ItemQuality.Extreme:
                    return "$QualityEpic";
                default:
                    return "";
            }
        }
    }
}
