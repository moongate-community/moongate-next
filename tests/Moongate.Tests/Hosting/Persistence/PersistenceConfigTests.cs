using Moongate.Abstractions.Data.Persistence;

namespace Moongate.Tests.Hosting.Persistence;

public class PersistenceConfigTests
{
    [Fact]
    public void Defaults_MatchExpected()
    {
        var config = new PersistenceConfig();

        Assert.Equal(TimeSpan.FromSeconds(300), config.AutosaveInterval);
        Assert.Equal(".snapshot.bin", config.SnapshotFileSuffix);
        Assert.Equal("world.journal.bin", config.JournalFileName);
        Assert.True(config.EnableFileLock);
    }
}
