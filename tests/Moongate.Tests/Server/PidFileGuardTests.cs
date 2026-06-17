using Moongate.Server.Services.Diagnostics;

namespace Moongate.Tests.Server;

public sealed class PidFileGuardTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nr-pid-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Acquire_ReplacesStalePid()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "moongate.pid"), "456");

        using var guard = PidFileGuard.Acquire(_root, static () => 123, static _ => false);

        Assert.Equal("123", File.ReadAllText(Path.Combine(_root, "moongate.pid")).Trim());
    }

    [Fact]
    public void Acquire_ThrowsWhenExistingPidIsAlive()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "moongate.pid"), "456");

        var ex = Assert.Throws<InvalidOperationException>(() => PidFileGuard.Acquire(
                _root,
                static () => 123,
                static pid => pid == 456
            )
        );

        Assert.Contains("PID 456", ex.Message);
    }

    [Fact]
    public void Acquire_WritesPidFileInRootDirectory()
    {
        using var guard = PidFileGuard.Acquire(_root, static () => 123, static _ => false);

        var pidPath = Path.Combine(_root, "moongate.pid");

        Assert.Equal("123", File.ReadAllText(pidPath).Trim());
    }

    [Fact]
    public void Dispose_RemovesPidFileOnlyWhenOwnedByCurrentProcess()
    {
        var pidPath = Path.Combine(_root, "moongate.pid");

        using (PidFileGuard.Acquire(_root, static () => 123, static _ => false))
        {
            File.WriteAllText(pidPath, "456");
        }

        Assert.True(File.Exists(pidPath));
        Assert.Equal("456", File.ReadAllText(pidPath).Trim());
    }
}
