using Combat.Component.Bullet;
using UnityEngine;

namespace Combat.Component.Controller
{
    public class EnergyWaveController : IController
    {
        public EnergyWaveController(IBullet bullet, float time, float size, float zoomFactor)
        {
            _bullet = bullet;
            _time = time;
            _originSize = size;
            _originColor = _bullet.View.Color;
            _zoomFactor = zoomFactor;
        }

        public void Dispose() {}

        public void UpdatePhysics(float elapsedTime)
        {
            _power = Mathf.Clamp01(_power + elapsedTime / _time);
            var fadeFactor = Mathf.Clamp01(5f * (1f - _power));

            _bullet.Body.SetSize(_originSize * (1f + (_zoomFactor - 1f) * _power));
            _bullet.View.Color = new Color(_originColor.r, _originColor.g, _originColor.b, _originColor.a * fadeFactor);
        }

        private float _power;
        private float _originSize;
        private readonly IBullet _bullet;
        private readonly Color _originColor;
        private readonly float _time;
        private static float _zoomFactor;
    }
}