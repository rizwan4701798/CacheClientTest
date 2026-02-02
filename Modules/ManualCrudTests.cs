using TestApp.Helpers;
using TestApp.Models;
using CacheClient;
using TestApp.Constants;

namespace TestApp.Modules;

public static class ManualCrudTests
{
    public static void Run(ClientStateManager state)
    {
        while (true)
        {
            Console.WriteLine();
            ConsoleHelper.PrintDivider('-');
            ConsoleHelper.PrintCentered(ClientTestConstants.CrudHeader, ConsoleColor.Yellow);
            ConsoleHelper.PrintDivider('-');
            Console.WriteLine();
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option1, ClientTestConstants.DescAdd);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option2, ClientTestConstants.DescGet);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option3, ClientTestConstants.DescUpdate);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option4, ClientTestConstants.DescRemove);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.Option5, ClientTestConstants.DescAddEx);
            ConsoleHelper.PrintMenuItem(ClientTestConstants.OptionB, ClientTestConstants.DescBack);
            Console.WriteLine();

            var choice = ConsoleHelper.ReadLine(ClientTestConstants.PromptSelectOption);

            try
            {
                switch (choice?.ToUpperInvariant())
                {
                    case ClientTestConstants.Option1: AddProduct(state.CurrentClient); break;
                    case ClientTestConstants.Option2: GetProduct(state.CurrentClient); break;
                    case ClientTestConstants.Option3: UpdateProduct(state.CurrentClient); break;
                    case ClientTestConstants.Option4: RemoveProduct(state.CurrentClient); break;
                    case ClientTestConstants.Option5: AddProductWithExpiration(state.CurrentClient); break;
                    case ClientTestConstants.OptionB: return;
                    default: ConsoleHelper.PrintWarning(ClientTestConstants.InvalidChoice); break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintError(string.Format(ClientTestConstants.OperationFailed, ex.Message));
            }
        }
    }

    private static void AddProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader(ClientTestConstants.AddHeader);

        var key = ConsoleHelper.ReadLine(ClientTestConstants.PromptKey) ?? ClientTestConstants.KeyProduct1;
        var name = ConsoleHelper.ReadLine(ClientTestConstants.PromptName) ?? ClientTestConstants.NameSample;
        var priceStr = ConsoleHelper.ReadLine(ClientTestConstants.PromptPrice) ?? ClientTestConstants.Price99;
        var description = ConsoleHelper.ReadLine(ClientTestConstants.PromptDesc) ?? ClientTestConstants.DescSample;

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
        ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.AddedSuccess, name, key));
    }

    private static void AddProductWithExpiration(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader(ClientTestConstants.AddExHeader);

        var key = ConsoleHelper.ReadLine(ClientTestConstants.PromptKeySimple) ?? "product:temp";
        var name = ConsoleHelper.ReadLine(ClientTestConstants.PromptName) ?? ClientTestConstants.TempProduct;
        var expirationStr = ConsoleHelper.ReadLine(ClientTestConstants.PromptExSeconds) ?? "30";

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
        ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.AddedExSuccess, expiration));
    }

    private static void GetProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader(ClientTestConstants.GetHeader);

        var key = ConsoleHelper.ReadLine(ClientTestConstants.PromptKeySimple) ?? ClientTestConstants.KeyProduct1;
        var result = cache.Get(key);

        if (result is Product product)
        {
            ConsoleHelper.PrintSuccess(ClientTestConstants.ProductFound);
            ConsoleHelper.PrintProductDetails(product);
        }
        else if (result is not null)
        {
            ConsoleHelper.PrintInfo(string.Format(ClientTestConstants.ValueFound, result.GetType().Name, result));
        }
        else
        {
            ConsoleHelper.PrintWarning(ClientTestConstants.ProductNotFound);
        }
    }

    private static void UpdateProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader(ClientTestConstants.UpdateHeader);

        var key = ConsoleHelper.ReadLine(ClientTestConstants.PromptKeySimple) ?? ClientTestConstants.KeyProduct1;
        var name = ConsoleHelper.ReadLine(ClientTestConstants.PromptNewName) ?? ClientTestConstants.UpdatedProduct;
        var priceStr = ConsoleHelper.ReadLine(ClientTestConstants.PromptNewPrice) ?? ClientTestConstants.Price149;

        if (!decimal.TryParse(priceStr, out var price))
            price = 149.99m;

        var product = new Product
        {
            Id = Utils.ExtractIdFromKey(key),
            Name = name,
            Price = price,
            Description = string.Format(ClientTestConstants.UpdateDesc, DateTime.Now)
        };

        cache.Update(key, product);
        ConsoleHelper.PrintSuccess(ClientTestConstants.UpdatedSuccess);
    }

    private static void RemoveProduct(ICache cache)
    {
        Console.WriteLine();
        ConsoleHelper.PrintSubHeader(ClientTestConstants.RemoveHeader);

        var key = ConsoleHelper.ReadLine(ClientTestConstants.PromptKeySimple) ?? ClientTestConstants.KeyProduct1;
        cache.Remove(key);
        ConsoleHelper.PrintSuccess(string.Format(ClientTestConstants.RemovedSuccess, key));
    }
}
