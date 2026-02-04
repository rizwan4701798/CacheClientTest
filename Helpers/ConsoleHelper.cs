using System;
using TestApp.Models;

namespace TestApp.Helpers;

public static class ConsoleHelper
{
    private static readonly object _consoleLock = new();

    public static void PrintHeader()
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

    public static void PrintDivider(char c = '-', int length = 60)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string(c, length));
        Console.ResetColor();
    }

    public static void PrintCentered(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        var padding = (60 - text.Length) / 2;
        Console.WriteLine($"{new string(' ', Math.Max(0, padding))}{text}");
        Console.ResetColor();
    }

    public static void PrintSubHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($">> {text}");
        Console.ResetColor();
    }

    public static void PrintMenuItem(string key, string description)
    {
        Console.Write("  [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(key);
        Console.ResetColor();
        Console.WriteLine($"] {description}");
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"  {message}");
        Console.ResetColor();
    }

    public static void PrintEvent(string eventType, string key, ConsoleColor color)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"  [EVENT] {eventType}: {key}");
            Console.ResetColor();
        }
    }

    public static void PrintProductDetails(Product product)
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

    public static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    public static string? ReadLine(string prompt)
    {
        Console.Write($"  {prompt}: ");
        Console.ForegroundColor = ConsoleColor.White;
        var result = Console.ReadLine();
        Console.ResetColor();
        return result;
    }

    public static void WaitForKey()
    {
        Console.WriteLine();
        PrintInfo("Press any key to continue...");
        Console.ReadKey(true);
    }
}
