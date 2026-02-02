using TestApp.Helpers;
using TestApp.Models;
using CacheClient;
using CacheClient.Models;
using TestApp.Constants;

namespace TestApp.Modules;

public static class EventNotificationTests
{
    public static void Run(ClientStateManager state)
    {
        var cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        {
            try { cache.Unsubscribe(); } catch { }
        }
    }
}
