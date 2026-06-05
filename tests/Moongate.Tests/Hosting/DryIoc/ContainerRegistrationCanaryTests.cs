using DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Hosting;
using Moongate.Tests.Support;

namespace Moongate.Tests.Hosting.DryIoc;

public class ContainerRegistrationCanaryTests : IDisposable
{
    private readonly string _dir = Path.Combine(
        Path.GetTempPath(),
        $"moongate-container-canary-{Guid.NewGuid():N}"
    );

    private string ConfigPath => Path.Combine(_dir, "moongate.toml");

    [Fact]
    public void AddMoongateHosting_CalledTwice_ResolvesSingleOrchestrator()
    {
        var container = new Container();
        container.AddMoongateHosting();
        container.AddMoongateHosting();

        Assert.Same(container.Orchestrator(), container.Orchestrator());
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task EventBus_RegisteredNatively_ResolvesAndStartsViaOrchestrator()
    {
        var container = new Container();
        container.AddMoongateEventBus();
        container.AddMoongateConfig(ConfigPath);

        // Interface aliases resolve to the same singleton instances.
        Assert.NotNull(container.Resolve<IEventBusService>());
        Assert.NotNull(container.Resolve<IGameLoopService>());

        // The orchestrator drives the lifecycle (surfaced as the host's IHostedService in Program.cs).
        var orchestrator = container.Orchestrator();
        Assert.Equal("MoongateServiceOrchestrator", orchestrator.GetType().Name);

        await orchestrator.StartAsync(CancellationToken.None);
        await orchestrator.StopAsync(CancellationToken.None);
    }
}
