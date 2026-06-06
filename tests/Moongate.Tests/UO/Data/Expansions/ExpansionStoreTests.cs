using Moongate.Abstractions.Types.Expansions;
using Moongate.UO.Data.Expansions;
using Moongate.UO.Data.Types.Expansions;

namespace Moongate.Tests.UO.Data.Expansions;

public class ExpansionStoreTests
{
    private const string Yaml =
        """
        expansion:
          - id: 5
            name: Age of Shadows
            client_flags: Trammel, Ilshenar, Malas
            supported_features: ExpansionAos
            character_list_flags: ExpansionAos
            housing_flags: HousingAos
            map_selection_flags: Felucca, Trammel, Ilshenar, Malas
            mobile_status_version: 6
            required_client_version: 4.0.0a
        """;

    [Fact]
    public void Load_ParsesExpansion_AndFlags()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "expansions.yaml"), Yaml);
            var store = new ExpansionStore(dir.FullName);

            Assert.Equal(1, store.Count);

            var aos = store.GetInfo(UoExpansionType.Aos);
            Assert.NotNull(aos);
            Assert.Equal("Age of Shadows", aos!.Name);
            Assert.True(aos.HousingFlags.HasFlag(HousingFlags.Aos));
            Assert.True(aos.SupportedFeatures.HasFlag(FeatureFlags.Aos));
            Assert.Equal(6, aos.MobileStatusVersion);
            Assert.NotNull(aos.RequiredClient);
            Assert.Equal(4, aos.RequiredClient!.Major);
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void MissingFile_YieldsEmptyTable()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var store = new ExpansionStore(dir.FullName);

            Assert.Equal(0, store.Count);
            Assert.Null(store.GetInfo(0));
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
