using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Localization;

namespace Constructor.Modification
{
	class BulletVelocity2 : IModification
	{
		public BulletVelocity2(ModificationQuality quality)
		{
			_velocityMultiplier = quality.PowerMultiplier(0.55f, 0.7f, 0.85f, 1.3f, 1.60f, 1.9f, 2.2f);
			_energyMultiplier = quality.PowerMultiplier(0.8f, 0.85f, 0.9f, 1.15f, 1.3f, 1.45f, 1.6f);
			Quality = quality;
		}

		public string GetDescription(ILocalization localization)
        {
			return localization.GetString(
				"$BulletVelocityMod2",
				Maths.Format.SignedPercent(_velocityMultiplier - 1.0f),
				Maths.Format.SignedPercent(_energyMultiplier - 1.0f)); 
		}

		public ModificationQuality Quality { get; private set; }

        public void Apply(ref ShipEquipmentStats stats) { }
        public void Apply(ref DeviceStats device) { }
        public void Apply(ref DroneBayStats droneBay) { }

        public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition)
        {
            ammunition.Velocity *= _velocityMultiplier;

			if (ammunition.EnergyCost > 0)
				ammunition.EnergyCost *= _energyMultiplier;
		}

	    public void Apply(ref WeaponStatModifier statModifier)
	    {
	        statModifier.VelocityMultiplier *= _velocityMultiplier;
	    }

        private readonly float _velocityMultiplier;
		private readonly float _energyMultiplier;
	}
}
