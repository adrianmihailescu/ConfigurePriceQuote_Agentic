namespace ConfigureQuote.Api.Models;

public sealed record ProductChoice(string Id, string Label, decimal PriceDelta);

public sealed record ProductOptionGroup(
    string Key,
    string Label,
    bool Required,
    IReadOnlyList<ProductChoice> Choices);

public sealed record CatalogProduct(
    string Id,
    string Name,
    string Category,
    string Description,
    decimal BasePrice,
    string ImageUrl,
    IReadOnlyList<ProductOptionGroup> Options);

public sealed record AgentStep(string Agent, string Status, string Summary);

public sealed record ChatRequest(
    string ProductId,
    Dictionary<string, string>? SelectedOptions,
    string? Message);

public sealed record ChatResponse(
    string Reply,
    bool IsValid,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    IReadOnlyList<string> Issues,
    IReadOnlyList<AgentStep> AgentSteps);

public sealed record CreateQuoteRequest(
    string CustomerName,
    string CustomerEmail,
    string ProductId,
    Dictionary<string, string>? SelectedOptions,
    string? Notes);

public sealed record QuoteSummary(
    int Id,
    string QuoteNumber,
    string CustomerName,
    string CustomerEmail,
    string ProductName,
    decimal Total,
    string Status,
    DateTime CreatedAt);

public sealed record SalesOverview(
    int QuotesThisMonth,
    int QuotesLastMonth,
    int TotalQuotes,
    int OpenQuotes,
    decimal PipelineValue,
    decimal? WinRate,
    int DecidedQuotes);
