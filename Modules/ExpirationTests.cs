using System.Diagnostics;
using TestApp.Helpers;
using TestApp.Models;
using CacheClient;
using TestApp.Constants;

namespace TestApp.Modules;

public static class ExpirationTests
{
    public static void Run(ClientStateManager state)
    {
        var cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        ConsoleHelper.PrintCentered(ClientTestConstants.ExpirationHeader, ConsoleColor.Magenta);
        ConsoleHelper.PrintDivider('-');
        Console.WriteLine();

        ConsoleHelper.PrintInfo(ClientTestConstants.ExpTestInfo);
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
            ConsoleHelper.PrintInfo(string.Format(ClientTestConstants.ExpAdded, key, exp));
        }

        Console.WriteLine();
        ConsoleHelper.PrintInfo(ClientTestConstants.MonitoringExp);
        ConsoleHelper.PrintInfo(ClientTestConstants.StopMonitoring);

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
        ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.ExpCompleted, stopwatch.Elapsed));
    }
}
