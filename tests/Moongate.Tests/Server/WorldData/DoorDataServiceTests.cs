using Moongate.Server.Services.World;

namespace Moongate.Tests.Server.WorldData;

public class DoorDataServiceTests
{
    [Fact]
    public void SetEntries_BuildsToggleDefinitionsForClosedAndOpenIds()
    {
        const int closedItemId = 0x06A5;
        const int openedItemId = 0x06A6;

        var service = new DoorDataService();

        service.SetEntries(
            [
                new(
                    0,
                    0x06A5,
                    0x06A7,
                    0x06A9,
                    0x06AB,
                    0x06AD,
                    0x06AF,
                    0x06B1,
                    0x06B3,
                    0,
                    "Metal Door"
                )
            ]
        );

        Assert.True(service.TryGetToggleDefinition(closedItemId, out var closedDefinition));
        Assert.Equal(openedItemId, closedDefinition.NextItemId);
        Assert.True(closedDefinition.IsClosed);
        Assert.Equal(new(-1, 0, 0), closedDefinition.Offset);

        Assert.True(service.TryGetToggleDefinition(openedItemId, out var openedDefinition));
        Assert.Equal(closedItemId, openedDefinition.NextItemId);
        Assert.False(openedDefinition.IsClosed);
        Assert.Equal(new(1, 0, 0), openedDefinition.Offset);

        Assert.True(service.TryGetToggleDefinition(0x06A4, out var legacyDefinition));
        Assert.Equal(openedItemId, legacyDefinition.NextItemId);
        Assert.True(legacyDefinition.IsClosed);
        Assert.Equal(new(-1, 0, 0), legacyDefinition.Offset);
    }
}
