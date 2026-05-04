using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.CurrencyController
{
    public class CurrencyController : ICurrencyController
    {
        private const string SoftCurrencyKey = "SoftCurrency";
        private const string HardCurrencyKey = "HardCurrency";

        private readonly Dictionary<CurrencyType, float> _currencyValues = new();

        public CurrencyController()
        {
            _currencyValues[CurrencyType.SoftCurrency] = Load(SoftCurrencyKey);
            _currencyValues[CurrencyType.HardCurrency] = Load(HardCurrencyKey);
        }

        public float GetAmount(CurrencyType type)
        {
            return _currencyValues[type];
        }

        public void AddCurrency(float amount, CurrencyType currencyType)
        {
            if (amount <= 0)
            {
                return;
            }

            _currencyValues[currencyType] += amount;
            Save(currencyType);
        }

        public bool TryRemoveCurrency(float amount, CurrencyType currencyType)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (_currencyValues[currencyType] < amount)
            {
                return false;
            }

            _currencyValues[currencyType] -= amount;
            Save(currencyType);
            return true;
        }

        public bool HasEnough(float amount, CurrencyType currencyType)
        {
            return _currencyValues[currencyType] >= amount;
        }

        private void Save(CurrencyType currencyType)
        {
            var key = GetKey(currencyType);
            PlayerPrefs.SetFloat(key, _currencyValues[currencyType]);
            PlayerPrefs.Save();
        }

        private float Load(string key)
        {
            return PlayerPrefs.GetFloat(key, 0f);
        }

        private string GetKey(CurrencyType currencyType)
        {
            return currencyType switch
            {
                CurrencyType.SoftCurrency => SoftCurrencyKey,
                CurrencyType.HardCurrency => HardCurrencyKey,
                _ => throw new ArgumentOutOfRangeException(nameof(currencyType), currencyType, null)
            };
        }
    }
}