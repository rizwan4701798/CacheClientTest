using TestApp.Helpers;
using CacheClient;

namespace TestApp.Modules;

public static class MultiClientManager
{
    private static ClientStateManager? _state;

    public static void Run(ClientStateManager state)
    {
        _state = state;
        while (true)
        {
            Console.WriteLine();
            ConsoleHelper.PrintDivider('-');
            ConsoleHelper.PrintCentered($"MULTI-CLIENT MANAGER (Current: {state.CurrentClientName})", ConsoleColor.Magenta);
            ConsoleHelper.PrintDivider('-');
            Console.WriteLine();
            ConsoleHelper.PrintMenuItem("1", "Create New Client");
            ConsoleHelper.PrintMenuItem("2", "Switch Current Client");
            ConsoleHelper.PrintMenuItem("3", "List Active Clients");
            ConsoleHelper.PrintMenuItem("4", "Broadcast Message");
            ConsoleHelper.PrintMenuItem("5", "Bulk Create Clients");
            ConsoleHelper.PrintMenuItem("6", "Bulk Add Operation");
            ConsoleHelper.PrintMenuItem("B", "Back to Main Menu");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadLine("Select option");

            try
            {
                switch (choice?.ToUpperInvariant())
                {
                    case "1": CreateNewClient(); break;
                    case "2": SwitchClient(); break;
                    case "3": ListClients(); break;
                    case "4": BroadcastMessage(); break;
                    case "5": BulkCreateClients(); break;
                    case "6": BulkAddOperation(); break;
                    case "B": return;
                    default: ConsoleHelper.PrintWarning("Invalid choice."); break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        }
    }

    private static void CreateNewClient()
    {
        var name = ConsoleHelper.ReadLine("Enter unique client name");
        if (string.IsNullOrWhiteSpace(name))
        {
            ConsoleHelper.PrintWarning("Name cannot be empty.");
            return;
        }

        CreateClientInternal(name);
    }
    
    private static void CreateClientInternal(string name)
    {
        if (_state!.Clients.ContainsKey(name))
        {
            ConsoleHelper.PrintWarning($"Client '{name}' already exists.");
            return;
        }

        try
        {
            var options = new CacheClientOptions
            {
                Host = "localhost",
                Port = 5050,
                TimeoutMilliseconds = 5000
            };

            var client = new CacheClient.CacheClient(options);
            client.Initialize();
            
            // Auto sub to events for visibility
            client.ItemAdded += (s, e) => ConsoleHelper.PrintEvent($"ADDED ({name})", e.Key, ConsoleColor.Green);
            client.ItemUpdated += (s, e) => ConsoleHelper.PrintEvent($"UPDATED ({name})", e.Key, ConsoleColor.Yellow);
            client.ItemRemoved += (s, e) => ConsoleHelper.PrintEvent($"REMOVED ({name})", e.Key, ConsoleColor.Red);

            _state.AddClient(name, client);
            ConsoleHelper.PrintSuccess($"Client '{name}' created and connected.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError($"Failed to create client '{name}': {ex.Message}");
        }
    }

    private static void BulkCreateClients()
    {
        var countStr = ConsoleHelper.ReadLine("Enter number of clients to create");
        var prefix = ConsoleHelper.ReadLine("Enter name prefix (e.g. 'LoadClient')");
        
        if (!int.TryParse(countStr, out var count) || count <= 0)
        {
             ConsoleHelper.PrintWarning("Invalid count.");
             return;
        }
        
        if (string.IsNullOrWhiteSpace(prefix)) prefix = "Client";
        
        ConsoleHelper.PrintInfo($"Creating {count} clients...");
        
        for (int i = 1; i <= count; i++)
        {
            CreateClientInternal($"{prefix}_{i}");
        }
        
        ConsoleHelper.PrintSuccess("Bulk creation complete.");
    }

    private static void BulkAddOperation()
    {
        var keyPrefix = ConsoleHelper.ReadLine("Enter key prefix (e.g. 'bulk:item')");
        if (string.IsNullOrWhiteSpace(keyPrefix)) keyPrefix = "bulk:item";
        
        ConsoleHelper.PrintInfo($"Sending ADD requests from all {_state!.Clients.Count} clients...");
        
        int success = 0;
        int fail = 0;
        
        foreach (var kvp in _state.Clients)
        {
            try
            {
                // Unique key per client to avoid locking/overwriting issues being the primary test
                // or same key? The prompt said "send add to cache request to all of those client"
                // Assuming distinct items usually makes sense for load, but collisions are also valid.
                // Let's use unique keys to ensure clean Create.
                var key = $"{keyPrefix}:{kvp.Key}"; 
                kvp.Value.Add(key, $"Value from {kvp.Key}");
                success++;
            }
            catch (Exception ex) 
            {
                ConsoleHelper.PrintError($"Client {kvp.Key} failed: {ex.Message}");
                fail++;
            }
        }
        
        ConsoleHelper.PrintSuccess($"Bulk Add Completed. Success: {success}, Failed: {fail}");
    }

    private static void SwitchClient()
    {
        ListClients();
        var name = ConsoleHelper.ReadLine("Enter client name to use");
        
        try
        {
            _state!.SwitchClient(name ?? "");
            ConsoleHelper.PrintSuccess($"Switched to client '{name}'.");
        }
        catch (KeyNotFoundException)
        {
            ConsoleHelper.PrintWarning("Client not found.");
        }
    }

    private static void ListClients()
    {
        Console.WriteLine();
        ConsoleHelper.PrintInfo("Active Clients:");
        foreach (var name in _state!.Clients.Keys)
        {
            var prefix = name == _state.CurrentClientName ? " * " : "   ";
            Console.WriteLine($"{prefix}{name}");
        }
    }

    private static void BroadcastMessage()
    {
        var key = ConsoleHelper.ReadLine("Enter cache key");
        var message = ConsoleHelper.ReadLine("Enter message");

        ConsoleHelper.PrintInfo($"Broadcasting using all {_state!.Clients.Count} clients...");
        
        foreach (var kvp in _state.Clients)
        {
            try 
            {
                // Each client updates the same key, effectively "chatting" or overwriting
                kvp.Value.Update(key!, $"{message} (from {kvp.Key})");
                ConsoleHelper.PrintSuccess($"Client {kvp.Key} sent update.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintWarning($"Client {kvp.Key} failed: {ex.Message}");
            }
        }
    }
}
