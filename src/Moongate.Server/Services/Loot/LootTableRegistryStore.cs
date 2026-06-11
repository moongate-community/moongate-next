namespace Moongate.Server.Services.Loot;

public sealed class LootTableRegistryStore
{
    private LootTableRegistry? _registry;

    public LootTableRegistry Registry
        => _registry ?? throw new InvalidOperationException("Loot table registry is not ready.");

    public bool IsReady => _registry is not null;

    public void SetRegistry(LootTableRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        _registry = registry;
    }
}
