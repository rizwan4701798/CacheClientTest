using CacheClient;

namespace TestApp.Helpers;

public class ClientStateManager(ICache initialCache) : IDisposable
{
    private readonly Dictionary<string, ICache> _clients = new() { ["Default"] = initialCache };

    public ICache CurrentClient { get; private set; } = initialCache;
    public string CurrentClientName { get; private set; } = "Default";

    public IReadOnlyDictionary<string, ICache> Clients => _clients;

    public void AddClient(string name, ICache client)
    {
        if (_clients.ContainsKey(name))
            throw new ArgumentException($"Client {name} already exists.");
            
        _clients[name] = client;
    }
    
    public void SwitchClient(string name)
    {
        if (_clients.TryGetValue(name, out var client))
        {
            CurrentClient = client;
            CurrentClientName = name;
        }
        else
        {
            throw new KeyNotFoundException($"Client '{name}' not found");
        }
    }

    public void Dispose()
    {
         foreach (var client in _clients.Values.Distinct())
         {
             try { client.Dispose(); } catch {}
         }
         _clients.Clear();
    }
}
