using Moongate.UO.Data.Types.Bodies;

namespace Moongate.UO.Data.Interfaces.Bodies;

/// <summary>Provides the body-id → <see cref="UoBodyType" /> classification table.</summary>
public interface IBodyDataStore
{
    /// <summary>Number of classified body ids.</summary>
    int Count { get; }

    /// <summary>Returns the body type for <paramref name="bodyId" />, or <see cref="UoBodyType.Empty" />.</summary>
    /// <param name="bodyId">Body graphic id.</param>
    UoBodyType GetBodyType(int bodyId);

    /// <summary>Returns every body id whose classification is not <see cref="UoBodyType.Empty" />.</summary>
    IReadOnlyCollection<int> GetClassifiedBodies();
}
