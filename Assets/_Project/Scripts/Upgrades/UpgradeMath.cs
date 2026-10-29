using System;

namespace _Project.Scripts.Upgrades
{
    public static class UpgradeMath
    {
        private const double BaseCost = 50.0;
        private const double CostGrowth = 1.07;
        private const float DamagePerLevel = 10f;
        private const float StartDamage = 1f;

        // Cost of the NEXT purchase at the given number of purchased levels.
        public static float TapDamageCost(int purchasedLevels)
        {
            return (float)(BaseCost * Math.Pow(CostGrowth, purchasedLevels));
        }

        public static float TapDamage(int purchasedLevels)
        {
            return purchasedLevels <= 0 ? StartDamage : purchasedLevels * DamagePerLevel;
        }
    }
}
