namespace _Project.Scripts.Core
{
    public class DamageInfo
    {
        public float DamageOnTap { get; private set; } = 1f;
        public float DamagePerSecond { get; private set; }

        public void SetDamagePerSecond(float damagePerSecond)
        {
            DamagePerSecond = damagePerSecond;
        }

        public void SetDamageOnTap(float damage)
        {
            DamageOnTap = damage;
        }
    }
}