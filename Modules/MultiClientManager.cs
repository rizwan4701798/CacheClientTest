using TestApp.Helpers;
using CacheClient;
using TestApp.Constants;

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
            ConsoleHelper.PrintCentered($"{ClientTestConstants.MultiClientHeader} (Current: {state.CurrentClientName})", ConsoleColor.Magenta);
            ConsoleHelper.PrintDivider('-');
            Console.WriteLine();
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option1, ClientTestConstants.DescCreateClient);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option2, ClientTestConstants.DescSwitchClient);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option3, ClientTestConstants.DescListClients);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option4, ClientTestConstants.DescBroadcast);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.OptionB, ClientTestConstants.DescBack);
            Console.WriteLine();

            var choice = ConsoleHelper.ReadLine(ClientTestConstants.PromptSelectOption);

            try
            {
                switch (choice?.ToUpperInvariant())
                {
                    case ClientTestConstants.Option1: CreateNewClient(); break;
                    case ClientTestConstants.Option2: SwitchClient(); break;
                    case ClientTestConstants.Option3: ListClients(); break;
                    case ClientTestConstants.Option4: BroadcastMessage(); break;
                    case ClientTestConstants.OptionB: return;
                    default: ConsoleHelper.PrintWarning(ClientTestConstants.InvalidChoice); break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError(string.Format(ClientTestConstants.ErrorPrefix, ex.Message));
            }
        }
    }

    private static void CreateNewClient()
    {
        var name = ConsoleHelper.ReadLine(ClientTestConstants.PromptClientName);
        if (string.IsNullOrWhiteSpace(name))
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.PromptNameEmpty);
            return;
        }

        CreateClientInternal(name);
    }
    
    private static void CreateClientInternal(string name)
    {
        if (_state!.Clients.ContainsKey(name))
        {
            ConsoleHelper.PrintWarning(string.Format(ClientTestConstants.PromptClientExists, name));
            return;
        }

        try
        {
            var options = new CacheClientOptions
            {
                Host = ClientTestConstants.Localhost,
                Port = ClientTestConstants.DefaultPort,
                TimeoutMilliseconds = ClientTestConstants.DefaultTimeout
            };

            var client = new CacheClient.CacheClient(options);
            client.Initialize();
            
        
            client.ItemAdded += (s, e) => ConsoleHelper.PrintEvent($"{ClientTestConstants.ADDED} ({name})", e.Key, ConsoleColor.Green);
            client.ItemUpdated += (s, e) => ConsoleHelper.PrintEvent($"{ClientTestConstants.UPDATED} ({name})", e.Key, ConsoleColor.Yellow);
            client.ItemRemoved += (s, e) => ConsoleHelper.PrintEvent($"{ClientTestConstants.REMOVED} ({name})", e.Key, ConsoleColor.Red);
            client.ItemExpired += (s, e) => ConsoleHelper.PrintEvent($"{ClientTestConstants.EXPIRED} ({name})", e.Key, ConsoleColor.Magenta);
            client.ItemEvicted += (s, e) => ConsoleHelper.PrintEvent($"{ClientTestConstants.EVICTED} ({name})", e.Key, ConsoleColor.DarkYellow);

            _state.AddClient(name, client);
            ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.ClientCreated, name));
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(string.Format(ClientTestConstants.ClientCreateFailed, name, ex.Message));
        }
    }



    private static void SwitchClient()
    {
        ListClients();
        var name = ConsoleHelper.ReadLine(ClientTestConstants.PromptUseClient);
        
        try
        {
            _state!.SwitchClient(name ?? "");
            ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.SwitchedToClient, name));
        }
        catch (KeyNotFoundException)
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.ClientNotFound);
        }
    }

    private static void ListClients()
    {
        Console.WriteLine();
        ConsoleHelper.PrintInfo(ClientTestConstants.ActiveClients);
        foreach (var name in _state!.Clients.Keys)
        {
            var prefix = name == _state.CurrentClientName ? " * " : "   ";
            Console.WriteLine($"{prefix}{name}");
        }
    }

    private static void BroadcastMessage()
    {
        var key = ConsoleHelper.ReadLine(ClientTestConstants.PromptKeySimple);
        var message = ConsoleHelper.ReadLine(ClientTestConstants.PromptMessage);

        ConsoleHelper.PrintInfo(string.Format(ClientTestConstants.BroadcastInfo, _state!.Clients.Count));
        
        foreach (var kvp in _state.Clients)
        {
            try 
            {
                kvp.Value.Update(key!, $"{message} (from {kvp.Key})");
                ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.ClientSentUpdate, kvp.Key));
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintWarning(string.Format(ClientTestConstants.ClientFailed, kvp.Key, ex.Message));
            }
        }
    }
}
