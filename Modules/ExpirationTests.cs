using System.Diagnostics;
using TestApp.Helpers;
using TestApp.Models;
using CacheClient;

namespace TestApp.Modules;

public static class ExpirationTests
{
    public static void Run(ClientStateManager state)
    {
        var cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        ConsoleHelper.PrintCentered("EXPIRATION TESTS", ConsoleColor.Magenta);
        ConsoleHelper.PrintDivider('-');
        Console.WriteLine();

        ConsoleHelper.PrintInfo("This test will add items with different expiration times and verify they expire correctly.");
        Console.WriteLine();

        var expirations = new[] { 3, 5, 10 };
        var keys = new List<string>();

        foreach (var exp in expirations)
        {
            var key = $"exptest:{exp}s:{Guid.NewGuid():N}";
            keys.Add(key);
            
            var product = new Product
            {
                Id = exp,
                Name = $"Expires in {exp}s",
                Price = exp * 10,
                Description = $"Created at {DateTime.Now:HH:mm:ss.fff}"
            };

            cache.Add(key, product, exp);
            ConsoleHelper.PrintInfo($"Added '{key}' with {exp}s expiration");
        }

        Console.WriteLine();
        ConsoleHelper.PrintInfo("Monitoring expiration...");
        ConsoleHelper.PrintInfo("Press any key to stop monitoring.\n");

        var stopwatch = Stopwatch.StartNew();
        var allExpired = false;

        while (!allExpired && !Console.KeyAvailable)
        {
            Thread.Sleep(1000);
            
            var remaining = keys.Where(k => cache.Get(k) is not null).ToList();
            var expired = keys.Except(remaining).ToList();

            Console.Write($"\r  [{stopwatch.Elapsed:mm\\:ss}] Active: {remaining.Count}, Expired: {expired.Count}    ");

            allExpired = remaining.Count == 0;
        }

        if (Console.KeyAvailable)
            Console.ReadKey(true);

        Console.WriteLine();
        ConsoleHelper.PrintSuccess($"Expiration test completed in {stopwatch.Elapsed:mm\\:ss}");
    }
}
