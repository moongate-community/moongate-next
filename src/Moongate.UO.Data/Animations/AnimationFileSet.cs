using Moongate.UO.Data.Files;
using Moongate.UO.Data.Interfaces.Files;

namespace Moongate.UO.Data.Animations;

/// <summary>
///     Owns the <see cref="FileIndex" /> for each animation file (fileType 1 = anim.mul, 2..5 = anim2..anim5)
///     and routes a <see cref="Seek" /> to the right one. Files that are absent build an empty index, so a
///     missing expansion file simply yields <c>null</c> for the bodies routed to it (non-fatal).
/// </summary>
public sealed class AnimationFileSet
{
    private const int AnimIndexLength = 0x40000;
    private const int AnimFileId = 6;

    private readonly FileIndex[] _files; // indexed by fileType; slots 1..5 used, slot 0 unused

    public AnimationFileSet(IUoFileResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        _files = new FileIndex[6];
        _files[1] = Build(resolver, "anim.idx", "anim.mul");
        _files[2] = Build(resolver, "anim2.idx", "anim2.mul");
        _files[3] = Build(resolver, "anim3.idx", "anim3.mul");
        _files[4] = Build(resolver, "anim4.idx", "anim4.mul");
        _files[5] = Build(resolver, "anim5.idx", "anim5.mul");
    }

    public Stream? Seek(int fileType, int index, out int length)
    {
        length = 0;

        if (fileType < 1 || fileType >= _files.Length)
        {
            return null;
        }

        return _files[fileType].Seek(index, out length, out _, out _);
    }

    private static FileIndex Build(IUoFileResolver resolver, string idxName, string mulName)
    {
        return new FileIndex(
            resolver.Resolve(idxName),
            resolver.Resolve(mulName),
            AnimIndexLength,
            AnimFileId,
            new NullVerdataPatchSource()
        );
    }
}
