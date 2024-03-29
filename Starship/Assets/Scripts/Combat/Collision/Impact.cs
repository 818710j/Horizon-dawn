using System;
using Combat.Component.Body;
using Constructor.Modification;
using GameDatabase.Enums;
using UnityEngine;

namespace Combat.Collision
{
    public class Impulse
    {
        public Impulse()
        {
            _values = new Vector2[8];
            _count = 0;
        }

        public void Apply(IBody body)
        {
            for (var i = 0; i < _count; ++i)
                body.ApplyForce(_values[i*2], _values[i*2 + 1]);
        }

        public void Append(Vector2 position, Vector2 impulse)
        {
            if (_count + 2 >= _values.Length)
                Array.Resize(ref _values, _count + 2);

            _values[_count++] = position;
            _values[_count++] = impulse;
        }

        public Impulse Append(Impulse other)
        {
            if (other == null || other._count == 0)
                return this;

            if (_count + other._count >= _values.Length)
                Array.Resize(ref _values, _count + other._count);

            Array.Copy(other._values, 0, _values, _count, other._count);
            _count += other._count;

            return this;
        }

        public void Clear()
        {
            _count = 0;
        }

        private int _count;
        private Vector2[] _values;
    }

    public struct Impact
    {
        public float KineticDamage;
        public float EnergyDamage;
        public float HeatDamage;
        public float DirectDamage;

        public float StructureDamage;
        public float Repair;
        public float ShieldDamage;
        public float EnergyDrain;
        public Impulse Impulse;
        public CollisionEffect Effects;

        public float GetTotalDamage(Resistance resistance)
        {
            //var minResistance = Mathf.Min(Mathf.Min(resistance.Kinetic, resistance.Energy), resistance.Heat);
            var damage1 =
                KineticDamage * (1f - resistance.Kinetic) +
                EnergyDamage * (1f - resistance.Energy) +
                HeatDamage * (1f - resistance.Heat);

            var damage3 = DirectDamage * (1f - 0.5f * resistance.Structure);

            var damage2 = StructureDamage * (1f - resistance.Structure);

            var r_damage1 = damage1 + damage2 * 2 + damage3;
            var r_damage2 = damage2 + (damage1 * 0.15f + damage3 * 0.35f) * (1f - resistance.Structure);
            return r_damage1 + r_damage2;
        }
        public float GetArmorTotalDamage(Resistance resistance)
        {
            //var minResistance = Mathf.Min(Mathf.Min(resistance.Kinetic, resistance.Energy), resistance.Heat);
            var damage1 =
                KineticDamage * (1f - resistance.Kinetic) +
                EnergyDamage * (1f - resistance.Energy) +
                HeatDamage * (1f - resistance.Heat);

            var damage3 = DirectDamage * (1f - 0.5f * resistance.Structure);

            var damage2 = StructureDamage * (1f - resistance.Structure);

            var r_damage = damage1 + damage2 * 2 + damage3;
            Debug.Log("GetArmorTotalDamage:  Damage:  " + (damage1 + damage3) + "  -  StructureDamage:  " + StructureDamage);
            return r_damage;
        }
        public float GetStructureTotalDamage(Resistance resistance, float ex, bool r)
        {
            //var minResistance = Mathf.Min(Mathf.Min(resistance.Kinetic, resistance.Energy), resistance.Heat);
            var damage1 =
                KineticDamage * (1f - resistance.Kinetic) +
                EnergyDamage * (1f - resistance.Energy) +
                HeatDamage * (1f - resistance.Heat);

            var damage3 = DirectDamage * (1f - 0.5f * resistance.Structure);

            var damage2 = StructureDamage * (1f - resistance.Structure);

            var ex_damage = ex == 0 && !r ? (damage1 * 0.15f + damage3 * 0.35f) * (1f - resistance.Structure) : ex;
            var r_damage = damage2 + ex_damage;
            Debug.Log("GetStructureTotalDamage:  StructureDamage:  " + damage2 + "  -  ExDamage:  " + ex_damage + "  :  " + (ex == 0 && !r ? damage1 + "  -  " + damage3 : ex));
            return r_damage;
        }

        public float GetEnergyShieldDamage()
        {
            return KineticDamage + EnergyDamage + HeatDamage + DirectDamage + StructureDamage;
        }
        public void AddDamage(DamageType type, float amount)
        {
            if (amount < 0)
                throw new InvalidOperationException();

            if (type == DamageType.Direct)
                DirectDamage += amount;
            else if (type == DamageType.Impact)
                KineticDamage += amount;
            else if (type == DamageType.Energy)
                EnergyDamage += amount;
            else if (type == DamageType.Heat)
                HeatDamage += amount;
            else if (type == DamageType.Structure)
                StructureDamage += amount;
            else
                throw new System.ArgumentException("unknown damage type");
        }

        public void AddImpulse(Vector2 position, Vector2 impulse)
        {
            if (Impulse == null)
                Impulse = new Impulse();

            Impulse.Append(position, impulse);
        }

        public void ApplyImpulse(IBody body)
        {
            if (Impulse != null)
                Impulse.Apply(body);
        }

        public void RemoveImpulse()
        {
            if (Impulse != null)
                Impulse.Clear();
        }

        public Impact GetDamage(Resistance resistance)
        {
            return new Impact
            {
                KineticDamage = this.KineticDamage * (1f - resistance.Kinetic),
                EnergyDamage = this.EnergyDamage * (1f - resistance.Energy),
                HeatDamage = this.HeatDamage * (1f - resistance.Heat),
                DirectDamage = this.DirectDamage * (1f - 0.5f * resistance.Structure),
                StructureDamage=this.StructureDamage*(1f-resistance.Structure),
                ShieldDamage = this.ShieldDamage,
                EnergyDrain = this.EnergyDrain,
                Impulse = this.Impulse,
                Repair = this.Repair,
                Effects = this.Effects
            };
        }

        public void ApplyShield(float power)
        {
            var damage = KineticDamage + EnergyDamage + HeatDamage + DirectDamage+StructureDamage;

            if (damage <= 0 || power <= 0)
                return;

            if (damage <= power)
            {
                RemoveDamage();
                ShieldDamage += damage;
            }
            else
            {
                KineticDamage -= power * KineticDamage / damage;
                EnergyDamage -= power * EnergyDamage / damage;
                HeatDamage -= power * HeatDamage / damage;
                DirectDamage -= power * DirectDamage / damage;
                StructureDamage -= power * StructureDamage / damage;
                ShieldDamage += power;
            }
        }

        public void RemoveDamage(float amount)
        {
            var total = GetEnergyShieldDamage();
            if (total <= amount || total <= 0.000001f)
            {
                RemoveDamage();
                return;
            }

            KineticDamage -= amount * KineticDamage / total;
            EnergyDamage -= amount * EnergyDamage / total;
            HeatDamage -= amount * HeatDamage / total;
            DirectDamage -= amount * DirectDamage / total;
            StructureDamage -= amount * StructureDamage / total;
        }

        public void RemoveDamage()
        {
            KineticDamage = 0;
            EnergyDamage = 0;
            HeatDamage = 0;
            DirectDamage = 0;
            StructureDamage = 0;
        }

        public bool ApplyStructureShield(float power,float amount,Resistance resistance,out float energycost,out float extrastructuredamage)
        {
            var _r = false;
            var damage1 =
                KineticDamage * (1f - resistance.Kinetic) +
                EnergyDamage * (1f - resistance.Energy) +
                HeatDamage * (1f - resistance.Heat);

            var damage3 = DirectDamage * (1f - 0.5f * resistance.Structure);

            var damage2 = StructureDamage;

            var e_damage = damage1 * 0.15f + damage3 * 0.35f;
            var r_damage2 = damage2 + e_damage;



            var damage = r_damage2 / power;
            if(damage>amount)
            {
                StructureDamage -= amount * StructureDamage / damage;
                energycost = amount;
                extrastructuredamage = e_damage - amount * e_damage / damage;
                _r = false;
            }
            else
            {
                StructureDamage = 0;
                energycost = damage;
                extrastructuredamage = 0;
                _r = true;
            }
            Debug.Log("ApplyStructureShield:  energycost:  " + energycost + "  -  extrastructuredamage:  "+extrastructuredamage);

            return _r;
        }
        public void Append(Impact second)
        {
            KineticDamage += second.KineticDamage;
            EnergyDamage += second.EnergyDamage;
            HeatDamage += second.HeatDamage;
            DirectDamage += second.DirectDamage;
            ShieldDamage += second.ShieldDamage;
            Repair += second.Repair;
            Effects |= second.Effects;
            Impulse = Impulse == null ? second.Impulse : Impulse.Append(second.Impulse);
        }
    }
}
