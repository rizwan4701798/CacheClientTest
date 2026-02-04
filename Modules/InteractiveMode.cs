using TestApp.Helpers;
using TestApp.Models;
using CacheClient;
using CacheClient.Models;
using TestApp.Constants;

namespace TestApp.Modules;

public static class InteractiveMode
{
    private static ICache? _cache;

    public static void Run(ClientStateManager state)
    {
        _cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        ConsoleHelper.PrintCentered(ClientTestConstants.InteractiveHeader, ConsoleColor.Cyan);
        ConsoleHelper.PrintDivider('-');
        Console.WriteLine();
        ConsoleHelper.PrintInfo(ClientTestConstants.InteractiveInfo);
        Console.WriteLine();

        while (true)
        {
            Console.Write("cache> ");
            Console.ForegroundColor = ConsoleColor.White;
            var input = Console.ReadLine()?.Trim();
            Console.ResetColor();

            if (string.IsNullOrEmpty(input)) continue;

            var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLowerInvariant();
            var args = parts.Length > 1 ? parts[1] : "";

            try
            {
                switch (command)
                {
                    case "help":
                        PrintInteractiveHelp();
                        break;
                    case "exit" or "quit":
                        return;
                    case "add" or "set":
                        InteractiveAdd(args);
                        break;
                    case "get":
                        InteractiveGet(args);
                        break;
                    case "del" or "delete" or "remove":
                        InteractiveDelete(args);
                        break;
                    case "update":
                        InteractiveUpdate(args);
                        break;
                    case "addex":
                        InteractiveAddWithExpiration(args);
                        break;
                    case "subscribe" or "sub":
                        InteractiveSubscribe(args);
                        break;
                    case "unsubscribe" or "unsub":
                        InteractiveUnsubscribe(args);
                        break;
                    default:
                        ConsoleHelper.PrintWarning(string.Format(ClientTestConstants.UnknownCommand, command));
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError(string.Format(ClientTestConstants.ErrorPrefix, ex.Message));
            }
        }
    }

    private static void PrintInteractiveHelp()
    {
        Console.WriteLine();
        ConsoleHelper.PrintInfo(ClientTestConstants.AvailableCommands);
        Console.WriteLine("  add <key> <value>      - Add a string value");
        Console.WriteLine("  addex <key> <ttl> <val>- Add with expiration (ttl in seconds)");
        Console.WriteLine("  get <key>              - Get a value");
        Console.WriteLine("  update <key> <value>   - Update a value");
        Console.WriteLine("  del <key>              - Delete a key");
        Console.WriteLine("  subscribe <events>     - Subscribe to events (e.g. ItemAdded ItemRemoved)");
        Console.WriteLine("  unsubscribe [events]   - Unsubscribe from events (empty for all)");
        Console.WriteLine("  exit                   - Return to main menu");
        Console.WriteLine();
    }

    private static void InteractiveAdd(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.UsageAdd);
            return;
        }
        _cache!.Add(parts[0], parts[1]);
        ConsoleHelper.PrintSuccess(ClientTestConstants.OK);
    }

    private static void InteractiveAddWithExpiration(string args)
    {
        var parts = args.Split(' ', 3);
        if (parts.Length < 3 || !int.TryParse(parts[1], out var ttl))
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.UsageAddEx);
            return;
        }
        _cache!.Add(parts[0], parts[2], ttl);
        ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.OKExpires, ttl));
    }

    private static void InteractiveGet(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.UsageGet);
            return;
        }
        var result = _cache!.Get(args);
        if (result is Product p)
            ConsoleHelper.PrintProductDetails(p);
        else if (result is not null)
            Console.WriteLine($"  {result}");
        else
            ConsoleHelper.PrintWarning(ClientTestConstants.Nil);
    }

    private static void InteractiveUpdate(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.UsageUpdate);
            return;
        }
        _cache!.Update(parts[0], parts[1]);
        ConsoleHelper.PrintSuccess(ClientTestConstants.OK);
    }

    private static void InteractiveDelete(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.UsageDel);
            return;
        }
        _cache!.Remove(args);
        ConsoleHelper.PrintSuccess(ClientTestConstants.OK);
    }

    private static void InteractiveSubscribe(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
             ConsoleHelper.PrintWarning("Usage: subscribe <event1> <event2> ...");
             return;
        }

        try
        {
            var events = args.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => Enum.Parse<CacheEventType>(s, true))
                             .ToArray();
            _cache!.Subscribe(events);
            ConsoleHelper.PrintSuccess("Subscribed to " + string.Join(", ", events));
        }
        catch (Exception)
        {
             ConsoleHelper.PrintError("Invalid event types.");
        }
    }

    private static void InteractiveUnsubscribe(string args)
    {
        try
        {
            var events = string.IsNullOrWhiteSpace(args) 
                ? Array.Empty<CacheEventType>()
                : args.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => Enum.Parse<CacheEventType>(s, true))
                      .ToArray();
            
             _cache!.Unsubscribe(events);
             
             if (events.Length == 0)
                 ConsoleHelper.PrintSuccess("Unsubscribed from ALL events.");
             else
                 ConsoleHelper.PrintSuccess("Unsubscribed from " + string.Join(", ", events));
        }
        catch (Exception)
        {
             ConsoleHelper.PrintError("Invalid event types.");
        }
    }
}
