namespace Moongate.Server.Services.Timing.Internal;

/// <summary>
///     Internal record stored in the timer wheel. Mutable so the wheel can update
///     SlotIndex, RemainingRounds, Node and Cancelled in place.
/// </summary>
internal sealed class TimerEntry
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Action Callback { get; init; }
    public required TimeSpan Interval { get; init; }
    public required bool Repeat { get; init; }

    public int SlotIndex { get; set; }
    public long RemainingRounds { get; set; }
    public LinkedListNode<TimerEntry>? Node { get; set; }
    public bool Cancelled { get; set; }
}
