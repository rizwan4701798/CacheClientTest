using CacheClientLib = CacheClient;
using TestApp.Helpers;
using TestApp.Modules;
using TestApp.Constants;

namespace TestApp;

class Program
{
    private static ClientStateManager? _stateManager;

    static void Main(string[] args)
    {
        Console.Title = ClientTestConstants.AppTitle;
        
        ConsoleHelper.PrintHeader();
        
        var initialCache = InitializeCache();
        if (initialCache == null)
        {
            ConsoleHelper.PrintError(ClientTestConstants.InitFailed);
            ConsoleHelper.WaitForKey();
            return;
        }

        _stateManager = new ClientStateManager(initialCache);

        RunMainLoop();

        _stateManager.Dispose();
        ConsoleHelper.PrintSuccess(ClientTestConstants.Disposed);
    }

    static CacheClientLib.ICache? InitializeCache()
    {
        try
        {
            ConsoleHelper.PrintInfo(ClientTestConstants.Initializing);
            
            var options = new CacheClientLib.CacheClientOptions
            {
                Host = ClientTestConstants.Localhost,
                Port = ClientTestConstants.DefaultPort,
                TimeoutMilliseconds = ClientTestConstants.DefaultTimeout
            };

            var cache = new CacheClientLib.CacheClient(options);
            cache.Initialize();

            // Subscribe to events
            cache.ItemAdded += (s, e) => ConsoleHelper.PrintEvent(ClientTestConstants.ADDED, e.Key, ConsoleColor.Green);
            cache.ItemUpdated += (s, e) => ConsoleHelper.PrintEvent(ClientTestConstants.UPDATED, e.Key, ConsoleColor.Yellow);
            cache.ItemRemoved += (s, e) => ConsoleHelper.PrintEvent(ClientTestConstants.REMOVED, e.Key, ConsoleColor.Red);
            cache.ItemExpired += (s, e) => ConsoleHelper.PrintEvent(ClientTestConstants.EXPIRED, e.Key, ConsoleColor.Magenta);
            cache.ItemEvicted += (s, e) => ConsoleHelper.PrintEvent(ClientTestConstants.EVICTED, e.Key, ConsoleColor.DarkYellow);

            ConsoleHelper.PrintSuccess(ClientTestConstants.InitSuccess);
            return cache;
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError(string.Format(ClientTestConstants.InitError, ex.Message));
            return null;
        }
    }

    static void RunMainLoop()
    {
        while (true)
        {
            ShowMainMenu();
            var choice = ConsoleHelper.ReadLine(ClientTestConstants.PromptSelectOption);

            try
            {
                if (_stateManager == null) return;

                switch (choice?.ToUpperInvariant())
                {
                    case ClientTestConstants.Option1: ManualCrudTests.Run(_stateManager); break;
                    case ClientTestConstants.Option2: ExpirationTests.Run(_stateManager); break;
                    case ClientTestConstants.Option3: EventNotificationTests.Run(_stateManager); break;
                    case ClientTestConstants.Option4: PerformanceBenchmarks.Run(_stateManager); break;
                    case ClientTestConstants.Option5: StressTests.Run(_stateManager); break;
                    case ClientTestConstants.Option6: InteractiveMode.Run(_stateManager); break;
                    case ClientTestConstants.Option7: MultiClientManager.Run(_stateManager); break;
                    case ClientTestConstants.OptionQ: 
                    case ClientTestConstants.Option0: return;
                    default: ConsoleHelper.PrintWarning(ClientTestConstants.InvalidChoice); break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError(string.Format(ClientTestConstants.ErrorPrefix, ex.Message));
            }

            Console.WriteLine();
        }
    }

    static void ShowMainMenu()
    {
        Console.WriteLine();
        ConsoleHelper.PrintDivider('=');
        ConsoleHelper.PrintCentered($"{ClientTestConstants.MainMenuHeader} (Current: {_stateManager?.CurrentClientName ?? ClientTestConstants.UnknownClient})", ConsoleColor.Cyan);
        ConsoleHelper.PrintDivider('=');
        Console.WriteLine();
        ConsoleHelper.PrintMenuItem(ClientTestConstants.Option1, ClientTestConstants.DescCrud);
        ConsoleHelper.PrintMenuItem(ClientTestConstants.Option2, ClientTestConstants.DescExpiration);
        ConsoleHelper.PrintMenuItem(ClientTestConstants.Option3, ClientTestConstants.DescEvents);
        ConsoleHelper.PrintMenuItem(ClientTestConstants.Option4, ClientTestConstants.DescPerf);
        ConsoleHelper.PrintMenuItem(ClientTestConstants.Option5, ClientTestConstants.DescStress);
        ConsoleHelper.PrintMenuItem(ClientTestConstants.Option6, ClientTestConstants.DescInteractive);
        ConsoleHelper.PrintMenuItem(ClientTestConstants.Option7, ClientTestConstants.DescMultiClient);
        ConsoleHelper.PrintMenuItem(ClientTestConstants.OptionQ, ClientTestConstants.DescQuit);
        Console.WriteLine();
    }
}
