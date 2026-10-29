using _Project.Scripts.Core;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Upgrades
{
    public class TapDamageUpgradeService
    {
        private const string LevelSaveKey = "Upgrades.TapDamage.Level";

        private readonly DamageInfo _damageInfo;
        private readonly ICurrencyController _currencyController;

        public int Level { get; private set; }
        public float NextCost => UpgradeMath.TapDamageCost(Level);
        public float CurrentDamage => UpgradeMath.TapDamage(Level);
        public bool CanBuy => _currencyController.HasEnough(NextCost, CurrencyType.SoftCurrency);

        public TapDamageUpgradeService(DamageInfo damageInfo, ICurrencyController currencyController)
        {
            _damageInfo = damageInfo;
            _currencyController = currencyController;

            Level = PlayerPrefs.GetInt(LevelSaveKey, 0);
            _damageInfo.SetDamageOnTap(CurrentDamage);
        }

        public bool TryBuy()
        {
            if (!_currencyController.TryRemoveCurrency(NextCost, CurrencyType.SoftCurrency))
            {
                return false;
            }

            Level++;
            PlayerPrefs.SetInt(LevelSaveKey, Level);
            PlayerPrefs.Save();
            _damageInfo.SetDamageOnTap(CurrentDamage);
            UpgradeChangedSignal.Emit();
            return true;
        }
    }
}
