using System.Collections.Generic;
using _Project.Scripts.Events;
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

        private readonly List<RaycastResult> _raycastResults = new(8);
        private PointerEventData _eventData;
        private EventSystem _eventSystem;

        private void Awake()
        {
            EnhancedTouchSupport.Enable();
            _eventSystem = EventSystem.current;
            _eventData = new PointerEventData(_eventSystem);
        }

        private void Update()
        {
            if (Touch.activeTouches.Count == 0)
            {
                return;
            }

            var touch = Touch.activeTouches[0];
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            if (IsPointerOverUI())
            {
                return;
            }

            if (_debugLog)
            {
                Debug.Log("[Tap]");
            }

            TapEvent.Emit();
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
