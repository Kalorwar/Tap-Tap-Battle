using System.Collections.Generic;
using _Project.Scripts.Assistants;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Upgrades;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Core
{
    public class LevelInstaller : MonoInstaller
    {
        [SerializeField] private EnemyLibrary _enemyLibrary;
        [SerializeField] private List<AssistantConfig> _assistantConfigs = new();

        public override void InstallBindings()
        {
            Container.Bind<EnemyLibrary>().FromInstance(_enemyLibrary).AsSingle().NonLazy();
            Container.Bind<DamageInfo>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RewardService>().AsSingle().NonLazy();
            Container.Bind<ICurrencyController>().To<CurrencyController.CurrencyController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelCountService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DamageController>().AsSingle().NonLazy();
            Container.Bind<SeedService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BossFightService>().AsSingle().NonLazy();
            Container.Bind<TapDamageUpgradeService>().AsSingle().NonLazy();
            Container.Bind<IReadOnlyList<AssistantConfig>>().FromInstance(_assistantConfigs).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AssistantService>().AsSingle().NonLazy();
        }
    }
}
