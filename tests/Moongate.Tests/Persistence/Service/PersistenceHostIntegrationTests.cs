using DryIoc;
using Moongate.Core.Ids;
using Moongate.Persistence.Extensions.DryIoc;
using Moongate.Persistence.Interfaces.Persistence;
using Moongate.Persistence.Services.Persistence;
using Moongate.Server.Extensions.Configuration;
using Moongate.Server.Extensions.Persistence;
using Moongate.Tests.Persistence.Support;
using Moongate.Tests.Support;

namespace Moongate.Tests.Persistence.Service;

public class PersistenceHostIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-persist-host-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "moongate.yaml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Host_StartsService_AndDataAccessIsInjectable()
    {
        var container = NewContainer();
        var orchestrator = container.Orchestrator();

        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            var players = container.Resolve<IDataAccess<TestPlayer, Serial>>();
            await players.UpsertAsync(new TestPlayer { Id = new Serial(1), Name = "Hosted" });

            Assert.Equal("Hosted", (await players.GetByIdAsync(new Serial(1)))!.Name);
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public void PersistenceService_ReportsMetrics()
    {
        var container = NewContainer();
        var metrics = (PersistenceService)
            container.Resolve<IPersistenceService>();

        var names = metrics.Collect().Select(s => s.Name).ToHashSet();

        Assert.Contains("entities_total", names);
        Assert.Contains("snapshots_written_total", names);
    }

    private IContainer NewContainer()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(ConfigPath, "persistence:\n  enable_file_lock: false\n");

        var container = new Container();
        container.RegisterPersistenceEntity<TestPlayer, Serial>(1, 1, p => p.Id);
        container.RegisterPersistenceEntity<TestItem, Serial>(2, 1, i => i.Id);
        container.AddMoongatePersistence(_dir);
        container.AddMoongateConfig(ConfigPath);

        return container;
    }
}
