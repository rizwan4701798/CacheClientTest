using CacheClient;
using TestApp.Constants;

namespace TestApp.Helpers;

public class ClientStateManager(ICache initialCache) : IDisposable
{
    private readonly Dictionary<string, ICache> _clients = new() { [ClientTestConstants.DefaultClientName] = initialCache };

    public ICache CurrentClient { get; private set; } = initialCache;
    public string CurrentClientName { get; private set; } = ClientTestConstants.DefaultClientName;

    public IReadOnlyDictionary<string, ICache> Clients => _clients;

    public void AddClient(string name, ICache client)
    {
        if (_clients.ContainsKey(name))
            throw new ArgumentException(string.Format(ClientTestConstants.PromptClientExists, name));
            
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
            // The constant is "Client not found." which is simple. 
            // The code had $"Client '{name}' not found". 
            // I'll stick to the constant message unless I update constant to take arg.
            // I'll update the constant to just "Client not found" message or use string interpolation if the constant supported it.
            // My constant ClientNotFound = "Client not found."
            // So: throw new KeyNotFoundException(ClientTestConstants.ClientNotFound);
            throw new KeyNotFoundException(ClientTestConstants.ClientNotFound);
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
