using Moongate.Server.Interfaces.Services.World;

namespace Moongate.Server.Services.World.Internal;

/// <summary>
///     Base implementation for world data services that load their backing YAML on demand.
/// </summary>
public abstract class LazyDataService : IDataService
{
    private readonly Lock _loadSync = new();
    private bool _isLoaded;
    private bool _isLoading;

    public bool IsLazy => true;

    public bool IsLoaded
    {
        get
        {
            lock (_loadSync)
            {
                return _isLoaded;
            }
        }
    }

    public void EnsureLoaded()
    {
        lock (_loadSync)
        {
            if (_isLoaded)
            {
                return;
            }

            LoadUnderLock();
        }
    }

    public void Reload()
    {
        lock (_loadSync)
        {
            LoadUnderLock();
        }
    }

    protected abstract void LoadCore();

    protected void MarkLoaded()
    {
        lock (_loadSync)
        {
            if (!_isLoading)
            {
                _isLoaded = true;
            }
        }
    }

    private void LoadUnderLock()
    {
        _isLoading = true;

        try
        {
            LoadCore();
            _isLoaded = true;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
