using System;
using Combat.Collision;
using Combat.Component.Mods;
using Combat.Unit.HitPoints;

namespace Combat.Component.Stats
{
    public interface IStats : IDisposable
    {
        bool IsAlive { get; }
        float StructureShield { get; set; }

        IResourcePoints Armor { get; }
        IResourcePoints Shield { get; }
        IResourcePoints Energy { get; }
        IResourcePoints Structure { get; }

        float WeaponDamageMultiplier { get; }
        float RammingDamageMultiplier { get; }
        float HitPointsMultiplier { get; }

        Resistance Resistance { get; }
        WeaponUpgrade WeaponUpgrade { get; }
        Modifications<Resistance> Modifications { get; }
        Modifications<WeaponUpgrade> WeaponModifications { get; }

        float TimeFromLastHit { get; }

        void ApplyDamage(Impact damage);
        void UpdatePhysics(float elapsedTime);
    }
}
