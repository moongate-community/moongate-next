using Microsoft.AspNetCore.Http.HttpResults;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Templates;
using Moongate.Server.Extensions.Endpoints;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Tests.Server.Endpoints;

public sealed class UoItemEndpointExtensionsTests
{
    [Fact]
    public void HandleList_ReturnsPagedUoItems()
    {
        var result = UoItemEndpointExtensions.HandleList(
            SeedTileData(),
            1,
            2,
            null,
            null
        );

        var ok = Assert.IsType<Ok<PagedResult<UoItemSummary>>>(result);
        Assert.Equal(2, ok.Value!.Items.Count);
        Assert.Equal(3, ok.Value.TotalCount);
        Assert.Equal(["gold coin", "wooden crate"], ok.Value.Items.Select(static item => item.Name));
    }

    [Theory]
    [InlineData("crate", "wooden crate")]
    [InlineData("1", "wooden crate")]
    [InlineData("0x001", "wooden crate")]
    [InlineData("0x1", "wooden crate")]
    [InlineData("wearable", "longsword")]
    public void HandleList_SearchesNameDecimalAndHex(string search, string expectedName)
    {
        var result = UoItemEndpointExtensions.HandleList(
            SeedTileData(),
            1,
            20,
            search,
            null
        );

        var ok = Assert.IsType<Ok<PagedResult<UoItemSummary>>>(result);
        Assert.Equal(expectedName, Assert.Single(ok.Value!.Items).Name);
    }

    [Fact]
    public void HandleList_FiltersByFlag()
    {
        var result = UoItemEndpointExtensions.HandleList(
            SeedTileData(),
            1,
            20,
            null,
            "Container"
        );

        var ok = Assert.IsType<Ok<PagedResult<UoItemSummary>>>(result);
        var item = Assert.Single(ok.Value!.Items);
        Assert.Equal("wooden crate", item.Name);
        Assert.True(item.Container);
        Assert.Contains("Container", item.Flags);
    }

    [Fact]
    public void HandleList_InvalidFlag_ReturnsBadRequest()
    {
        var result = UoItemEndpointExtensions.HandleList(
            SeedTileData(),
            1,
            20,
            null,
            "Sparkly"
        );

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public void HandleDetail_ExistingItem_ReturnsDetail()
    {
        var result = UoItemEndpointExtensions.HandleDetail(SeedTileData(), "0x001");

        var ok = Assert.IsType<Ok<UoItemDetail>>(result);
        Assert.Equal(1, ok.Value!.ItemId);
        Assert.Equal("0x001", ok.Value.ItemIdHex);
        Assert.Equal("wooden crate", ok.Value.Name);
        Assert.Equal("/api/items/0x001.png", ok.Value.ImageUrl);
        Assert.True(ok.Value.Container);
        Assert.True(ok.Value.Surface);
        Assert.Equal((ulong)(UoTileFlag.Container | UoTileFlag.Surface), ok.Value.RawFlags);
    }

    [Fact]
    public void HandleDetail_InvalidItemId_ReturnsBadRequest()
    {
        var result = UoItemEndpointExtensions.HandleDetail(SeedTileData(), "crate");

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public void HandleDetail_MissingItem_ReturnsNotFound()
    {
        var result = UoItemEndpointExtensions.HandleDetail(SeedTileData(), "0x063");

        Assert.IsType<NotFound>(result);
    }

    private static TestTileDataStore SeedTileData()
        => new(
            [
                new("gold coin", UoTileFlag.Generic, 1, 0, 0, 0, 1, 1),
                new("wooden crate", UoTileFlag.Container | UoTileFlag.Surface, 10, 0, 0, 0, 5, 6),
                new("longsword", UoTileFlag.Weapon | UoTileFlag.Wearable, 5, 0, 0, 0, 45, 1),
                default
            ]
        );

    private sealed class TestTileDataStore : ITileDataStore
    {
        public TestTileDataStore(IReadOnlyList<ItemData> itemTable)
        {
            ItemTable = itemTable;
        }

        public IReadOnlyList<LandData> LandTable { get; } = [];

        public IReadOnlyList<ItemData> ItemTable { get; }

        public ItemData GetItem(int id)
            => id >= 0 && id < ItemTable.Count ? ItemTable[id] : default;

        public LandData GetLand(int id)
            => default;
    }
}
