using System.Globalization;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.ListQueries;
using Moongate.Server.Data.Templates;
using Moongate.UO.Data.Data.Tiles;
using Moongate.UO.Data.Interfaces.Tiles;
using Moongate.UO.Data.Types.Tiles;

namespace Moongate.Server.Extensions.Endpoints;

public static class UoItemEndpointExtensions
{
    public static IEndpointRouteBuilder MapMoongateUoItems(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/uo/items")
                             .WithTags("Admin UO Items")
                             .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)));

        group.MapGet(
                 "/",
                 (ITileDataStore tileData, int? page, int? pageSize, string? search, string? flag)
                     => HandleList(tileData, page, pageSize, search, flag)
             )
             .WithName("ListUoItems")
             .WithSummary("Returns a paginated, searchable list of raw UO item tile data.");

        group.MapGet(
                 "/{itemId}",
                 (ITileDataStore tileData, string itemId) => HandleDetail(tileData, itemId)
             )
             .WithName("GetUoItem")
             .WithSummary("Returns raw UO item tile data for a single item id.");

        return endpoints;
    }

    internal static IResult HandleDetail(ITileDataStore tileData, string itemId)
    {
        ArgumentNullException.ThrowIfNull(tileData);

        if (!TryParseItemId(itemId, out var parsedItemId))
        {
            return TypedResults.BadRequest("itemId must be a decimal number or hex text prefixed with 0x.");
        }

        if (parsedItemId >= tileData.ItemTable.Count)
        {
            return TypedResults.NotFound();
        }

        var item = tileData.GetItem(parsedItemId);

        return IsEmpty(item) ? TypedResults.NotFound() : TypedResults.Ok(ToDetail(parsedItemId, item));
    }

    internal static IResult HandleList(
        ITileDataStore tileData,
        int? page,
        int? pageSize,
        string? search,
        string? flag
    )
    {
        ArgumentNullException.ThrowIfNull(tileData);

        if (!TryParseFlag(flag, out var flagFilter, out var flagError))
        {
            return TypedResults.BadRequest(flagError);
        }

        var filters = new List<Func<(int ItemId, ItemData Item), bool>>();

        if (flagFilter.HasValue)
        {
            filters.Add(item => item.Item[flagFilter.Value]);
        }

        var request = PageRequest.Normalize(page, pageSize, search);
        var ordered = tileData.ItemTable
                              .Select(static (item, itemId) => (ItemId: itemId, Item: item))
                              .Where(static item => !IsEmpty(item.Item))
                              .OrderBy(static item => item.ItemId);
        var result = InMemoryListQuery.Apply(ordered, request, SearchFields, filters);

        return TypedResults.Ok(result.Select(static item => ToSummary(item.ItemId, item.Item)));
    }

    private static IReadOnlyList<string> FormatFlags(UoTileFlag flags)
        => Enum.GetValues<UoTileFlag>()
               .Where(flag => flag != UoTileFlag.None && flags.HasFlag(flag))
               .Select(static flag => flag.ToString())
               .ToArray();

    private static string FormatImageUrl(int itemId)
        => $"/api/items/{FormatItemId(itemId)}.png";

    private static string FormatItemId(int itemId)
        => $"0x{itemId.ToString("X4", CultureInfo.InvariantCulture)}";

    private static bool IsEmpty(ItemData item)
        => string.IsNullOrWhiteSpace(item.Name) && item.Flags == UoTileFlag.None;

    private static IEnumerable<string?> SearchFields((int ItemId, ItemData Item) item)
    {
        yield return item.ItemId.ToString(CultureInfo.InvariantCulture);
        yield return FormatItemId(item.ItemId);
        yield return $"0x{item.ItemId:X3}";
        yield return $"0x{item.ItemId:X}";
        yield return item.Item.Name;

        foreach (var flag in FormatFlags(item.Item.Flags))
        {
            yield return flag;
        }
    }

    private static UoItemDetail ToDetail(int itemId, ItemData item)
        => new(
            itemId,
            FormatItemId(itemId),
            item.Name,
            FormatImageUrl(itemId),
            FormatFlags(item.Flags),
            (ulong)item.Flags,
            item.Weight,
            item.Quality,
            item.Animation,
            item.Quantity,
            item.Value,
            item.Height,
            item[UoTileFlag.Container],
            item[UoTileFlag.Weapon],
            item[UoTileFlag.Armor],
            item[UoTileFlag.Wearable],
            item[UoTileFlag.Door],
            item[UoTileFlag.Surface],
            item[UoTileFlag.Background],
            item[UoTileFlag.Wall]
        );

    private static UoItemSummary ToSummary(int itemId, ItemData item)
        => new(
            itemId,
            FormatItemId(itemId),
            item.Name,
            FormatImageUrl(itemId),
            FormatFlags(item.Flags),
            item.Weight,
            item.Quality,
            item.Animation,
            item.Quantity,
            item.Value,
            item.Height,
            item[UoTileFlag.Container],
            item[UoTileFlag.Weapon],
            item[UoTileFlag.Armor],
            item[UoTileFlag.Wearable],
            item[UoTileFlag.Door],
            item[UoTileFlag.Surface],
            item[UoTileFlag.Background],
            item[UoTileFlag.Wall]
        );

    private static bool TryParseFlag(string? value, out UoTileFlag? parsed, out string error)
    {
        parsed = null;
        error = "";

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Enum.TryParse<UoTileFlag>(value, true, out var result) && result != UoTileFlag.None)
        {
            parsed = result;

            return true;
        }

        error = $"Unknown UoTileFlag '{value}'.";

        return false;
    }

    private static bool TryParseItemId(string? value, out int itemId)
    {
        itemId = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        var parsed = trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                         ? int.TryParse(
                             trimmed.AsSpan(2),
                             NumberStyles.AllowHexSpecifier,
                             CultureInfo.InvariantCulture,
                             out itemId
                         )
                         : int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out itemId);

        return parsed && itemId >= 0;
    }
}
