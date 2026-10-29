using _Project.Scripts.Core;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Enemies
{
    public readonly struct EnemyHitEvent
    {
        public readonly float CurrentHealthPercent;
        public readonly float DamagePercent;

        public EnemyHitEvent(float currentHealth, float maxHealth, float damage)
        {
            CurrentHealthPercent = currentHealth / maxHealth;
            DamagePercent = damage / maxHealth;
        }
    }

    public class Enemy : MonoBehaviour, IDamageable
    {
        private float _currentHealth;
        private float _maxHealth;
        private float _reward;

        private bool IsDead => _currentHealth <= 0;

        public bool IsAlive => !IsDead;
        public float MaxHealth => _maxHealth;

        public void TakeDamage(float damage)
        {
            damage = MathfRound.Round(damage);

            var actualDamage = Mathf.Min(damage, _currentHealth);

            _currentHealth = MathfRound.Round(_currentHealth - actualDamage);
            if (_currentHealth < 0)
            {
                _currentHealth = 0;
            }

            EnemyHitSignal.Emit(new EnemyHitEvent(_currentHealth, _maxHealth, actualDamage));

            if (IsDead)
            {
                EnemyDiedEvent.Emit();
                Destroy(gameObject);
            }
        }

        public void Configure(float health)
        {
            _maxHealth = MathfRound.Round(health);
            _currentHealth = _maxHealth;
        }
    }
}