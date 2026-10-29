using System;

namespace _Project.Scripts.CurrencyController
{
    public interface ICurrencyController
    {
        public event Action OnCurrencyChange;
        public float GetAmount(CurrencyType type);
        public void AddCurrency(float amount, CurrencyType currencyType);
        public bool TryRemoveCurrency(float amount, CurrencyType currencyType);
        public bool HasEnough(float amount, CurrencyType currencyType);
    }
}