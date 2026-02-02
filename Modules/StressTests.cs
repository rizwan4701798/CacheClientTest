using System.Diagnostics;
using TestApp.Helpers;
using TestApp.Models;
using CacheClient;

namespace TestApp.Modules;

public static class StressTests
{
    private static ICache? _cache;

    public static void Run(ClientStateManager state)
    {
        _cache = state.CurrentClient;
        Console.WriteLine();
        ConsoleHelper.PrintDivider('-');
        ConsoleHelper.PrintCentered("STRESS TESTS", ConsoleColor.Red);
        ConsoleHelper.PrintDivider('-');
        Console.WriteLine();

        ConsoleHelper.PrintMenuItem("1", "Concurrent Writers Test");
        ConsoleHelper.PrintMenuItem("2", "Concurrent Readers Test");
        ConsoleHelper.PrintMenuItem("3", "Mixed Concurrent Operations");
        ConsoleHelper.PrintMenuItem("4", "Rapid Fire Test");
        ConsoleHelper.PrintMenuItem("B", "Back");
        Console.WriteLine();

        var choice = ConsoleHelper.ReadLine("Select test");

        switch (choice?.ToUpperInvariant())
        {
            case "1": ConcurrentWritersTest(); break;
            case "2": ConcurrentReadersTest(); break;
            case "3": MixedConcurrentTest(); break;
            case "4": RapidFireTest(); break;
        }
    }

    private static void ConcurrentWritersTest()
    {
        var threadsStr = ConsoleHelper.ReadLine("Enter number of threads (default: 10)") ?? "10";
        var opsStr = ConsoleHelper.ReadLine("Enter operations per thread (default: 100)") ?? "100";

        if (!int.TryParse(threadsStr, out var threads)) threads = 10;
        if (!int.TryParse(opsStr, out var ops)) ops = 100;

        Console.WriteLine();
        ConsoleHelper.PrintInfo($"Running {threads} concurrent writer threads, {ops} ops each...");

        var errors = 0;
        var completed = 0;
        var sw = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, threads).Select(t => Task.Run(() =>
        {
            for (int i = 0; i < ops; i++)
            {
                try
                {
                    var key = $"stress:writer:{t}:{i}";
                    _cache!.Add(key, new Product { Id = i, Name = $"Stress {t}-{i}", Price = i, Description = "Stress test" });
                    Interlocked.Increment(ref completed);
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }
            }
        })).ToArray();

        Task.WaitAll(tasks);
        sw.Stop();

        PrintStressResult(threads * ops, completed, errors, sw.Elapsed);

        // Cleanup
        ConsoleHelper.PrintInfo("Cleaning up...");
        Parallel.For(0, threads, t =>
        {
            for (int i = 0; i < ops; i++)
                try { _cache!.Remove($"stress:writer:{t}:{i}"); } catch { }
        });
    }

    private static void ConcurrentReadersTest()
    {
        var threadsStr = ConsoleHelper.ReadLine("Enter number of threads (default: 20)") ?? "20";
        var opsStr = ConsoleHelper.ReadLine("Enter operations per thread (default: 500)") ?? "500";

        if (!int.TryParse(threadsStr, out var threads)) threads = 20;
        if (!int.TryParse(opsStr, out var ops)) ops = 500;

        // Pre-populate
        ConsoleHelper.PrintInfo("Pre-populating cache with test data...");
        for (int i = 0; i < 100; i++)
        {
            try { _cache!.Add($"stress:read:{i}", new Product { Id = i, Name = $"Read Test {i}", Price = i, Description = "Test" }); } catch { }
        }

        Console.WriteLine();
        ConsoleHelper.PrintInfo($"Running {threads} concurrent reader threads, {ops} ops each...");

        var hits = 0;
        var misses = 0;
        var errors = 0;
        var sw = Stopwatch.StartNew();
        var random = new Random();

        var tasks = Enumerable.Range(0, threads).Select(t => Task.Run(() =>
        {
            var rnd = new Random(t);
            for (int i = 0; i < ops; i++)
            {
                try
                {
                    var key = $"stress:read:{rnd.Next(100)}";
                    var result = _cache!.Get(key);
                    if (result is not null)
                        Interlocked.Increment(ref hits);
                    else
                        Interlocked.Increment(ref misses);
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }
            }
        })).ToArray();

        Task.WaitAll(tasks);
        sw.Stop();

        PrintStressResult(threads * ops, hits + misses, errors, sw.Elapsed);
        ConsoleHelper.PrintInfo($"  Cache hits: {hits}, Misses: {misses}");

        // Cleanup
        for (int i = 0; i < 100; i++)
            try { _cache!.Remove($"stress:read:{i}"); } catch { }
    }

    private static void MixedConcurrentTest()
    {
        var threadsStr = ConsoleHelper.ReadLine("Enter number of threads (default: 15)") ?? "15";
        var opsStr = ConsoleHelper.ReadLine("Enter operations per thread (default: 200)") ?? "200";

        if (!int.TryParse(threadsStr, out var threads)) threads = 15;
        if (!int.TryParse(opsStr, out var ops)) ops = 200;

        Console.WriteLine();
        ConsoleHelper.PrintInfo($"Running {threads} concurrent mixed operation threads, {ops} ops each...");

        var writes = 0;
        var reads = 0;
        var updates = 0;
        var deletes = 0;
        var errors = 0;
        var sw = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, threads).Select(t => Task.Run(() =>
        {
            var rnd = new Random(t);
            for (int i = 0; i < ops; i++)
            {
                var key = $"stress:mixed:{rnd.Next(50)}";
                var op = rnd.Next(4);

                try
                {
                    switch (op)
                    {
                        case 0:
                            _cache!.Add(key, new Product { Id = i, Name = "Mixed", Price = i, Description = "Test" });
                            Interlocked.Increment(ref writes);
                            break;
                        case 1:
                            _ = _cache!.Get(key);
                            Interlocked.Increment(ref reads);
                            break;
                        case 2:
                            _cache!.Update(key, new Product { Id = i, Name = "Updated", Price = i * 2, Description = "Updated" });
                            Interlocked.Increment(ref updates);
                            break;
                        case 3:
                            _cache!.Remove(key);
                            Interlocked.Increment(ref deletes);
                            break;
                    }
                }
                catch
                {
                    Interlocked.Increment(ref errors);
                }
            }
        })).ToArray();

        Task.WaitAll(tasks);
        sw.Stop();

        PrintStressResult(threads * ops, writes + reads + updates + deletes, errors, sw.Elapsed);
        ConsoleHelper.PrintInfo($"  Writes: {writes}, Reads: {reads}, Updates: {updates}, Deletes: {deletes}");

        // Cleanup
        for (int i = 0; i < 50; i++)
            try { _cache!.Remove($"stress:mixed:{i}"); } catch { }
    }

    private static void RapidFireTest()
    {
        var opsStr = ConsoleHelper.ReadLine("Enter total operations (default: 5000)") ?? "5000";
        if (!int.TryParse(opsStr, out var ops)) ops = 5000;

        Console.WriteLine();
        ConsoleHelper.PrintInfo($"Running rapid fire test with {ops} sequential operations...");

        var success = 0;
        var errors = 0;
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < ops; i++)
        {
            try
            {
                var key = $"rapid:{i % 100}";
                switch (i % 3)
                {
                    case 0: _cache!.Add(key, $"Value {i}"); break;
                    case 1: _ = _cache!.Get(key); break;
                    case 2: _cache!.Remove(key); break;
                }
                success++;
            }
            catch { errors++; }

            if (i % 500 == 0)
                Console.Write($"\r  Progress: {i * 100 / ops}%    ");
        }

        sw.Stop();
        Console.WriteLine();

        PrintStressResult(ops, success, errors, sw.Elapsed);
    }

    private static void PrintStressResult(int total, int completed, int errors, TimeSpan elapsed)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSuccess($"Stress test completed!");
        ConsoleHelper.PrintInfo($"  Total ops: {total:N0}");
        ConsoleHelper.PrintInfo($"  Completed: {completed:N0}");
        ConsoleHelper.PrintInfo($"  Errors: {errors:N0}");
        ConsoleHelper.PrintInfo($"  Duration: {elapsed.TotalSeconds:F2}s");
        ConsoleHelper.PrintInfo($"  Throughput: {total / elapsed.TotalSeconds:F2} ops/sec");
    }
}
