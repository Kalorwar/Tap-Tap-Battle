using _Project.Scripts.Core;
using TMPro;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.UI
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        private LevelCountService _levelCountService;

        [Inject]
        private void Construct(LevelCountService levelCountService)
        {
            _levelCountService = levelCountService;
        }

        private void Start()
        {
            SetLevel(_levelCountService.LevelCount);
        }

        private void OnEnable()
        {
            // The signal is emitted with the NEW level number already
            LevelCompletedSignal.OnLevelCompleted += SetLevel;
        }

        private void OnDisable()
        {
            LevelCompletedSignal.OnLevelCompleted -= SetLevel;
        }

        private void SetLevel(int level)
        {
            _text.text = $"Уровень {level}";
        }
    }
}
