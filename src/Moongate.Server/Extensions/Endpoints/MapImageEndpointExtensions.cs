using System.Collections.Concurrent;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.UO.Data.Interfaces.Maps;
using SixLabors.ImageSharp.Formats.Png;

namespace Moongate.Server.Extensions.Endpoints;

public static class MapImageEndpointExtensions
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _generationLocks = new();

    public static IEndpointRouteBuilder MapMoongateMapImages(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                     "/api/maps/{mapId:int}.png",
                     (
                         int mapId,
                         IMapImageService mapImages,
                         DirectoriesConfig directories,
                         CancellationToken cancellationToken
                     ) => HandleGetMapImageAsync(mapId, mapImages, directories, cancellationToken)
                 )
                 .WithName("GetMapImage")
                 .WithTags("Maps")
                 .WithSummary("Returns a lazily generated radar-colour PNG image of the specified UO map.")
                 .Produces(StatusCodes.Status200OK, contentType: "image/png")
                 .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    internal static async Task<IResult> HandleGetMapImageAsync(
        int mapId,
        IMapImageService mapImages,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(mapImages);
        ArgumentNullException.ThrowIfNull(directories);

        var cachePath = GetCachePath(directories, mapId);

        if (File.Exists(cachePath))
        {
            return Results.File(cachePath, "image/png");
        }

        var generationLock = _generationLocks.GetOrAdd(mapId, static _ => new(1, 1));
        await generationLock.WaitAsync(cancellationToken);

        try
        {
            if (File.Exists(cachePath))
            {
                return Results.File(cachePath, "image/png");
            }

            using var image = mapImages.GetMapImage(mapId);

            if (image is null)
            {
                return TypedResults.NotFound();
            }

            var tempPath = $"{cachePath}.{Guid.NewGuid():N}.tmp";

            try
            {
                await using (var stream = File.Create(tempPath))
                {
                    await image.SaveAsync(stream, new PngEncoder(), cancellationToken);
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

            return Results.File(cachePath, "image/png");
        }
        finally
        {
            generationLock.Release();
        }
    }

    internal static string GetCachePath(DirectoriesConfig directories, int mapId)
    {
        ArgumentNullException.ThrowIfNull(directories);

        var directory = Path.Combine(directories[DirectoryType.Cache], "images", "maps");
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, $"{mapId}.png");
    }
}
