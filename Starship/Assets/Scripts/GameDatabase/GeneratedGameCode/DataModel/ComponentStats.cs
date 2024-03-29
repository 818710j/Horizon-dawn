//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using System.Linq;
using GameDatabase.Enums;
using GameDatabase.Serializable;
using GameDatabase.Model;

namespace GameDatabase.DataModel
{
	public partial class ComponentStats
	{
		partial void OnDataDeserialized(ComponentStatsSerializable serializable, Database.Loader loader);

		public static ComponentStats Create(ComponentStatsSerializable serializable, Database.Loader loader)
		{
			return new ComponentStats(serializable, loader);
		}

		private ComponentStats(ComponentStatsSerializable serializable, Database.Loader loader)
		{
			Id = new ItemId<ComponentStats>(serializable.Id);
			loader.AddComponentStats(serializable.Id, this);

			Type = serializable.Type;
			ArmorPoints = UnityEngine.Mathf.Clamp(serializable.ArmorPoints, -1000000f, 1000000f);
			ArmorRepairRate = UnityEngine.Mathf.Clamp(serializable.ArmorRepairRate, -1000000f, 1000000f);
			ArmorRepairCooldownModifier = UnityEngine.Mathf.Clamp(serializable.ArmorRepairCooldownModifier, -1f, 1f);
            StructurePoints = UnityEngine.Mathf.Clamp(serializable.StructurePoints, -1000000f, 1000000f);
            StructureRepairRate = UnityEngine.Mathf.Clamp(serializable.StructureRepairRate, -1000000f, 1000000f);
            StructureRepairCooldownModifier = UnityEngine.Mathf.Clamp(serializable.StructureRepairCooldownModifier, -5f, 5f);
			EnergyPoints = UnityEngine.Mathf.Clamp(serializable.EnergyPoints, -1000000f, 1000000f);
			EnergyRechargeRate = UnityEngine.Mathf.Clamp(serializable.EnergyRechargeRate, -1000000f, 1000000f);
			EnergyRechargeCooldownModifier = UnityEngine.Mathf.Clamp(serializable.EnergyRechargeCooldownModifier, -5f, 5f);
			ShieldPoints = UnityEngine.Mathf.Clamp(serializable.ShieldPoints, -1000000f, 1000000f);
			ShieldRechargeRate = UnityEngine.Mathf.Clamp(serializable.ShieldRechargeRate, -1000000f, 1000000f);
			ShieldRechargeCooldownModifier = UnityEngine.Mathf.Clamp(serializable.ShieldRechargeCooldownModifier, -5f, 5f);
			Weight = UnityEngine.Mathf.Clamp(serializable.Weight, -1000000f, 1000000f);
			RammingDamage = UnityEngine.Mathf.Clamp(serializable.RammingDamage, -1000000f, 1000000f);
			EnergyAbsorption = UnityEngine.Mathf.Clamp(serializable.EnergyAbsorption, -1000000f, 1000000f);
			KineticResistance = UnityEngine.Mathf.Clamp(serializable.KineticResistance, -1000000f, 1000000f);
			EnergyResistance = UnityEngine.Mathf.Clamp(serializable.EnergyResistance, -1000000f, 1000000f);
			ThermalResistance = UnityEngine.Mathf.Clamp(serializable.ThermalResistance, -1000000f, 1000000f);
            StructureResistance = UnityEngine.Mathf.Clamp(serializable.StructureResistance, -1000000f, 1000000f);
			EnginePower = UnityEngine.Mathf.Clamp(serializable.EnginePower, -2000f, 2000f);
			TurnRate = UnityEngine.Mathf.Clamp(serializable.TurnRate, -2000f, 2000f);
			Autopilot = serializable.Autopilot;
			DroneRangeModifier = UnityEngine.Mathf.Clamp(serializable.DroneRangeModifier, -500f, 500f);
			DroneDamageModifier = UnityEngine.Mathf.Clamp(serializable.DroneDamageModifier, -500f, 500f);
			DroneDefenseModifier = UnityEngine.Mathf.Clamp(serializable.DroneDefenseModifier, -500f, 500f);
			DroneSpeedModifier = UnityEngine.Mathf.Clamp(serializable.DroneSpeedModifier, -500f, 500f);
			DronesBuiltPerSecond = UnityEngine.Mathf.Clamp(serializable.DronesBuiltPerSecond, 0f, 1000f);
			DroneBuildTimeModifier = UnityEngine.Mathf.Clamp(serializable.DroneBuildTimeModifier, 0f, 1000f);
			WeaponFireRateModifier = UnityEngine.Mathf.Clamp(serializable.WeaponFireRateModifier, -1000f, 1000f);
			WeaponDamageModifier = UnityEngine.Mathf.Clamp(serializable.WeaponDamageModifier, -1000f, 1000f);
			WeaponRangeModifier = UnityEngine.Mathf.Clamp(serializable.WeaponRangeModifier, -1000f, 1000f);
			WeaponEnergyCostModifier = UnityEngine.Mathf.Clamp(serializable.WeaponEnergyCostModifier, -1000f, 1000f);
			AlterWeaponPlatform = serializable.AlterWeaponPlatform;

			OnDataDeserialized(serializable, loader);
		}

		public readonly ItemId<ComponentStats> Id;

		public ComponentStatsType Type { get; private set; }
		public float ArmorPoints { get; private set; }
		public float ArmorRepairRate { get; private set; }
		public float ArmorRepairCooldownModifier { get; private set; }
		public float StructurePoints { get; private set; }
		public float StructureRepairRate { get; private set; }
		public float StructureRepairCooldownModifier { get; private set; }
		public float EnergyPoints { get; private set; }
		public float EnergyRechargeRate { get; private set; }
		public float EnergyRechargeCooldownModifier { get; private set; }
		public float ShieldPoints { get; private set; }
		public float ShieldRechargeRate { get; private set; }
		public float ShieldRechargeCooldownModifier { get; private set; }
		public float Weight { get; private set; }
		public float RammingDamage { get; private set; }
		public float EnergyAbsorption { get; private set; }
		public float KineticResistance { get; private set; }
		public float EnergyResistance { get; private set; }
		public float ThermalResistance { get; private set; }
		public float StructureResistance { get; private set; }
		public float EnginePower { get; private set; }
		public float TurnRate { get; private set; }
		public bool Autopilot { get; private set; }
		public float DroneRangeModifier { get; private set; }
		public float DroneDamageModifier { get; private set; }
		public float DroneDefenseModifier { get; private set; }
		public float DroneSpeedModifier { get; private set; }
		public float DronesBuiltPerSecond { get; private set; }
		public float DroneBuildTimeModifier { get; private set; }
		public float WeaponFireRateModifier { get; private set; }
		public float WeaponDamageModifier { get; private set; }
		public float WeaponRangeModifier { get; private set; }
		public float WeaponEnergyCostModifier { get; private set; }
		public PlatformType AlterWeaponPlatform { get; private set; }

		public static ComponentStats DefaultValue { get; private set; }
	}
}
