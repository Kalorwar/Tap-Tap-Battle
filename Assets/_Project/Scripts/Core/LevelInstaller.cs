using _Project.Scripts.CurrencyController;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Core
{
    public class LevelInstaller : MonoInstaller
    {
        [SerializeField] private EnemyLibrary _enemyLibrary;

        public override void InstallBindings()
        {
            Container.Bind<EnemyLibrary>().FromInstance(_enemyLibrary).AsSingle().NonLazy();
            Container.Bind<DamageInfo>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RewardService>().AsSingle().NonLazy();
            Container.Bind<ICurrencyController>().To<CurrencyController.CurrencyController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelCountService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DamageController>().AsSingle().NonLazy();
            Container.Bind<SeedService>().AsSingle().NonLazy();
        }
    }
}