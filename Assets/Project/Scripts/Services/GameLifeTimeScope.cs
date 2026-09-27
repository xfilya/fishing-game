using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerConfig _playerConfig;
    [SerializeField] private FishCatalog _fishCatalog;
    [SerializeField] private EquipmentCatalog _equipmentCatalog;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_playerConfig);
        builder.RegisterInstance(_fishCatalog);
        builder.RegisterInstance(_equipmentCatalog);

        builder.Register<IInputService, InputService>(Lifetime.Singleton);
        builder.Register<SaveService>(Lifetime.Singleton);
        builder.Register<ProgressService>(Lifetime.Singleton);
        builder.Register<EconomyService>(Lifetime.Singleton);
        builder.Register<CatchGenerator>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<UIService>();
        builder.RegisterComponentInHierarchy<FishingHUD>();
        builder.RegisterComponentInHierarchy<Player>();
        builder.RegisterComponentInHierarchy<PauseController>();
        builder.RegisterComponentInHierarchy<FishingController>();
        builder.RegisterComponentInHierarchy<FishShadowSpawner>();
        builder.RegisterComponentInHierarchy<AquariumController>();
        builder.RegisterComponentInHierarchy<EconomyHUD>();
        builder.RegisterComponentInHierarchy<ShopUIController>();
        builder.RegisterComponentInHierarchy<ShopInteractionController>();
        builder.RegisterComponentInHierarchy<PauseMenuController>();
        builder.RegisterComponentInHierarchy<AudioService>();
        builder.RegisterComponentInHierarchy<PlayerAudio>();
        builder.RegisterComponentInHierarchy<FishingAudio>();
    }
}
