using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerConfig _playerConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_playerConfig);

        builder.Register<IInputService, InputService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<UIService>();
        builder.RegisterComponentInHierarchy<Player>();
        builder.RegisterComponentInHierarchy<PauseController>();
    }
}