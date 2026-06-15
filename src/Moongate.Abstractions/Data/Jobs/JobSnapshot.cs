using Moongate.Abstractions.Types.Jobs;

namespace Moongate.Abstractions.Data.Jobs;

/// <summary>Read-only view of a registered job and its latest run metadata.</summary>
public sealed record JobSnapshot(
    string Id,
    string Name,
    string? Description,
    JobSourceType Source,
    double IntervalMs,
    bool Repeat,
    DateTimeOffset? NextRunAt,
    DateTimeOffset? LastRunAt,
    double? LastDurationMs,
    JobStatusType LastStatus,
    string? LastError,
    long RunCount
);
