using Constructor.Model;
using GameDatabase.DataModel;
using GameDatabase.Enums;
using Services.Localization;
using UnityEngine;

namespace Constructor.Modification
{
    class Fortified2 : IModification
    {
        public Fortified2(ModificationQuality quality)
        {
            _defenseMultiplier = quality.PowerMultiplier(0.55f, 0.7f, 0.85f, 1.5f, 2f, 2.5f, 3f);
            _weightMultiplier = quality.PowerMultiplier(1.35f, 1.25f, 1.15f, 1.25f, 1.5f, 1.75f, 2f);
            Quality = quality;
        }

		public string GetDescription(ILocalization localization) 
        { 
            return localization.GetString(
               "$Fortified2",
                  Maths.Format.SignedPercent(_defenseMultiplier - 1.0f),
                  Maths.Format.SignedPercent(_weightMultiplier - 1.0f));
        }

        public ModificationQuality Quality { get; private set; }

        public void Apply(ref ShipEquipmentStats stats)
        {
            if (stats.ArmorPoints > 0)
                stats.ArmorPoints *= _defenseMultiplier;
            if (stats.StructurePoints > 0)
                stats.StructurePoints *= _defenseMultiplier;
            if (stats.EnergyResistance > 0)
                stats.EnergyResistance *= _defenseMultiplier;
            if (stats.ThermalResistance > 0)
                stats.ThermalResistance *= _defenseMultiplier;
            if (stats.KineticResistance > 0)
                stats.KineticResistance *= _defenseMultiplier;
            if (stats.StructureResistance > 0)
                stats.StructureResistance *= _defenseMultiplier;
            if (stats.ShieldPoints > 0)
                stats.ShieldPoints *= _defenseMultiplier;
            if (stats.Weight > 0)
                stats.Weight *= _weightMultiplier;
        }

        public void Apply(ref DeviceStats device) { }
        public void Apply(ref WeaponStats weapon, ref AmmunitionObsoleteStats ammunition) { }
        public void Apply(ref DroneBayStats droneBay) { }
        public void Apply(ref WeaponStatModifier statModifier) { }

        private readonly float _defenseMultiplier;
        private readonly float _weightMultiplier;
    }
}
