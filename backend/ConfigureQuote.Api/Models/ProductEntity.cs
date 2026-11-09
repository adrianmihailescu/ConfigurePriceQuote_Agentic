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
