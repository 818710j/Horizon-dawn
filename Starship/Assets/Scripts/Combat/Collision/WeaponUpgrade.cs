using GameDatabase.Model;
using UnityEngine;

namespace Combat.Collision
{
    public struct WeaponUpgrade
    {
        public float DamageMultiplier;
        public float RangeMultiplier;
        public float FireRateMultiplier;
        public float EnergyCostMultiplier;


        public static readonly WeaponUpgrade Empty = new WeaponUpgrade();
    }
}
