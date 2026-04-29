using UnityEngine;

namespace _Project.Scripts.Core
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        private float _maxHealth;
        private float _reward;

        public float CurrentHealth { get; private set; }

        public bool IsDead => CurrentHealth <= 0;

        public void TakeDamage(float damage)
        {
            CurrentHealth = Round(CurrentHealth - damage);
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }

            if (IsDead)
            {
                EnemyDiedEvent.Emit();
                Destroy(gameObject);
            }
        }

        public void Configure(float health)
        {
            _maxHealth = Round(health);
            CurrentHealth = health;
        }

        private float Round(float value)
        {
            return Mathf.Round(value * 10f) / 10f;
        }
    }
}