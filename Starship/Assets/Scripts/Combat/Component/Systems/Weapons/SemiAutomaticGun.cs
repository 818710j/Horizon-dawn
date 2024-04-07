using Combat.Component.Bullet;
using Combat.Component.Platform;
using Combat.Component.Triggers;
using GameDatabase.DataModel;
using UnityEngine;
using Combat.Component.Ship;

namespace Combat.Component.Systems.Weapons
{
    public class SemiAutomaticGun : SystemBase, IWeapon
    {
        public SemiAutomaticGun(IWeaponPlatform platform, WeaponStats weaponStats, Factory.IBulletFactory bulletFactory, int keyBinding,IShip ship)
            : base(keyBinding, weaponStats.ControlButtonIcon,ship)
        {
            MaxCooldown = weaponStats.FireRate >= 0 ? 1f / weaponStats.FireRate : 999999999;
            _cooldown = MaxCooldown;

            _bulletFactory = bulletFactory;
            _platform = platform;
            _energyConsumption = bulletFactory.Stats.EnergyCost;
            _spread = weaponStats.Spread;
            _magazine = weaponStats.Magazine;
            _ship = ship;
            Info = new WeaponInfo(WeaponType.Common, _spread, bulletFactory, platform);
        }

        public override float ActivationCost { get { return _energyConsumption * _ship.Stats.WeaponUpgrade.EnergyCostMultiplier; } }
        public override bool CanBeActivated { get { return base.CanBeActivated && _platform.EnergyPoints.Value > _energyConsumption * _ship.Stats.WeaponUpgrade.EnergyCostMultiplier; } }
        public override float Cooldown { get { return Mathf.Max(_platform.Cooldown / Mathf.Max(0.0000001f, _ship.Stats.WeaponUpgrade.FireRateMultiplier), base.Cooldown); } }

        public WeaponInfo Info { get; private set; }
        public IWeaponPlatform Platform { get { return _platform; } }
        public float PowerLevel { get { return 1.0f; } }
        public IBullet ActiveBullet { get { return null; } }


        protected override void OnUpdateView(float elapsedTime) { }

        protected override void OnUpdatePhysics(float elapsedTime)
        {
            MaxCooldown = _cooldown / Mathf.Max(0.0000001f, _ship.Stats.WeaponUpgrade.FireRateMultiplier);
            if (Active && _shots < _magazine && CanBeActivated)
            {
                if (_platform.IsReady && _platform.EnergyPoints.TryGet(_energyConsumption * _ship.Stats.WeaponUpgrade.EnergyCostMultiplier)&& _lastActive != Active)
                {
                    Shot();
                    _shots++;
                    InvokeTriggers(ConditionType.OnActivate);
                }
            }
            else if (_shots >= _magazine)
            {
                _shots = 0;
                TimeFromLastUse = 0;
            }
            _lastActive = Active;
        }

        protected override void OnDispose() {}

        private void Shot()
        {
            _platform.Aim(Info.BulletSpeed, Info.Range, Info.IsRelativeVelocity);
            _platform.OnShot();
            _bulletFactory.Create(_platform, _spread, 0, 0, Vector2.zero);
        }

        private bool _lastActive;
        private int _shots;
        private readonly int _magazine;
        private readonly float _spread;
        private readonly float _energyConsumption;
        private readonly IWeaponPlatform _platform;
        private readonly Factory.IBulletFactory _bulletFactory;
        private readonly IShip _ship;
        private float _cooldown;
    }
}
