using System.Collections.Concurrent;
using System.Globalization;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Mobiles;
using Moongate.Server.Utils;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Bodies;
using Moongate.UO.Data.Types.Bodies;
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

        endpoints.MapGet(
                     "/api/admin/bodies",
                     (IBodyDataStore bodies, int? page, int? pageSize, string? search) =>
                         HandleListBodies(bodies, page, pageSize, search)
                 )
                 .WithName("ListAdminBodies")
                 .WithTags("Admin Bodies")
                 .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)))
                 .WithSummary("Returns a paged list of classified UO bodies for the picker.")
                 .Produces<PagedResult<BodySummary>>();

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

    internal static IResult HandleListBodies(IBodyDataStore bodies, int? page, int? pageSize, string? search)
    {
        ArgumentNullException.ThrowIfNull(bodies);

        var pageNumber = page is > 0 ? page.Value : 1;
        var size = pageSize is > 0 and <= 200 ? pageSize.Value : 60;

        IEnumerable<int> ids = bodies.GetClassifiedBodies()
                                     .Where(id => bodies.GetBodyType(id) != UoBodyType.Equipment)
                                     .OrderBy(static id => id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            ids = ids.Where(id => MatchesBody(id, term));
        }

        var ordered = ids.ToArray();

        var items = ordered
                    .Skip((pageNumber - 1) * size)
                    .Take(size)
                    .Select(
                        id => new BodySummary(
                            id,
                            $"0x{id:X4}",
                            bodies.GetBodyType(id).ToString(),
                            $"/api/mobiles/{id.ToString(CultureInfo.InvariantCulture)}.png"
                        )
                    )
                    .ToArray();

        return TypedResults.Ok(new PagedResult<BodySummary>(items, pageNumber, size, ordered.Length));
    }

    private static bool MatchesBody(int id, string term)
    {
        if (term.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(term[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex) && hex == id;
        }

        if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dec))
        {
            return dec == id;
        }

        return $"0x{id:X4}".Contains(term, StringComparison.OrdinalIgnoreCase);
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

        var generationLock = _generationLocks.GetOrAdd(cachePath, static _ => new(1, 1));
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
