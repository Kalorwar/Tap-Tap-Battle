using _Project.Scripts.Events;
using Assets.PixelFantasy.Common.Scripts;
using R3;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class PlayerTapHandler : MonoBehaviour
    {
        private readonly CompositeDisposable _disposables = new();
        private Creature _character;
        private UnitAnimation _unitAnimation;

        private void Awake()
        {
            _character = GetComponent<Creature>();
            _unitAnimation = new UnitAnimation(_character);
        }

        private void OnEnable()
        {
            TapEvent.OnTapped.Subscribe(_ => AttackAnimation()).AddTo(_disposables);
        }

        private void OnDisable()
        {
            _disposables.Dispose();
        }

        private void AttackAnimation()
        {
            Debug.Log("Attack");
            _unitAnimation.Slash();
        }
    }

    public class UnitAnimation
    {
        private static readonly int Slash1 = Animator.StringToHash("Slash");
        private readonly Creature _character;

        public UnitAnimation(Creature character)
        {
            _character = character;
        }

        public void Slash()
        {
            _character.Animator.SetTrigger(Slash1);
        }
    }
}