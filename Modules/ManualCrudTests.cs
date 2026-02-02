using TestApp.Helpers;
using TestApp.Models;
using CacheClient;

namespace TestApp.Modules;

public static class ManualCrudTests
{
    public static void Run(ClientStateManager state)
    {
        while (true)
        {
            Console.WriteLine();
            ConsoleHelper.PrintDivider('-');
            ConsoleHelper.PrintCentered("CRUD OPERATIONS", ConsoleColor.Yellow);
            ConsoleHelper.PrintDivider('-');
            Console.WriteLine();
            ConsoleHelper.PrintMenuItem("1", "Add Product");
            ConsoleHelper.PrintMenuItem("2", "Get Product");
            ConsoleHelper.PrintMenuItem("3", "Update Product");
            ConsoleHelper.PrintMenuItem("4", "Remove Product");
            ConsoleHelper.PrintMenuItem("5", "Add with Expiration");
            ConsoleHelper.PrintMenuItem("B", "Back to Main Menu");
            Console.WriteLine();

            var choice = ConsoleHelper.ReadLine("Select option");

            try
            {
                switch (choice?.ToUpperInvariant())
                {
                    case "1": AddProduct(state.CurrentClient); break;
                    case "2": GetProduct(state.CurrentClient); break;
                    case "3": UpdateProduct(state.CurrentClient); break;
                    case "4": RemoveProduct(state.CurrentClient); break;
                    case "5": AddProductWithExpiration(state.CurrentClient); break;
                    case "B": return;
                    default: ConsoleHelper.PrintWarning("Invalid choice."); break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError($"Operation failed: {ex.Message}");
            }
        }
    }

    private static void AddProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader("Add New Product");

        var key = ConsoleHelper.ReadLine("Enter cache key (e.g., product:1)") ?? "product:1";
        var name = ConsoleHelper.ReadLine("Enter product name") ?? "Sample Product";
        var priceStr = ConsoleHelper.ReadLine("Enter price") ?? "99.99";
        var description = ConsoleHelper.ReadLine("Enter description") ?? "A sample product";

        if (!decimal.TryParse(priceStr, out var price))
            price = 99.99m;

        var product = new Product
        {
            Id = Utils.ExtractIdFromKey(key),
            Name = name,
            Price = price,
            Description = description
        };

        cache.Add(key, product);
        ConsoleHelper.PrintSuccess($"Product '{name}' added with key '{key}'");
    }

    private static void AddProductWithExpiration(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader("Add Product with Expiration");

        var key = ConsoleHelper.ReadLine("Enter cache key") ?? "product:temp";
        var name = ConsoleHelper.ReadLine("Enter product name") ?? "Temporary Product";
        var expirationStr = ConsoleHelper.ReadLine("Enter expiration in seconds") ?? "30";

        if (!int.TryParse(expirationStr, out var expiration))
            expiration = 30;

        var product = new Product
        {
            Id = Utils.ExtractIdFromKey(key),
            Name = name,
            Price = 49.99m,
            Description = $"Expires in {expiration} seconds"
        };

        cache.Add(key, product, expiration);
        ConsoleHelper.PrintSuccess($"Product added with {expiration}s expiration");
    }

    private static void GetProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader("Get Product");

        var key = ConsoleHelper.ReadLine("Enter cache key") ?? "product:1";
        var result = cache.Get(key);

        if (result is Product product)
        {
            ConsoleHelper.PrintSuccess("Product found:");
            ConsoleHelper.PrintProductDetails(product);
        }
        else if (result is not null)
        {
            ConsoleHelper.PrintInfo($"Value found (type: {result.GetType().Name}): {result}");
        }
        else
        {
            ConsoleHelper.PrintWarning("Product not found in cache.");
        }
    }

    private static void UpdateProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader("Update Product");

        var key = ConsoleHelper.ReadLine("Enter cache key") ?? "product:1";
        var name = ConsoleHelper.ReadLine("Enter new product name") ?? "Updated Product";
        var priceStr = ConsoleHelper.ReadLine("Enter new price") ?? "149.99";

        if (!decimal.TryParse(priceStr, out var price))
            price = 149.99m;

        var product = new Product
        {
            Id = Utils.ExtractIdFromKey(key),
            Name = name,
            Price = price,
            Description = $"Updated at {DateTime.Now:HH:mm:ss}"
        };

        cache.Update(key, product);
        ConsoleHelper.PrintSuccess($"Product updated successfully");
    }

    private static void RemoveProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader("Remove Product");

        var key = ConsoleHelper.ReadLine("Enter cache key") ?? "product:1";
        cache.Remove(key);
        ConsoleHelper.PrintSuccess($"Product with key '{key}' removed");
    }
}
