namespace ZavaChat.Core.Models;

/// <summary>
/// Represents an item in the shopping cart.
/// </summary>
public sealed record CartItem
{
    /// <summary>Product reference</summary>
    public required Product Product { get; init; }
    
    /// <summary>Quantity of this product in cart</summary>
    public required int Quantity { get; init; }
    
    /// <summary>When item was added to cart</summary>
    public DateTime AddedAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>Calculated line total (Price * Quantity)</summary>
    public decimal LineTotal => Product.Price * Quantity;
}
