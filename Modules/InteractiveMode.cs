using TestApp.Helpers;
using TestApp.Models;
using CacheClient;

namespace TestApp.Modules;

public static class InteractiveMode
{
    private static ICache? _cache;

    public static void Run(ClientStateManager state)
    {
        _cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        ConsoleHelper.PrintCentered("INTERACTIVE MODE", ConsoleColor.Cyan);
        ConsoleHelper.PrintDivider('-');
        Console.WriteLine();
        ConsoleHelper.PrintInfo("Enter commands directly. Type 'help' for available commands, 'exit' to return.");
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
                    default:
                        ConsoleHelper.PrintWarning($"Unknown command: {command}. Type 'help' for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }
        }
    }

    private static void PrintInteractiveHelp()
    {
        Console.WriteLine();
        ConsoleHelper.PrintInfo("Available commands:");
        Console.WriteLine("  add <key> <value>      - Add a string value");
        Console.WriteLine("  addex <key> <ttl> <val>- Add with expiration (ttl in seconds)");
        Console.WriteLine("  get <key>              - Get a value");
        Console.WriteLine("  update <key> <value>   - Update a value");
        Console.WriteLine("  del <key>              - Delete a key");
        Console.WriteLine("  exit                   - Return to main menu");
        Console.WriteLine();
    }

    private static void InteractiveAdd(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
        {
            ConsoleHelper.PrintWarning("Usage: add <key> <value>");
            return;
        }
        _cache!.Add(parts[0], parts[1]);
        ConsoleHelper.PrintSuccess($"OK");
    }

    private static void InteractiveAddWithExpiration(string args)
    {
        var parts = args.Split(' ', 3);
        if (parts.Length < 3 || !int.TryParse(parts[1], out var ttl))
        {
            ConsoleHelper.PrintWarning("Usage: addex <key> <ttl_seconds> <value>");
            return;
        }
        _cache!.Add(parts[0], parts[2], ttl);
        ConsoleHelper.PrintSuccess($"OK (expires in {ttl}s)");
    }

    private static void InteractiveGet(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            ConsoleHelper.PrintWarning("Usage: get <key>");
            return;
        }
        var result = _cache!.Get(args);
        if (result is Product p)
            ConsoleHelper.PrintProductDetails(p);
        else if (result is not null)
            Console.WriteLine($"  {result}");
        else
            ConsoleHelper.PrintWarning("(nil)");
    }

    private static void InteractiveUpdate(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
        {
            ConsoleHelper.PrintWarning("Usage: update <key> <value>");
            return;
        }
        _cache!.Update(parts[0], parts[1]);
        ConsoleHelper.PrintSuccess("OK");
    }

    private static void InteractiveDelete(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            ConsoleHelper.PrintWarning("Usage: del <key>");
            return;
        }
        _cache!.Remove(args);
        ConsoleHelper.PrintSuccess("OK");
    }
}
