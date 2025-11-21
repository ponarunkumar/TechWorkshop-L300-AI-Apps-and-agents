namespace ZavaChat.Core.Models;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public sealed record Product
{
    /// <summary>Unique product identifier</summary>
    public required string Id { get; init; }
    
    /// <summary>Product name</summary>
    public required string Name { get; init; }
    
    /// <summary>Product description</summary>
    public required string Description { get; init; }
    
    /// <summary>Product price</summary>
    public required decimal Price { get; init; }
    
    /// <summary>Product image URL</summary>
    public required string ImageUrl { get; init; }
    
    /// <summary>Product category</summary>
    public required string Category { get; init; }
    
    /// <summary>Available quantity</summary>
    public int Quantity { get; init; }
    
    /// <summary>Product SKU</summary>
    public string? Sku { get; init; }
    
    /// <summary>Product tags for search</summary>
    public List<string>? Tags { get; init; }
}
