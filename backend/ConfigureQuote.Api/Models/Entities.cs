namespace ConfigureQuote.Api.Models;

public sealed class ProductEntity
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Description { get; set; }
    public decimal BasePrice { get; set; }
    public required string ImageUrl { get; set; }
    public required string OptionsJson { get; set; }
}

public sealed class QuoteEntity
{
    public int Id { get; set; }
    public required string QuoteNumber { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string SelectedOptionsJson { get; set; }
    public required string Notes { get; set; }
    public decimal Total { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
