using _Project.Scripts.Events;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class BossTimerView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private RectTransform _root; // optional; the text object is used when not set

        private GameObject _rootObject;

        private void Awake()
        {
            _rootObject = _root != null ? _root.gameObject : _text.gameObject;

            // Hidden until the boss fight starts
            _rootObject.SetActive(false);
        }

        private void OnEnable()
        {
            BossTimerSignal.OnBossTimer += OnBossTimer;
        }

        private void OnDisable()
        {
            BossTimerSignal.OnBossTimer -= OnBossTimer;
        }

        private void OnBossTimer(BossTimerInfo info)
        {
            // RemainingSeconds is already an integer (ceil), so a plain cast is enough
            if (info.IsRunning)
            {
                _text.text = ((int)info.RemainingSeconds).ToString();
                _rootObject.SetActive(true);
            }
            else
            {
                _rootObject.SetActive(false);
            }
        }
    }
}
