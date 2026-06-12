using System.Collections.Concurrent;
using System.Globalization;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Persistence.Data;
using Moongate.Server.Data.Mobiles;
using Moongate.Server.Utils;
using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Interfaces.Animations;
using SixLabors.ImageSharp.Formats.Png;

namespace Moongate.Server.Extensions.Endpoints;

public static class HairImageEndpointExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _generationLocks = new();

    public static IEndpointRouteBuilder MapMoongateHairImages(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                     "/api/mobiles/hair/{style}.png",
                     (
                         string style,
                         int? hue,
                         int? body,
                         bool? facial,
                         IMobileFigureRenderer renderer,
                         DirectoriesConfig directories,
                         CancellationToken cancellationToken
                     ) => TryParseStyle(style, out var styleId)
                              ? HandleGetHairImageAsync(
                                  styleId,
                                  hue.GetValueOrDefault(),
                                  body,
                                  facial.GetValueOrDefault(),
                                  renderer,
                                  directories,
                                  cancellationToken
                              )
                              : Task.FromResult<IResult>(TypedResults.BadRequest("style must be an integer"))
                 )
                 .WithName("GetHairImage")
                 .WithTags("Mobiles")
                 .WithSummary("Returns a lazily generated PNG of a hair style rendered over a reference body.")
                 .Produces(StatusCodes.Status200OK, contentType: "image/png")
                 .Produces(StatusCodes.Status404NotFound);

        endpoints.MapGet(
                     "/api/admin/hair-styles",
                     (bool? facial, string? search) => HandleListHairStyles(facial.GetValueOrDefault(), search)
                 )
                 .WithName("ListAdminHairStyles")
                 .WithTags("Admin Mobiles")
                 .RequireAuthorization(policy => policy.RequireRole(nameof(UserLevelType.Administrator)))
                 .WithSummary("Returns the selectable hair (or facial-hair) styles.")
                 .Produces<PagedResult<HairStyleSummary>>();

        return endpoints;
    }

    internal static IResult HandleListHairStyles(bool facial, string? search)
    {
        var source = facial ? HairStyleCatalog.Facial : HairStyleCatalog.Hair;

        IEnumerable<HairStyleEntry> entries = source;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            entries = entries.Where(
                entry => entry.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                         || entry.StyleHex.Contains(term, StringComparison.OrdinalIgnoreCase)
                         || entry.Style.ToString(CultureInfo.InvariantCulture).Contains(term)
            );
        }

        var items = entries
                    .Select(
                        entry => new HairStyleSummary(
                            entry.Style,
                            entry.StyleHex,
                            entry.Name,
                            entry.IsFacial,
                            $"/api/mobiles/hair/{entry.Style.ToString(CultureInfo.InvariantCulture)}.png?facial={entry.IsFacial.ToString().ToLowerInvariant()}"
                        )
                    )
                    .ToArray();

        return TypedResults.Ok(new PagedResult<HairStyleSummary>(items, 1, items.Length == 0 ? 1 : items.Length, items.Length));
    }

    internal static async Task<IResult> HandleGetHairImageAsync(
        int style,
        int hue,
        int? body,
        bool facial,
        IMobileFigureRenderer renderer,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(directories);

        var referenceBody = body is > 0 ? body.Value : 400;
        var cachePath = GetCachePath(directories, style, hue, referenceBody, facial);

        if (File.Exists(cachePath))
        {
            return Results.File(cachePath, "image/png");
        }

        var generationLock = _generationLocks.GetOrAdd(cachePath, static _ => new(1, 1));
        await generationLock.WaitAsync(cancellationToken);

        try
        {
            if (File.Exists(cachePath))
            {
                return Results.File(cachePath, "image/png");
            }

            var request = new MobileRenderRequest(
                referenceBody,
                0,
                facial ? 0 : style,
                hue,
                facial ? style : 0,
                hue,
                []
            );

            using var image = renderer.Render(request);

            if (image is null)
            {
                return TypedResults.NotFound();
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

            return Results.File(cachePath, "image/png");
        }
        finally
        {
            generationLock.Release();
        }
    }

    internal static string GetCachePath(DirectoriesConfig directories, int style, int hue, int body, bool facial)
    {
        ArgumentNullException.ThrowIfNull(directories);

        var directory = Path.Combine(directories[DirectoryType.Cache], "images", "hair");
        Directory.CreateDirectory(directory);

        var prefix = facial ? "f" : "h";
        var fileName =
            $"{prefix}{style.ToString(CultureInfo.InvariantCulture)}_{hue.ToString(CultureInfo.InvariantCulture)}_{body.ToString(CultureInfo.InvariantCulture)}.png";

        return Path.Combine(directory, fileName);
    }

    private static bool TryParseStyle(string text, out int style)
    {
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out style);
        }

        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out style);
    }
}
