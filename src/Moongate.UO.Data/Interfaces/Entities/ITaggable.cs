namespace Moongate.UO.Data.Interfaces.Entities;

/// <summary>
/// Opt-in capability for entities that carry an extensible bag of string
/// key/value tags. Use it for plugin- or script-defined data that does not
/// warrant a dedicated strongly-typed field. Values are plain strings to keep
/// the entity serialization-safe; implementations should allocate the backing
/// store lazily so untagged entities pay no memory cost.
/// </summary>
public interface ITaggable
{
    /// <summary>Custom string tags keyed by name.</summary>
    IDictionary<string, string> Tags { get; }

    /// <summary>Returns the tag value for the given key, or null when it is not set.</summary>
    string? GetTag(string key);

    /// <summary>Adds or replaces the tag for the given key.</summary>
    void SetTag(string key, string value);

    /// <summary>Removes the tag for the given key; returns true when one was removed.</summary>
    bool RemoveTag(string key);
}
