using System.Globalization;
using _Project.Scripts.CurrencyController;
using TMPro;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.UI
{
    public class CurrencyUI : MonoBehaviour
    {
        [SerializeField] private CurrencyType _currencyType;
        [SerializeField] private TextMeshProUGUI _currencyText;

        private ICurrencyController _currencyController;

        [Inject]
        private void Construct(ICurrencyController currencyController)
        {
            _currencyController = currencyController;
        }

        private void Start()
        {
            UpdateUI();
        }

        private void OnEnable()
        {
            _currencyController.OnCurrencyChange += UpdateUI;
        }

        private void OnDisable()
        {
            if (_currencyController != null)
            {
                _currencyController.OnCurrencyChange -= UpdateUI;
            }
        }

        private void UpdateUI()
        {
            var currency = _currencyController.GetAmount(_currencyType);
            _currencyText.text = currency.ToString(CultureInfo.InvariantCulture);
        }
    }
}