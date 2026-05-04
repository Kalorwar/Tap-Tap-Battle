using System;
using _Project.Scripts.Events;
using R3;

namespace _Project.Scripts.Core
{
    public class DamageController : IDisposable
    {
        private IDamageable _damageable;
        private CompositeDisposable _disposables;

        public DamageController(DamageInfo damageInfo)
        {
            TapEvent.OnTapped.Subscribe(_ => ApplyDamage(damageInfo.DamageOnTap));
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        public void SetTarget(IDamageable damageable)
        {
            _damageable = damageable;
        }

        private void ApplyDamage(float damage)
        {
            _damageable.TakeDamage(damage);
        }
    }
}