using Combat.Collision;
using Combat.Component.Mods;
using Combat.Unit.HitPoints;
using Constructor;
using System.Linq;
using UnityEngine;

namespace Combat.Component.Stats
{
    public class ShipStats : IStats
    {
        public ShipStats(IShipSpecification spec)
        {
            var stats = spec.Stats;

            _resistance = new Resistance
            {
                Energy = stats.EnergyResistancePercentage,
                EnergyDrain = stats.EnergyAbsorptionPercentage,
                Heat = stats.ThermalResistancePercentage,
                Kinetic = stats.KineticResistancePercentage,
                Structure = stats.StructureResistancePercentage
            };

            _weaponupgrade = new WeaponUpgrade
            {
                DamageMultiplier = 1,
                RangeMultiplier = 1,
                FireRateMultiplier = 1,
                EnergyCostMultiplier = 1,

            };

            WeaponDamageMultiplier = stats.DamageMultiplier.Value;
            RammingDamageMultiplier = stats.RammingDamageMultiplier;
            HitPointsMultiplier = stats.ArmorMultiplier.Value;

            if (stats.ArmorPoints < 0.1f)
                _armorPoints = new EmptyResources();
            else if (stats.ArmorRepairRate > 0)
                _armorPoints = new Energy(stats.ArmorPoints, stats.ArmorRepairRate, stats.ArmorRepairCooldown);
            else
                _armorPoints = new HitPoints(stats.ArmorPoints);

            _shieldPoints = new Energy(stats.ShieldPoints, stats.ShieldRechargeRate, stats.ShieldRechargeCooldown);
            _energyPoints = new Energy(stats.EnergyPoints, stats.EnergyRechargeRate, stats.EnergyRechargeCooldown);

            _structurePoints = new Energy(stats.StructurePoints, stats.StructureRepairRate, stats.StructureRepairCooldown);
            /*
            if(spec.Devices.Any(item => item.Device.DeviceClass == GameDatabase.Enums.DeviceClass.StructureShield))
            {
                var device = spec.Devices.First(item => item.Device.DeviceClass == GameDatabase.Enums.DeviceClass.StructureShield);
                StructureShield = device.Device.Power * HitPointsMultiplier;
            }
            else
            {
                StructureShield = 0;
            }
            Debug.Log("StructureShield Power:  " + StructureShield);
            */
        }

        public bool IsAlive { get { return _armorPoints.Value > 0 && _structurePoints.Value>0; } }
        public float StructureShield
        {
            get
            {
                return _structureShield;
            }
            set
            {
                _structureShield = value;
                Debug.Log("StructureShield Power:  " + _structureShield);
            }
        }

        public IResourcePoints Armor { get { return _armorPoints; } }
        public IResourcePoints Shield { get { return _shieldPoints; } }
        public IResourcePoints Energy { get { return _energyPoints; } }
        public IResourcePoints Structure { get { return _structurePoints; } }

        public float WeaponDamageMultiplier { get; private set; }
        public float RammingDamageMultiplier { get; private set; }
        public float HitPointsMultiplier { get; private set; }

        public Resistance Resistance
        {
            get
            {
                var resistance = _resistance;
                _modifications.Apply(ref resistance);
                return resistance;
            }
        }
        public WeaponUpgrade WeaponUpgrade
        {
            get
            {
                var weaponupgrade = _weaponupgrade;
                _weaponmodifications.Apply(ref weaponupgrade);
                return weaponupgrade;
            }
        }
        public Modifications<WeaponUpgrade> WeaponModifications { get { return _weaponmodifications; } }
        public Modifications<Resistance> Modifications { get { return _modifications; } }

        public IDamageIndicator DamageIndicator { get; set; }

        public float TimeFromLastHit { get; private set; }

        public void ApplyDamage(Impact impact)
        {
            if (Shield.Exists)
                impact.ApplyShield(Shield.Value - impact.ShieldDamage);
            else
                impact.ShieldDamage = 0;

            var resistance = Resistance;

            if (DamageIndicator != null)
                DamageIndicator.ApplyDamage(impact.GetDamage(resistance));


            var damage = impact.GetTotalDamage(resistance);//
            if (damage > 0.1f)
                TimeFromLastHit = 0;

            if (resistance.EnergyDrain > 0.01f)
            {
                var energy = resistance.EnergyDrain * impact.EnergyDamage / HitPointsMultiplier;
                Energy.Get(-energy);
            }

            Energy.Get(impact.EnergyDrain);

            float ex = 0;
            var r = false;
            if (StructureShield > 0)
            {
                r=impact.ApplyStructureShield(StructureShield, Energy.Value, resistance, out var energycost, out ex);
                Energy.Get(energycost);
            }
            damage = impact.GetArmorTotalDamage(resistance);

            damage -= impact.Repair;
            Armor.Get(damage);

            Shield.Get(impact.ShieldDamage);

            damage = impact.GetStructureTotalDamage(resistance, ex,r);
            Structure.Get(damage);


            if (impact.Effects.Contains(CollisionEffect.Destroy))
            {
                Armor.Get(Armor.MaxValue);
                Structure.Get(Structure.MaxValue);
            }
        }

        public void UpdatePhysics(float elapsedTime)
        {
            if (!IsAlive)
                return;

            if (DamageIndicator != null)
                DamageIndicator.Update(elapsedTime);

            _energyPoints.Update(elapsedTime);
            _armorPoints.Update(elapsedTime);
            _shieldPoints.Update(elapsedTime);
            _structurePoints.Update(elapsedTime);

            TimeFromLastHit += elapsedTime;
        }

        public void Dispose()
        {
            if (DamageIndicator != null)
                DamageIndicator.Dispose();
        }

        private readonly IResourcePoints _armorPoints;
        private readonly IResourcePoints _shieldPoints;
        private readonly IResourcePoints _energyPoints;
        private readonly IResourcePoints _structurePoints;
        private readonly Resistance _resistance;
        private readonly WeaponUpgrade _weaponupgrade;
        private readonly Modifications<Resistance> _modifications = new Modifications<Resistance>();
        private readonly Modifications<WeaponUpgrade> _weaponmodifications = new Modifications<WeaponUpgrade>();

        private float _structureShield = 0;
    }
}
