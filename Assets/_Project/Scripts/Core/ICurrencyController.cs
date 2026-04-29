namespace _Project.Scripts.Core
{
    public interface ICurrencyController
    {
        public float GetAmount(CurrencyType type);
        public void AddCurrency(float amount, CurrencyType currencyType);
        public bool TryRemoveCurrency(float amount, CurrencyType currencyType);
        public bool HasEnough(float amount, CurrencyType currencyType);
    }
}