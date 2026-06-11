using System.Collections.Concurrent;
using System.Globalization;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Data.Mobiles;
using Moongate.Server.Utils;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Bodies;
using SixLabors.ImageSharp.Formats.Png;

namespace Moongate.Server.Extensions.Endpoints;

public static class BodyImageEndpointExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _generationLocks = new();

    public static IEndpointRouteBuilder MapMoongateBodyImages(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                     "/api/mobiles/{body}.png",
                     (
                         string body,
                         int? hue,
                         IAnimationService animationService,
                         DirectoriesConfig directories,
                         CancellationToken cancellationToken
                     ) => HandleGetBodyImageAsync(body, hue, animationService, directories, cancellationToken)
                 )
                 .WithName("GetBodyImage")
                 .WithTags("Mobiles")
                 .WithSummary("Returns a lazily generated PNG image of the specified UO body graphic, optionally hued.")
                 .Produces(StatusCodes.Status200OK, contentType: "image/png")
                 .Produces(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status404NotFound);

        endpoints.MapPost(
                     "/api/mobiles/build",
                     (
                         IAnimationService animationService,
                         IBodyDataStore bodies,
                         DirectoriesConfig directories,
                         CancellationToken cancellationToken
                     ) => HandleBuildBodyImagesAsync(animationService, bodies, directories, cancellationToken)
                 )
                 .WithName("BuildBodyImages")
                 .WithTags("Mobiles")
                 .WithSummary("Generates and caches PNG images for all classified UO bodies.")
                 .Produces<BodyImageBuildResult>()
                 .Produces(StatusCodes.Status200OK);

        return endpoints;
    }

    internal static string GetCachePath(DirectoriesConfig directories, int body, int hue)
    {
        ArgumentNullException.ThrowIfNull(directories);

        var directory = Path.Combine(directories[DirectoryType.Cache], "images", "mobiles");
        Directory.CreateDirectory(directory);

        var fileName = hue == 0
            ? $"{body.ToString(CultureInfo.InvariantCulture)}.png"
            : $"{body.ToString(CultureInfo.InvariantCulture)}_{hue.ToString(CultureInfo.InvariantCulture)}.png";

        return Path.Combine(directory, fileName);
    }

    internal static async Task<IResult> HandleGetBodyImageAsync(
        string bodyText,
        int? hue,
        IAnimationService animationService,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(animationService);
        ArgumentNullException.ThrowIfNull(directories);

        if (!TryParseBody(bodyText, out var body))
        {
            return TypedResults.BadRequest("body must be a non-negative integer");
        }

        var effectiveHue = hue.GetValueOrDefault() > 0 ? hue.GetValueOrDefault() : 0;

        var result = await EnsureBodyImageAsync(body, effectiveHue, animationService, directories, cancellationToken);

        return result.HasImage
                   ? Results.File(result.CachePath, "image/png")
                   : TypedResults.NotFound();
    }

    internal static async Task<IResult> HandleBuildBodyImagesAsync(
        IAnimationService animationService,
        IBodyDataStore bodies,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(animationService);
        ArgumentNullException.ThrowIfNull(bodies);
        ArgumentNullException.ThrowIfNull(directories);

        var classified = bodies.GetClassifiedBodies();
        var generated = 0;
        var cached = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var body in classified)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await EnsureBodyImageAsync(body, 0, animationService, directories, cancellationToken);

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
            new BodyImageBuildResult
            {
                TotalBodies = classified.Count,
                Generated = generated,
                Cached = cached,
                Skipped = skipped,
                Failed = failed
            }
        );
    }

    private static async Task<(bool HasImage, bool Generated, string CachePath)> EnsureBodyImageAsync(
        int body,
        int hue,
        IAnimationService animationService,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        var cachePath = GetCachePath(directories, body, hue);

        if (File.Exists(cachePath))
        {
            return (true, false, cachePath);
        }

        var generationLock = _generationLocks.GetOrAdd(cachePath, static _ => new SemaphoreSlim(1, 1));
        await generationLock.WaitAsync(cancellationToken);

        try
        {
            if (File.Exists(cachePath))
            {
                return (true, false, cachePath);
            }

            using var image = animationService.GetBodyFrame(body, hue: hue);

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

    private static bool TryParseBody(string bodyText, out int body)
    {
        body = 0;

        if (string.IsNullOrWhiteSpace(bodyText))
        {
            return false;
        }

        return int.TryParse(bodyText.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out body) && body >= 0;
    }
}
