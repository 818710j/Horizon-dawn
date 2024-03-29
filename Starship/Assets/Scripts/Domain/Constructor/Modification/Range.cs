using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Localization;

namespace Constructor.Modification
{
    class Range : IModification
    {
        public Range(ModificationQuality quality)
        {
            _multiplier = quality.PowerMultiplier(0.55f, 0.7f, 0.85f, 1.15f, 1.30f, 1.45f, 1.60f);
            Quality = quality;
        }

		public string GetDescription(ILocalization localization) { return localization.GetString("$RangeMod", Maths.Format.SignedPercent(_multiplier - 1.0f)); }

        public ModificationQuality Quality { get; private set; }

        public void Apply(ref ShipEquipmentStats stats) { }

        public void Apply(ref DeviceStats device)
        {
            device.Range *= _multiplier;
        }

        public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition)
        {
            ammunition.Range *= _multiplier;
        }

        public void Apply(ref WeaponStatModifier statModifier)
        {
            statModifier.RangeMultiplier *= _multiplier;
        }

        public void Apply(ref DroneBayStats droneBay) { }

        private readonly float _multiplier;
    }
}
