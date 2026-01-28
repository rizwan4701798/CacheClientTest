namespace TestApp.Models;

/// <summary>
/// Represents a product for cache testing.
/// </summary>
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;

    public override string ToString() => $"Product {{ Id={Id}, Name=\"{Name}\", Price=${Price:F2} }}";
}
