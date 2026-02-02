using System.Diagnostics;
using TestApp.Helpers;
using TestApp.Models;
using CacheClient;
using TestApp.Constants;

namespace TestApp.Modules;

public static class PerformanceBenchmarks
{
    private static ICache? _cache;

    public static void Run(ClientStateManager state)
    {
        _cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        ConsoleHelper.PrintCentered(ClientTestConstants.PerfHeader, ConsoleColor.Blue);
        ConsoleHelper.PrintDivider('-');
        Console.WriteLine();

        var iterationsStr = ConsoleHelper.ReadLine(ClientTestConstants.PromptIterations) ?? "1000";
        if (!int.TryParse(iterationsStr, out var iterations) || iterations <= 0)
            iterations = 1000;

        Console.WriteLine();

        // Write benchmark
        ConsoleHelper.PrintInfo(string.Format(ClientTestConstants.RunWriteBench, iterations));
        var writeResults = RunBenchmark("WRITE", iterations, i =>
        {
            var key = $"perf:write:{i}";
            _cache.Add(key, new Product { Id = i, Name = $"Perf Test {i}", Price = i, Description = "Benchmark" });
        });
        PrintBenchmarkResult("WRITE", writeResults, iterations);

        // Read benchmark
        ConsoleHelper.PrintInfo(string.Format(ClientTestConstants.RunReadBench, iterations));
        var readResults = RunBenchmark("READ", iterations, i =>
        {
            var key = $"perf:write:{i % iterations}";
            _ = _cache.Get(key);
        });
        PrintBenchmarkResult("READ", readResults, iterations);

        // Update benchmark
        ConsoleHelper.PrintInfo(string.Format(ClientTestConstants.RunUpdateBench, iterations));
        var updateResults = RunBenchmark("UPDATE", iterations, i =>
        {
            var key = $"perf:write:{i % iterations}";
            _cache.Update(key, new Product { Id = i, Name = $"Updated {i}", Price = i * 2, Description = "Updated" });
        });
        PrintBenchmarkResult("UPDATE", updateResults, iterations);

        // Mixed workload
        ConsoleHelper.PrintInfo(string.Format(ClientTestConstants.RunMixedBench, iterations));
        var random = new Random();
        var mixedResults = RunBenchmark("MIXED", iterations, i =>
        {
            var key = $"perf:write:{random.Next(iterations)}";
            var op = random.Next(3);
            switch (op)
            {
                case 0: try { _cache.Add($"perf:mixed:{i}", new Product { Id = i, Name = "Mixed", Price = i, Description = "Test" }); } catch { } break;
                case 1: _ = _cache.Get(key); break;
                case 2: try { _cache.Update(key, new Product { Id = i, Name = "Mixed Update", Price = i, Description = "Test" }); } catch { } break;
            }
        });
        PrintBenchmarkResult("MIXED", mixedResults, iterations);

        // Cleanup
        ConsoleHelper.PrintInfo(ClientTestConstants.CleaningBench);
        for (int i = 0; i < iterations; i++)
        {
            try { _cache.Remove($"perf:write:{i}"); } catch { }
            try { _cache.Remove($"perf:mixed:{i}"); } catch { }
        }

        Console.WriteLine();
        ConsoleHelper.PrintSuccess(ClientTestConstants.PerfCompleted);
    }

    private static (TimeSpan elapsed, double opsPerSecond) RunBenchmark(string name, int iterations, Action<int> operation)
    {
        var sw = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            try { operation(i); } catch { }
        }

        sw.Stop();
        var opsPerSecond = iterations / sw.Elapsed.TotalSeconds;
        return (sw.Elapsed, opsPerSecond);
    }

    private static void PrintBenchmarkResult(string name, (TimeSpan elapsed, double opsPerSecond) result, int iterations)
    {
        var color = result.opsPerSecond > 1000 ? ConsoleColor.Green :
                    result.opsPerSecond > 500 ? ConsoleColor.Yellow : ConsoleColor.Red;

        Console.WriteLine();
        Console.Write($"  {name,-10} | ");
        Console.Write($"Time: {result.elapsed.TotalMilliseconds,8:F2}ms | ");
        Console.Write($"Ops/sec: ");
        ConsoleHelper.WriteColored($"{result.opsPerSecond,10:F2}", color);
        Console.Write($" | Avg: {result.elapsed.TotalMilliseconds / iterations:F3}ms/op");
        Console.WriteLine();
    }
}
