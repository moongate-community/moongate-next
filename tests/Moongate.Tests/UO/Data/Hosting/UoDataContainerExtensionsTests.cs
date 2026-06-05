using DryIoc;
using Moongate.Server.Extensions.UoData;
using Moongate.UO.Data.Data;
using Moongate.UO.Data.Interfaces.Files;

namespace Moongate.Tests.UO.Data.Hosting;

public class UoDataContainerExtensionsTests
{
    [Fact]
    public void AddMoongateUoData_RegistersResolverAndVerdataAndConfigSection()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);

            var container = new Container();
            container.RegisterInstance(new UoConfig { ClientFilesDirectory = dir.FullName });
            container.AddMoongateUoData(dir.FullName);

            var resolver = container.Resolve<IUoFileResolver>();
            var verdata = container.Resolve<IVerdataPatchSource>();

            Assert.Equal(dir.FullName, resolver.RootDirectory);
            Assert.Empty(verdata.Patches);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
