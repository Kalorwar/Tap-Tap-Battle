using System.Collections.Generic;
using _Project.Scripts.Events;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace _Project.Scripts.Core
{
    public class ClickDetectorTouch : MonoBehaviour
    {
        [SerializeField] private bool _debugLog;

        private readonly CompositeDisposable _disposables = new();
        private readonly List<RaycastResult> _raycastResults = new(8);
        private PointerEventData _eventData;
        private EventSystem _eventSystem;

        private void Awake()
        {
            EnhancedTouchSupport.Enable();
            _eventSystem = EventSystem.current;
            _eventData = new PointerEventData(_eventSystem);

            Observable.EveryUpdate()
                .Where(_ => Touch.activeTouches.Count > 0)
                .Select(_ => Touch.activeTouches[0])
                .Where(touch => touch.phase == TouchPhase.Began)
                .Where(_ => !IsPointerOverUI())
                .Subscribe(_ =>
                {
                    if (_debugLog)
                    {
                        Debug.Log("[Tap]");
                    }

                    TapEvent.Emit();
                })
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ObservableSystem.DefaultFrameProvider = UnityFrameProvider.Update;
        }

        private bool IsPointerOverUI()
        {
            if (_eventSystem == null)
            {
                return false;
            }

            _eventData.position = Touch.activeTouches[0].screenPosition;
            _raycastResults.Clear();
            _eventSystem.RaycastAll(_eventData, _raycastResults);

            for (var i = 0; i < _raycastResults.Count; i++)
            {
                if (_raycastResults[i].gameObject.GetComponent<RectTransform>() != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}