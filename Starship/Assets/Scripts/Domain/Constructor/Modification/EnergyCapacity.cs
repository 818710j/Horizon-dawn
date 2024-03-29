using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Localization;

namespace Constructor.Modification
{
	class EnergyCapacity : IModification
	{
		public EnergyCapacity(ModificationQuality quality)
		{
			_multiplier = quality.PowerMultiplier(0.55f, 0.7f, 0.85f, 1.25f, 1.5f, 1.75f, 2f);
			Quality = quality;
		}

		public string GetDescription(ILocalization localization) { return localization.GetString("$EnergyMod", Maths.Format.SignedPercent(_multiplier - 1.0f)); }

		public ModificationQuality Quality { get; private set; }

		public void Apply(ref ShipEquipmentStats stats)
		{
			if (stats.EnergyPoints > 0)
				stats.EnergyPoints *= _multiplier;
		}

        public void Apply(ref DeviceStats device) { }
        public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition) { }
        public void Apply(ref DroneBayStats droneBay) { }
	    public void Apply(ref WeaponStatModifier statModifier) { }

        private readonly float _multiplier;
	}
}
