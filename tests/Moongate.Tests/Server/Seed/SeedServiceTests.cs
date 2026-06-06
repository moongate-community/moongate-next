using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Moongate.Abstractions.Extensions.DryIoc;
using Moongate.Abstractions.Interfaces.EventHandlers;
using Moongate.Abstractions.Interfaces.Services;
using Moongate.Core.Utils;
using Moongate.Server.Data.Events;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.EventBus;
using Moongate.Server.Extensions.Persistence;
using Moongate.Server.Extensions.Seed;
using Moongate.Server.Extensions.Users;
using Moongate.Server.Services.Seed;
using Moongate.Tests.Support;
using Moongate.UO.Domain.Interfaces.Services;
using Moongate.UO.Domain.Types;

namespace Moongate.Tests.Server.Seed;

public sealed class SeedServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-seed-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "moongate.yaml");

    private sealed class SeedProbe
    {
        public string Value { get; }

        public SeedProbe(string value)
        {
            Value = value;
        }
    }

    [Fact]
    public async Task AddMoongateSeeds_RegistersHandlerThatRunsSeedsOnServerStartedEvent()
    {
        var calls = 0;
        var container = new Container();
        container.AddMoongateEventBus();
        container.AddMoongateSeeds();
        container.AddSeed(
            (_, _) =>
            {
                calls++;

                return ValueTask.CompletedTask;
            }
        );
        container.AddMoongateConfig(ConfigPath);

        var bus = container.Resolve<IEventBusService>();
        bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));

        Assert.Equal(1, bus.DrainTickEvents(10));
        await WaitUntilAsync(() => Volatile.Read(ref calls) == 1);

        Assert.Equal(1, calls);
        Assert.NotEmpty(container.ResolveMany<ITickEventHandler<ServerStartedEvent>>());
    }

    [Fact]
    public async Task DefaultAdminSeed_CreatesInitialAdminUser()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(ConfigPath, "persistence:\n  enable_file_lock: false\n");

        var container = new Container();
        container.AddMoongateEventBus();
        container.AddMoongateSeeds();
        container.AddMoongateUsers();
        container.AddDefaultAdminUserSeed();
        container.AddMoongatePersistence(_dir);
        container.AddMoongateConfig(ConfigPath);

        var orchestrator = container.Orchestrator();
        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            var bus = container.Resolve<IEventBusService>();
            bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));
            bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));

            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

            while (DateTime.UtcNow < deadline)
            {
                var users = container.Resolve<IUserService>();

                if (await users.CountAsync() == 1)
                {
                    break;
                }

                await Task.Delay(10);
            }

            var service = container.Resolve<IUserService>();
            var admin = await service.GetByUsernameAsync("admin");

            Assert.NotNull(admin);
            Assert.Equal(new(1), admin!.Id);
            Assert.Equal(UserLevelType.Administrator, admin.Level);
            Assert.True(admin.IsActive);
            Assert.True(HashUtils.VerifyPassword("admin", admin.Password));
            Assert.Equal(1, await service.CountAsync());
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
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
    public async Task RunAsync_ExecutesRegisteredActionsOnce()
    {
        var calls = 0;
        var services = new ServiceCollection().BuildServiceProvider();
        var service = new SeedService(
            services,
            [
                (_, _) =>
                {
                    calls++;

                    return ValueTask.CompletedTask;
                }
            ]
        );

        await service.RunAsync();
        await service.RunAsync();

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task RunAsync_PassesServiceProviderToActions()
    {
        var services = new ServiceCollection()
                       .AddSingleton(new SeedProbe("admin"))
                       .BuildServiceProvider();
        string? captured = null;
        var service = new SeedService(
            services,
            [
                (provider, _) =>
                {
                    captured = provider.GetRequiredService<SeedProbe>().Value;

                    return ValueTask.CompletedTask;
                }
            ]
        );

        await service.RunAsync();

        Assert.Equal("admin", captured);
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(10);
        }
    }
}
