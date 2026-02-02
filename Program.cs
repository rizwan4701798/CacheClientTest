using CacheClientLib = CacheClient;
using TestApp.Helpers;
using TestApp.Modules;

namespace TestApp;

class Program
{
    private static ClientStateManager? _stateManager;

    static void Main(string[] args)
    {
        Console.Title = "Cache Server Test Application";
        
        ConsoleHelper.PrintHeader();
        
        var initialCache = InitializeCache();
        if (initialCache == null)
        {
            ConsoleHelper.PrintError("Failed to initialize cache client. Ensure the cache server is running.");
            ConsoleHelper.WaitForKey();
            return;
        }

        _stateManager = new ClientStateManager(initialCache);

        RunMainLoop();

        _stateManager.Dispose();
        ConsoleHelper.PrintSuccess("Cache client disposed. Goodbye!");
    }

    static CacheClientLib.ICache? InitializeCache()
    {
        try
        {
            ConsoleHelper.PrintInfo("Initializing cache client...");
            
            var options = new CacheClientLib.CacheClientOptions
            {
                Host = "localhost",
                Port = 5050,
                TimeoutMilliseconds = 5000
            };

            var cache = new CacheClientLib.CacheClient(options);
            cache.Initialize();

            // Subscribe to events
            cache.ItemAdded += (s, e) => ConsoleHelper.PrintEvent("ADDED", e.Key, ConsoleColor.Green);
            cache.ItemUpdated += (s, e) => ConsoleHelper.PrintEvent("UPDATED", e.Key, ConsoleColor.Yellow);
            cache.ItemRemoved += (s, e) => ConsoleHelper.PrintEvent("REMOVED", e.Key, ConsoleColor.Red);
            cache.ItemExpired += (s, e) => ConsoleHelper.PrintEvent("EXPIRED", e.Key, ConsoleColor.Magenta);
            cache.ItemEvicted += (s, e) => ConsoleHelper.PrintEvent("EVICTED", e.Key, ConsoleColor.DarkYellow);

            ConsoleHelper.PrintSuccess("Cache client initialized successfully!");
            return cache;
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError($"Initialization failed: {ex.Message}");
            return null;
        }
    }

    static void RunMainLoop()
    {
        while (true)
        {
            ShowMainMenu();
            var choice = ConsoleHelper.ReadLine("Select option");

            try
            {
                if (_stateManager == null) return;

                switch (choice?.ToUpperInvariant())
                {
                    case "1": ManualCrudTests.Run(_stateManager); break;
                    case "2": ExpirationTests.Run(_stateManager); break;
                    case "3": EventNotificationTests.Run(_stateManager); break;
                    case "4": PerformanceBenchmarks.Run(_stateManager); break;
                    case "5": StressTests.Run(_stateManager); break;
                    case "6": InteractiveMode.Run(_stateManager); break;
                    case "7": MultiClientManager.Run(_stateManager); break;
                    case "Q" or "0": return;
                    default: ConsoleHelper.PrintWarning("Invalid choice. Please try again."); break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    static void ShowMainMenu()
    {
        Console.WriteLine();
        ConsoleHelper.PrintDivider('=');
        ConsoleHelper.PrintCentered($"MAIN MENU (Current: {_stateManager?.CurrentClientName ?? "Unknown"})", ConsoleColor.Cyan);
        ConsoleHelper.PrintDivider('=');
        Console.WriteLine();
        ConsoleHelper.PrintMenuItem("1", "Manual CRUD Operations");
        ConsoleHelper.PrintMenuItem("2", "Expiration Tests");
        ConsoleHelper.PrintMenuItem("3", "Event Notification Tests");
        ConsoleHelper.PrintMenuItem("4", "Performance Benchmarks");
        ConsoleHelper.PrintMenuItem("5", "Stress Tests");
        ConsoleHelper.PrintMenuItem("6", "Interactive Mode");
        ConsoleHelper.PrintMenuItem("7", "Multi-Client Manager");
        ConsoleHelper.PrintMenuItem("Q", "Quit");
        Console.WriteLine();
    }
}
