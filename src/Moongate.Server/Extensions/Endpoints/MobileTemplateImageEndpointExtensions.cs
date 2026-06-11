using System.Collections.Concurrent;
using Moongate.Core.Data.Directories;
using Moongate.Core.Types;
using Moongate.Server.Utils;
using Moongate.UO.Data.Animations;
using Moongate.UO.Data.Interfaces.Animations;
using Moongate.UO.Data.Interfaces.Services;
using SixLabors.ImageSharp.Formats.Png;

namespace Moongate.Server.Extensions.Endpoints;

public static class MobileTemplateImageEndpointExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _generationLocks = new();

    public static IEndpointRouteBuilder MapMoongateMobileTemplateImages(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                     "/api/mobile-templates/{id}/image.png",
                     (
                         string id,
                         IMobileTemplateService templates,
                         IMobileFigureRenderer renderer,
                         DirectoriesConfig directories,
                         CancellationToken cancellationToken
                     ) => HandleGetTemplateImageAsync(id, templates, renderer, directories, cancellationToken)
                 )
                 .WithName("GetMobileTemplateImage")
                 .WithTags("Mobiles")
                 .WithSummary("Returns a lazily generated PNG of the dressed mobile figure (body + hair).")
                 .Produces(StatusCodes.Status200OK, contentType: "image/png")
                 .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    internal static string GetCachePath(DirectoriesConfig directories, string id)
    {
        ArgumentNullException.ThrowIfNull(directories);

        var directory = Path.Combine(directories[DirectoryType.Cache], "images", "mobile-templates");
        Directory.CreateDirectory(directory);

        var safeId = string.Concat(id.Select(c => Array.IndexOf(Path.GetInvalidFileNameChars(), c) >= 0 ? '_' : c));

        return Path.Combine(directory, $"{safeId}.png");
    }

    internal static async Task<IResult> HandleGetTemplateImageAsync(
        string id,
        IMobileTemplateService templates,
        IMobileFigureRenderer renderer,
        DirectoriesConfig directories,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(templates);
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(directories);

        if (string.IsNullOrWhiteSpace(id) || !templates.TryGet(id, out var template))
        {
            return TypedResults.NotFound();
        }

        var cachePath = GetCachePath(directories, id);

        if (File.Exists(cachePath))
        {
            return Results.File(cachePath, "image/png");
        }

        var generationLock = _generationLocks.GetOrAdd(cachePath, static _ => new SemaphoreSlim(1, 1));
        await generationLock.WaitAsync(cancellationToken);

        try
        {
            if (File.Exists(cachePath))
            {
                return Results.File(cachePath, "image/png");
            }

            var request = new MobileRenderRequest(
                template!.Body,
                template.SkinHue,
                template.HairStyle,
                template.HairHue,
                template.FacialHairStyle,
                template.FacialHairHue
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
}
