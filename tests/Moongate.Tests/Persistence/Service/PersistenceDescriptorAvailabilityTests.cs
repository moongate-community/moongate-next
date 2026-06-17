using Moongate.Abstractions.Data.Persistence;
using Moongate.Core.Ids;
using Moongate.Persistence.Data;
using Moongate.Persistence.Services.Persistence;
using Moongate.Tests.Persistence.Support;

namespace Moongate.Tests.Persistence.Service;

public class PersistenceDescriptorAvailabilityTests
{
    [Fact]
    public void GetAutoDataAccess_BeforeStartAsync_ResolvesRegisteredDescriptor()
    {
        var directory = Directory.CreateTempSubdirectory().FullName;

        try
        {
            var descriptor = new PersistenceEntityDescriptor<TestPlayer, Serial>(1, "TestPlayer", 1, player => player.Id);
            var registrations = new[] { new PersistenceEntityRegistration(descriptor) };
            var service = new PersistenceService(directory, new PersistenceConfig { EnableFileLock = false }, registrations);

            // A boot service with a lower priority than persistence resolves entity data access
            // before StartAsync runs; the descriptor must already be available.
            var access = service.GetAutoDataAccess<TestPlayer, Serial>();

            Assert.NotNull(access);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
