using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerConfig _playerConfig;
    [SerializeField] private FishCatalog _fishCatalog;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_playerConfig);
        builder.RegisterInstance(_fishCatalog);

        builder.Register<IInputService, InputService>(Lifetime.Singleton);
        builder.Register<ProgressService>(Lifetime.Singleton);
        builder.Register<CatchGenerator>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<UIService>();
        builder.RegisterComponentInHierarchy<FishingHUD>();
        builder.RegisterComponentInHierarchy<Player>();
        builder.RegisterComponentInHierarchy<PauseController>();
        builder.RegisterComponentInHierarchy<FishingController>();
        builder.RegisterComponentInHierarchy<FishShadowSpawner>();
        builder.RegisterComponentInHierarchy<AquariumController>();
    }
}
