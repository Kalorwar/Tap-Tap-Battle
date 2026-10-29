using System;
using _Project.Scripts.Events;

namespace _Project.Scripts.Core
{
    public class DamageController : IDisposable
    {
        private readonly DamageInfo _damageInfo;
        private IDamageable _damageable;

        public DamageController(DamageInfo damageInfo)
        {
            _damageInfo = damageInfo;
            TapEvent.OnTapped += OnTapped;
        }

        public void Dispose()
        {
            TapEvent.OnTapped -= OnTapped;
        }

        public void SetTarget(IDamageable damageable)
        {
            _damageable = damageable;
        }

        private void OnTapped()
        {
            if (_damageable == null)
            {
                return;
            }

            _damageable.TakeDamage(_damageInfo.DamageOnTap);
        }
    }
}
