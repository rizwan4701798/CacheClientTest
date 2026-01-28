using System.Diagnostics;
using CacheClientLib = CacheClient;
using CacheClient.Models;
using TestApp.Models;

namespace TestApp;

class Program
{
    private static CacheClientLib.ICache? _cache;
    private static readonly object _consoleLock = new();

    static void Main(string[] args)
    {
        Console.Title = "Cache Server Test Application";
        
        PrintHeader();
        
        if (!InitializeCache())
        {
            PrintError("Failed to initialize cache client. Ensure the cache server is running.");
            WaitForKey();
            return;
        }

        RunMainLoop();

        _cache?.Dispose();
        PrintSuccess("Cache client disposed. Goodbye!");
    }

    #region Initialization

    static bool InitializeCache()
    {
        try
        {
            PrintInfo("Initializing cache client...");
            
            var options = new CacheClientLib.CacheClientOptions
            {
                Host = "localhost",
                Port = 5050,
                NotificationPort = 5051,
                TimeoutMilliseconds = 5000
            };

            _cache = new CacheClientLib.CacheClient(options);
            _cache.Initialize();

            // Subscribe to events
            _cache.ItemAdded += (s, e) => PrintEvent("ADDED", e.Key, ConsoleColor.Green);
            _cache.ItemUpdated += (s, e) => PrintEvent("UPDATED", e.Key, ConsoleColor.Yellow);
            _cache.ItemRemoved += (s, e) => PrintEvent("REMOVED", e.Key, ConsoleColor.Red);
            _cache.ItemExpired += (s, e) => PrintEvent("EXPIRED", e.Key, ConsoleColor.Magenta);
            _cache.ItemEvicted += (s, e) => PrintEvent("EVICTED", e.Key, ConsoleColor.DarkYellow);

            PrintSuccess("Cache client initialized successfully!");
            return true;
        }
        catch (Exception ex)
        {
            PrintError($"Initialization failed: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Main Menu

    static void RunMainLoop()
    {
        while (true)
        {
            ShowMainMenu();
            var choice = ReadLine("Select option");

            try
            {
                switch (choice?.ToUpperInvariant())
                {
                    case "1": ManualCrudOperations(); break;
                    case "2": ExpirationTests(); break;
                    case "3": EventNotificationTests(); break;
                    case "4": PerformanceBenchmarks(); break;
                    case "5": StressTests(); break;
                    case "6": InteractiveMode(); break;
                    case "Q" or "0": return;
                    default: PrintWarning("Invalid choice. Please try again."); break;
                }
            }
            catch (Exception ex)
            {
                PrintError($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    static void ShowMainMenu()
    {
        Console.WriteLine();
        PrintDivider('=');
        PrintCentered("MAIN MENU", ConsoleColor.Cyan);
        PrintDivider('=');
        Console.WriteLine();
        PrintMenuItem("1", "Manual CRUD Operations");
        PrintMenuItem("2", "Expiration Tests");
        PrintMenuItem("3", "Event Notification Tests");
        PrintMenuItem("4", "Performance Benchmarks");
        PrintMenuItem("5", "Stress Tests");
        PrintMenuItem("6", "Interactive Mode");
        PrintMenuItem("Q", "Quit");
        Console.WriteLine();
    }

    #endregion

    #region Manual CRUD Operations

    static void ManualCrudOperations()
    {
        while (true)
        {
            Console.WriteLine();
            PrintDivider('-');
            PrintCentered("CRUD OPERATIONS", ConsoleColor.Yellow);
            PrintDivider('-');
            Console.WriteLine();
            PrintMenuItem("1", "Add Product");
            PrintMenuItem("2", "Get Product");
            PrintMenuItem("3", "Update Product");
            PrintMenuItem("4", "Remove Product");
            PrintMenuItem("5", "Add with Expiration");
            PrintMenuItem("B", "Back to Main Menu");
            Console.WriteLine();

            var choice = ReadLine("Select option");

            try
            {
                switch (choice?.ToUpperInvariant())
                {
                    case "1": AddProduct(); break;
                    case "2": GetProduct(); break;
                    case "3": UpdateProduct(); break;
                    case "4": RemoveProduct(); break;
                    case "5": AddProductWithExpiration(); break;
                    case "B": return;
                    default: PrintWarning("Invalid choice."); break;
                }
            }
            catch (Exception ex)
            {
                PrintError($"Operation failed: {ex.Message}");
            }
        }
    }

    static void AddProduct()
    {
        Console.WriteLine();
        PrintSubHeader("Add New Product");

        var key = ReadLine("Enter cache key (e.g., product:1)") ?? "product:1";
        var name = ReadLine("Enter product name") ?? "Sample Product";
        var priceStr = ReadLine("Enter price") ?? "99.99";
        var description = ReadLine("Enter description") ?? "A sample product";

        if (!decimal.TryParse(priceStr, out var price))
            price = 99.99m;

        var product = new Product
        {
            Id = ExtractIdFromKey(key),
            Name = name,
            Price = price,
            Description = description
        };

        _cache!.Add(key, product);
        PrintSuccess($"Product '{name}' added with key '{key}'");
    }

    static void AddProductWithExpiration()
    {
        Console.WriteLine();
        PrintSubHeader("Add Product with Expiration");

        var key = ReadLine("Enter cache key") ?? "product:temp";
        var name = ReadLine("Enter product name") ?? "Temporary Product";
        var expirationStr = ReadLine("Enter expiration in seconds") ?? "30";

        if (!int.TryParse(expirationStr, out var expiration))
            expiration = 30;

        var product = new Product
        {
            Id = ExtractIdFromKey(key),
            Name = name,
            Price = 49.99m,
            Description = $"Expires in {expiration} seconds"
        };

        _cache!.Add(key, product, expiration);
        PrintSuccess($"Product added with {expiration}s expiration");
    }

    static void GetProduct()
    {
        Console.WriteLine();
        PrintSubHeader("Get Product");

        var key = ReadLine("Enter cache key") ?? "product:1";
        var result = _cache!.Get(key);

        if (result is Product product)
        {
            PrintSuccess("Product found:");
            PrintProductDetails(product);
        }
        else if (result is not null)
        {
            PrintInfo($"Value found (type: {result.GetType().Name}): {result}");
        }
        else
        {
            PrintWarning("Product not found in cache.");
        }
    }

    static void UpdateProduct()
    {
        Console.WriteLine();
        PrintSubHeader("Update Product");

        var key = ReadLine("Enter cache key") ?? "product:1";
        var name = ReadLine("Enter new product name") ?? "Updated Product";
        var priceStr = ReadLine("Enter new price") ?? "149.99";

        if (!decimal.TryParse(priceStr, out var price))
            price = 149.99m;

        var product = new Product
        {
            Id = ExtractIdFromKey(key),
            Name = name,
            Price = price,
            Description = $"Updated at {DateTime.Now:HH:mm:ss}"
        };

        _cache!.Update(key, product);
        PrintSuccess($"Product updated successfully");
    }

    static void RemoveProduct()
    {
        Console.WriteLine();
        PrintSubHeader("Remove Product");

        var key = ReadLine("Enter cache key") ?? "product:1";
        _cache!.Remove(key);
        PrintSuccess($"Product with key '{key}' removed");
    }

    #endregion

    #region Expiration Tests

    static void ExpirationTests()
    {
        Console.WriteLine();
        PrintDivider('-');
        PrintCentered("EXPIRATION TESTS", ConsoleColor.Magenta);
        PrintDivider('-');
        Console.WriteLine();

        PrintInfo("This test will add items with different expiration times and verify they expire correctly.");
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

            _cache!.Add(key, product, exp);
            PrintInfo($"Added '{key}' with {exp}s expiration");
        }

        Console.WriteLine();
        PrintInfo("Monitoring expiration...");
        PrintInfo("Press any key to stop monitoring.\n");

        var stopwatch = Stopwatch.StartNew();
        var allExpired = false;

        while (!allExpired && !Console.KeyAvailable)
        {
            Thread.Sleep(1000);
            
            var remaining = keys.Where(k => _cache!.Get(k) is not null).ToList();
            var expired = keys.Except(remaining).ToList();

            Console.Write($"\r  [{stopwatch.Elapsed:mm\\:ss}] Active: {remaining.Count}, Expired: {expired.Count}    ");

            allExpired = remaining.Count == 0;
        }

        if (Console.KeyAvailable)
            Console.ReadKey(true);

        Console.WriteLine();
        PrintSuccess($"Expiration test completed in {stopwatch.Elapsed:mm\\:ss}");
    }

    #endregion

    #region Event Notification Tests

    static void EventNotificationTests()
    {
        Console.WriteLine();
        PrintDivider('-');
        PrintCentered("EVENT NOTIFICATION TESTS", ConsoleColor.Green);
        PrintDivider('-');
        Console.WriteLine();

        PrintInfo("Subscribing to cache events...");

        try
        {
            _cache!.Subscribe(
                CacheEventType.ItemAdded,
                CacheEventType.ItemUpdated,
                CacheEventType.ItemRemoved,
                CacheEventType.ItemExpired,
                CacheEventType.ItemEvicted);

            PrintSuccess("Subscribed to all cache events!");
            Console.WriteLine();
            PrintInfo("Now performing operations. Watch for event notifications:");
            Console.WriteLine();

            // Perform operations that trigger events
            var testKey = $"eventtest:{Guid.NewGuid():N}";

            Thread.Sleep(500);
            PrintInfo("  -> Adding item...");
            _cache.Add(testKey, new Product { Id = 1, Name = "Event Test Product", Price = 10, Description = "Test" });

            Thread.Sleep(500);
            PrintInfo("  -> Updating item...");
            _cache.Update(testKey, new Product { Id = 1, Name = "Updated Event Test", Price = 20, Description = "Updated" });

            Thread.Sleep(500);
            PrintInfo("  -> Removing item...");
            _cache.Remove(testKey);

            Thread.Sleep(500);
            PrintInfo("  -> Adding item with short expiration (2s)...");
            _cache.Add($"{testKey}:exp", new Product { Id = 2, Name = "Expiring", Price = 5, Description = "Will expire" }, 2);

            PrintInfo("  -> Waiting for expiration...");
            Thread.Sleep(3000);
            _cache.Get($"{testKey}:exp"); // Trigger expiration check

            Console.WriteLine();
            PrintSuccess("Event notification test completed!");
            PrintInfo("Note: Events are displayed in real-time as they occur.");
        }
        catch (Exception ex)
        {
            PrintError($"Event test failed: {ex.Message}");
        }
        finally
        {
            try { _cache!.Unsubscribe(); } catch { }
        }
    }

    #endregion

    #region Performance Benchmarks

    static void PerformanceBenchmarks()
    {
        Console.WriteLine();
        PrintDivider('-');
        PrintCentered("PERFORMANCE BENCHMARKS", ConsoleColor.Blue);
        PrintDivider('-');
        Console.WriteLine();

        var iterationsStr = ReadLine("Enter number of iterations (default: 1000)") ?? "1000";
        if (!int.TryParse(iterationsStr, out var iterations) || iterations <= 0)
            iterations = 1000;

        Console.WriteLine();

        // Write benchmark
        PrintInfo($"Running WRITE benchmark ({iterations} iterations)...");
        var writeResults = RunBenchmark("WRITE", iterations, i =>
        {
            var key = $"perf:write:{i}";
            _cache!.Add(key, new Product { Id = i, Name = $"Perf Test {i}", Price = i, Description = "Benchmark" });
        });
        PrintBenchmarkResult("WRITE", writeResults, iterations);

        // Read benchmark
        PrintInfo($"Running READ benchmark ({iterations} iterations)...");
        var readResults = RunBenchmark("READ", iterations, i =>
        {
            var key = $"perf:write:{i % iterations}";
            _ = _cache!.Get(key);
        });
        PrintBenchmarkResult("READ", readResults, iterations);

        // Update benchmark
        PrintInfo($"Running UPDATE benchmark ({iterations} iterations)...");
        var updateResults = RunBenchmark("UPDATE", iterations, i =>
        {
            var key = $"perf:write:{i % iterations}";
            _cache!.Update(key, new Product { Id = i, Name = $"Updated {i}", Price = i * 2, Description = "Updated" });
        });
        PrintBenchmarkResult("UPDATE", updateResults, iterations);

        // Mixed workload
        PrintInfo($"Running MIXED benchmark ({iterations} iterations)...");
        var random = new Random();
        var mixedResults = RunBenchmark("MIXED", iterations, i =>
        {
            var key = $"perf:write:{random.Next(iterations)}";
            var op = random.Next(3);
            switch (op)
            {
                case 0: try { _cache!.Add($"perf:mixed:{i}", new Product { Id = i, Name = "Mixed", Price = i, Description = "Test" }); } catch { } break;
                case 1: _ = _cache!.Get(key); break;
                case 2: try { _cache!.Update(key, new Product { Id = i, Name = "Mixed Update", Price = i, Description = "Test" }); } catch { } break;
            }
        });
        PrintBenchmarkResult("MIXED", mixedResults, iterations);

        // Cleanup
        PrintInfo("Cleaning up benchmark data...");
        for (int i = 0; i < iterations; i++)
        {
            try { _cache!.Remove($"perf:write:{i}"); } catch { }
            try { _cache!.Remove($"perf:mixed:{i}"); } catch { }
        }

        Console.WriteLine();
        PrintSuccess("Performance benchmarks completed!");
    }

    static (TimeSpan elapsed, double opsPerSecond) RunBenchmark(string name, int iterations, Action<int> operation)
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

    static void PrintBenchmarkResult(string name, (TimeSpan elapsed, double opsPerSecond) result, int iterations)
    {
        var color = result.opsPerSecond > 1000 ? ConsoleColor.Green :
                    result.opsPerSecond > 500 ? ConsoleColor.Yellow : ConsoleColor.Red;

        Console.WriteLine();
        Console.Write($"  {name,-10} | ");
        Console.Write($"Time: {result.elapsed.TotalMilliseconds,8:F2}ms | ");
        Console.Write($"Ops/sec: ");
        WriteColored($"{result.opsPerSecond,10:F2}", color);
        Console.Write($" | Avg: {result.elapsed.TotalMilliseconds / iterations:F3}ms/op");
        Console.WriteLine();
    }

    #endregion

    #region Stress Tests

    static void StressTests()
    {
        Console.WriteLine();
        PrintDivider('-');
        PrintCentered("STRESS TESTS", ConsoleColor.Red);
        PrintDivider('-');
        Console.WriteLine();

        PrintMenuItem("1", "Concurrent Writers Test");
        PrintMenuItem("2", "Concurrent Readers Test");
        PrintMenuItem("3", "Mixed Concurrent Operations");
        PrintMenuItem("4", "Rapid Fire Test");
        PrintMenuItem("B", "Back");
        Console.WriteLine();

        var choice = ReadLine("Select test");

        switch (choice?.ToUpperInvariant())
        {
            case "1": ConcurrentWritersTest(); break;
            case "2": ConcurrentReadersTest(); break;
            case "3": MixedConcurrentTest(); break;
            case "4": RapidFireTest(); break;
        }
    }

    static void ConcurrentWritersTest()
    {
        var threadsStr = ReadLine("Enter number of threads (default: 10)") ?? "10";
        var opsStr = ReadLine("Enter operations per thread (default: 100)") ?? "100";

        if (!int.TryParse(threadsStr, out var threads)) threads = 10;
        if (!int.TryParse(opsStr, out var ops)) ops = 100;

        Console.WriteLine();
        PrintInfo($"Running {threads} concurrent writer threads, {ops} ops each...");

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
        PrintInfo("Cleaning up...");
        Parallel.For(0, threads, t =>
        {
            for (int i = 0; i < ops; i++)
                try { _cache!.Remove($"stress:writer:{t}:{i}"); } catch { }
        });
    }

    static void ConcurrentReadersTest()
    {
        var threadsStr = ReadLine("Enter number of threads (default: 20)") ?? "20";
        var opsStr = ReadLine("Enter operations per thread (default: 500)") ?? "500";

        if (!int.TryParse(threadsStr, out var threads)) threads = 20;
        if (!int.TryParse(opsStr, out var ops)) ops = 500;

        // Pre-populate
        PrintInfo("Pre-populating cache with test data...");
        for (int i = 0; i < 100; i++)
        {
            try { _cache!.Add($"stress:read:{i}", new Product { Id = i, Name = $"Read Test {i}", Price = i, Description = "Test" }); } catch { }
        }

        Console.WriteLine();
        PrintInfo($"Running {threads} concurrent reader threads, {ops} ops each...");

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
        PrintInfo($"  Cache hits: {hits}, Misses: {misses}");

        // Cleanup
        for (int i = 0; i < 100; i++)
            try { _cache!.Remove($"stress:read:{i}"); } catch { }
    }

    static void MixedConcurrentTest()
    {
        var threadsStr = ReadLine("Enter number of threads (default: 15)") ?? "15";
        var opsStr = ReadLine("Enter operations per thread (default: 200)") ?? "200";

        if (!int.TryParse(threadsStr, out var threads)) threads = 15;
        if (!int.TryParse(opsStr, out var ops)) ops = 200;

        Console.WriteLine();
        PrintInfo($"Running {threads} concurrent mixed operation threads, {ops} ops each...");

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
        PrintInfo($"  Writes: {writes}, Reads: {reads}, Updates: {updates}, Deletes: {deletes}");

        // Cleanup
        for (int i = 0; i < 50; i++)
            try { _cache!.Remove($"stress:mixed:{i}"); } catch { }
    }

    static void RapidFireTest()
    {
        var opsStr = ReadLine("Enter total operations (default: 5000)") ?? "5000";
        if (!int.TryParse(opsStr, out var ops)) ops = 5000;

        Console.WriteLine();
        PrintInfo($"Running rapid fire test with {ops} sequential operations...");

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

    static void PrintStressResult(int total, int completed, int errors, TimeSpan elapsed)
    {
        Console.WriteLine();
        PrintSuccess($"Stress test completed!");
        PrintInfo($"  Total ops: {total:N0}");
        PrintInfo($"  Completed: {completed:N0}");
        PrintInfo($"  Errors: {errors:N0}");
        PrintInfo($"  Duration: {elapsed.TotalSeconds:F2}s");
        PrintInfo($"  Throughput: {total / elapsed.TotalSeconds:F2} ops/sec");
    }

    #endregion

    #region Interactive Mode

    static void InteractiveMode()
    {
        Console.WriteLine();
        PrintDivider('-');
        PrintCentered("INTERACTIVE MODE", ConsoleColor.Cyan);
        PrintDivider('-');
        Console.WriteLine();
        PrintInfo("Enter commands directly. Type 'help' for available commands, 'exit' to return.");
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
                        PrintWarning($"Unknown command: {command}. Type 'help' for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                PrintError($"Error: {ex.Message}");
            }
        }
    }

    static void PrintInteractiveHelp()
    {
        Console.WriteLine();
        PrintInfo("Available commands:");
        Console.WriteLine("  add <key> <value>      - Add a string value");
        Console.WriteLine("  addex <key> <ttl> <val>- Add with expiration (ttl in seconds)");
        Console.WriteLine("  get <key>              - Get a value");
        Console.WriteLine("  update <key> <value>   - Update a value");
        Console.WriteLine("  del <key>              - Delete a key");
        Console.WriteLine("  exit                   - Return to main menu");
        Console.WriteLine();
    }

    static void InteractiveAdd(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
        {
            PrintWarning("Usage: add <key> <value>");
            return;
        }
        _cache!.Add(parts[0], parts[1]);
        PrintSuccess($"OK");
    }

    static void InteractiveAddWithExpiration(string args)
    {
        var parts = args.Split(' ', 3);
        if (parts.Length < 3 || !int.TryParse(parts[1], out var ttl))
        {
            PrintWarning("Usage: addex <key> <ttl_seconds> <value>");
            return;
        }
        _cache!.Add(parts[0], parts[2], ttl);
        PrintSuccess($"OK (expires in {ttl}s)");
    }

    static void InteractiveGet(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            PrintWarning("Usage: get <key>");
            return;
        }
        var result = _cache!.Get(args);
        if (result is Product p)
            PrintProductDetails(p);
        else if (result is not null)
            Console.WriteLine($"  {result}");
        else
            PrintWarning("(nil)");
    }

    static void InteractiveUpdate(string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2)
        {
            PrintWarning("Usage: update <key> <value>");
            return;
        }
        _cache!.Update(parts[0], parts[1]);
        PrintSuccess("OK");
    }

    static void InteractiveDelete(string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            PrintWarning("Usage: del <key>");
            return;
        }
        _cache!.Remove(args);
        PrintSuccess("OK");
    }

    #endregion

    #region UI Helpers

    static void PrintHeader()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
   ╔═══════════════════════════════════════════════════════════╗
   ║           CACHE SERVER TEST APPLICATION                   ║
   ║                    v1.0.0                                 ║
   ╚═══════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    static void PrintDivider(char c = '-', int length = 60)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string(c, length));
        Console.ResetColor();
    }

    static void PrintCentered(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        var padding = (60 - text.Length) / 2;
        Console.WriteLine($"{new string(' ', Math.Max(0, padding))}{text}");
        Console.ResetColor();
    }

    static void PrintSubHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($">> {text}");
        Console.ResetColor();
    }

    static void PrintMenuItem(string key, string description)
    {
        Console.Write("  [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(key);
        Console.ResetColor();
        Console.WriteLine($"] {description}");
    }

    static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }

    static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {message}");
        Console.ResetColor();
    }

    static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    static void PrintEvent(string eventType, string key, ConsoleColor color)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"  [EVENT] {eventType}: {key}");
            Console.ResetColor();
        }
    }

    static void PrintProductDetails(Product product)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"    ┌─────────────────────────────────────");
        Console.WriteLine($"    │ ID:          {product.Id}");
        Console.WriteLine($"    │ Name:        {product.Name}");
        Console.WriteLine($"    │ Price:       ${product.Price:F2}");
        Console.WriteLine($"    │ Description: {product.Description}");
        Console.WriteLine($"    └─────────────────────────────────────");
        Console.ResetColor();
    }

    static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    static string? ReadLine(string prompt)
    {
        Console.Write($"  {prompt}: ");
        Console.ForegroundColor = ConsoleColor.White;
        var result = Console.ReadLine();
        Console.ResetColor();
        return result;
    }

    static void WaitForKey()
    {
        Console.WriteLine();
        PrintInfo("Press any key to continue...");
        Console.ReadKey(true);
    }

    static int ExtractIdFromKey(string key)
    {
        var parts = key.Split(':');
        if (parts.Length > 1 && int.TryParse(parts[^1], out var id))
            return id;
        return 0;
    }

    #endregion
}
