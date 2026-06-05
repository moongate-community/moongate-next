using Xunit;

// The MoonSharp Lua interpreter keeps process-global, non-thread-safe state. Several test classes
// execute Lua scripts; running them in parallel intermittently corrupts that shared state and makes
// Lua/timer tests flaky. Serializing the whole assembly removes the cross-talk reliably.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
