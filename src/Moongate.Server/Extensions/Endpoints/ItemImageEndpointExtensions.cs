using System.Collections.Concurrent;
using System.Globalization;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Items;
using Moongate.Server.Utils;
using Moongate.UO.Data.Interfaces.Art;
using SixLabors.ImageSharp.Formats.Png;

namespace Moongate.Server.Extensions.Endpoints;

public static class ItemImageEndpointExtensions
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _generationLocks = new();

    public static IEndpointRouteBuilder MapMoongateItemImages(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                "/api/items/{itemId}.png",
                (
                    string itemId,
                    IArtService artService,
                    DirectoriesConfig directories,
                    CancellationToken cancellationToken
                ) => HandleGetItemImageAsync(itemId, artService, directories, cancellationToken)
            )
            .WithName("GetItemImage")
            .WithTags("Items")
            .WithSummary("Returns a lazily generated PNG image of the specified UO item art.")
            .Produces(StatusCodes.Status200OK, contentType: "image/png")
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        endpoints.MapPost(
                "/api/items/build",
                (
                    IArtService artService,
                    DirectoriesConfig directories,
                    CancellationToken cancellationToken
                ) => HandleBuildItemImagesAsync(artService, directories, cancellationToken)
            )
            .WithName("BuildItemImages")
            .WithTags("Items")
            .WithSummary("Generates and caches PNG images for all available UO item art.")
            .Produces<ItemImageBuildResult>()
            .Produces(StatusCodes.Status200OK);

        return endpoints;
    }

    internal static string FormatFileName(int itemId)
    {
        return $"0x{itemId.ToString("X3", CultureInfo.InvariantCulture)}.png";
    }

    internal static string GetCachePath(DirectoriesConfig directories, int itemId)
    {
        ArgumentNullException.ThrowIfNull(directories);

        var directory = Path.Combine(directories[DirectoryType.Cache], "images", "items");
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, FormatFileName(itemId));
    }

    internal static async Task<IResult> HandleBuildItemImagesAsync(
        IArtService artService,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(artService);
        ArgumentNullException.ThrowIfNull(directories);

        var generated = 0;
        var cached = 0;
        var skipped = 0;
        var failed = 0;
        var maxItemId = artService.MaxItemId;

        for (var itemId = 0; itemId <= maxItemId; itemId++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await EnsureItemImageAsync(itemId, artService, directories, cancellationToken);

                if (!result.HasImage)
                {
                    skipped++;
                }
                else if (result.Generated)
                {
                    generated++;
                }
                else
                {
                    cached++;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                failed++;
            }
        }

        return TypedResults.Ok(
            new ItemImageBuildResult
            {
                MaxItemId = maxItemId,
                Generated = generated,
                Cached = cached,
                Skipped = skipped,
                Failed = failed
            }
        );
    }

    internal static async Task<IResult> HandleGetItemImageAsync(
        string itemIdText,
        IArtService artService,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(artService);
        ArgumentNullException.ThrowIfNull(directories);

        if (!TryParseHexItemId(itemIdText, out var itemId))
        {
            return TypedResults.BadRequest("itemId must be in 0x... format");
        }

        var result = await EnsureItemImageAsync(itemId, artService, directories, cancellationToken);

        return result.HasImage
            ? Results.File(result.CachePath, "image/png")
            : TypedResults.NotFound();
    }

    private static async Task<(bool HasImage, bool Generated, string CachePath)> EnsureItemImageAsync(
        int itemId,
        IArtService artService,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        var cachePath = GetCachePath(directories, itemId);

        if (File.Exists(cachePath))
        {
            return (true, false, cachePath);
        }

        var generationLock = _generationLocks.GetOrAdd(itemId, static _ => new SemaphoreSlim(1, 1));
        await generationLock.WaitAsync(cancellationToken);

        try
        {
            if (File.Exists(cachePath))
            {
                return (true, false, cachePath);
            }

            using var image = artService.GetArt(itemId);

            if (image is null)
            {
                return (false, false, cachePath);
            }

            using var normalized = ItemImageNormalizer.CropAndPad(image);
            var tempPath = $"{cachePath}.{Guid.NewGuid():N}.tmp";

            try
            {
                await using (var stream = File.Create(tempPath))
                {
                    await normalized.SaveAsync(stream, new PngEncoder(), cancellationToken);
                }

                File.Move(tempPath, cachePath, true);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }

            return (true, true, cachePath);
        }
        finally
        {
            generationLock.Release();
        }
    }

    private static bool TryParseHexItemId(string itemIdText, out int itemId)
    {
        itemId = 0;

        if (string.IsNullOrWhiteSpace(itemIdText))
        {
            return false;
        }

        var value = itemIdText.Trim();

        if (!value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var parsed = int.TryParse(
            value.AsSpan(2),
            NumberStyles.AllowHexSpecifier,
            CultureInfo.InvariantCulture,
            out itemId
        );

        return parsed && itemId >= 0;
    }
}
