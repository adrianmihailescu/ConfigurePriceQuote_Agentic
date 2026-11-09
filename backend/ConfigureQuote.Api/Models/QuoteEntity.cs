namespace ConfigureQuote.Api.Models;

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
