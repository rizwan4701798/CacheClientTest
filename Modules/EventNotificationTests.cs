using TestApp.Helpers;
using TestApp.Models;
using CacheClient;
using CacheClient.Models;

namespace TestApp.Modules;

public static class EventNotificationTests
{
    public static void Run(ClientStateManager state)
    {
        var cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        ConsoleHelper.PrintCentered("EVENT NOTIFICATION TESTS", ConsoleColor.Green);
        ConsoleHelper.PrintDivider('-');
        Console.WriteLine();

        ConsoleHelper.PrintInfo("Subscribing to cache events...");

        try
        {
            cache.Subscribe(
                CacheEventType.ItemAdded,
                CacheEventType.ItemUpdated,
                CacheEventType.ItemRemoved,
                CacheEventType.ItemExpired,
                CacheEventType.ItemEvicted);

            ConsoleHelper.PrintSuccess("Subscribed to all cache events!");
            Console.WriteLine();
            ConsoleHelper.PrintInfo("Now performing operations. Watch for event notifications:");
            Console.WriteLine();

            // Perform operations that trigger events
            var testKey = $"eventtest:{Guid.NewGuid():N}";

            Thread.Sleep(500);
            ConsoleHelper.PrintInfo("  -> Adding item...");
            cache.Add(testKey, new Product { Id = 1, Name = "Event Test Product", Price = 10, Description = "Test" });

            Thread.Sleep(500);
            ConsoleHelper.PrintInfo("  -> Updating item...");
            cache.Update(testKey, new Product { Id = 1, Name = "Updated Event Test", Price = 20, Description = "Updated" });

            Thread.Sleep(500);
            ConsoleHelper.PrintInfo("  -> Removing item...");
            cache.Remove(testKey);

            Thread.Sleep(500);
            ConsoleHelper.PrintInfo("  -> Adding item with short expiration (2s)...");
            cache.Add($"{testKey}:exp", new Product { Id = 2, Name = "Expiring", Price = 5, Description = "Will expire" }, 2);

            ConsoleHelper.PrintInfo("  -> Waiting for expiration...");
            Thread.Sleep(3000);
            cache.Get($"{testKey}:exp"); // Trigger expiration check

            Console.WriteLine();
            ConsoleHelper.PrintSuccess("Event notification test completed!");
            ConsoleHelper.PrintInfo("Note: Events are displayed in real-time as they occur.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError($"Event test failed: {ex.Message}");
        }
        finally
        {
            try { cache.Unsubscribe(); } catch { }
        }
    }
}
