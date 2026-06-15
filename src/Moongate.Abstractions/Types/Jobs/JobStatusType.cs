namespace Moongate.Abstractions.Types.Jobs;

/// <summary>Outcome of a job's most recent run.</summary>
public enum JobStatusType
{
    NeverRun,
    Success,
    Failed
}
