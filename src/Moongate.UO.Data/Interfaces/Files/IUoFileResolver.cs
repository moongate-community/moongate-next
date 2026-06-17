namespace Moongate.UO.Data.Interfaces.Files;

/// <summary>
///     Resolves logical Ultima Online client file names (e.g. <c>tiledata.mul</c>) to absolute paths
///     within a configured client-files directory.
/// </summary>
public interface IUoFileResolver
{
    /// <summary>Absolute path of the client-files directory being scanned.</summary>
    string RootDirectory { get; }

    /// <summary>Returns <c>true</c> when <paramref name="fileName" /> resolves to an existing file.</summary>
    /// <param name="fileName">Logical file name, e.g. <c>tiledata.mul</c>.</param>
    bool Contains(string fileName);

    /// <summary>
    ///     Returns the absolute path of the given client file, or <c>null</c> when it is not present
    ///     (or is not a recognised UO client file). Lookup is case-insensitive.
    /// </summary>
    /// <param name="fileName">Logical file name, e.g. <c>tiledata.mul</c>.</param>
    string? Resolve(string fileName);
}
