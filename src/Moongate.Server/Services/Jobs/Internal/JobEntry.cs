using Moongate.Abstractions.Types.Jobs;

namespace Moongate.Server.Services.Jobs.Internal;

/// <summary>Internal mutable record for a registered job and its latest run metadata.</summary>
internal sealed class JobEntry
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required JobSourceType Source { get; init; }
    public required TimeSpan Interval { get; init; }
    public required bool Repeat { get; init; }
    public required Action Handler { get; init; }

    public string? TimerId { get; set; }
    public long RunCount { get; set; }
    public DateTimeOffset? NextRunAt { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public double? LastDurationMs { get; set; }
    public JobStatusType LastStatus { get; set; } = JobStatusType.NeverRun;
    public string? LastError { get; set; }
}
