using DryIoc;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Server.Extensions.Hosting;
using Moongate.Tests.Support;

namespace Moongate.Tests.Hosting;

public class ServiceCollectionExtensionsTests
{
    private interface IFooService : IMoongateService;

    private sealed class FooService : IFooService
    {
        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public void AddMoongateHosting_CalledTwice_ResolvesSingleOrchestrator()
    {
        var container = new Container();
        container.AddMoongateHosting();
        container.AddMoongateHosting();

        // Idempotent: the orchestrator resolves as a single instance.
        Assert.NotNull(container.Orchestrator());
        Assert.Same(container.Orchestrator(), container.Orchestrator());
    }

    [Fact]
    public void AddMoongateService_WithInterface_RegistersSingletonAndAlias()
    {
        var container = new Container();
        container.AddMoongateService<IFooService, FooService>();

        var asInterface = container.Resolve<IFooService>();
        var asImpl = container.Resolve<FooService>();

        Assert.NotNull(asImpl);
        Assert.NotNull(asInterface);
        Assert.Same(asImpl, asInterface);
    }

    [Fact]
    public void AddMoongateService_WithoutInterface_RegistersImplementationOnly()
    {
        var container = new Container();
        container.AddMoongateService<FooService>();

        Assert.NotNull(container.Resolve<FooService>());
    }
}
