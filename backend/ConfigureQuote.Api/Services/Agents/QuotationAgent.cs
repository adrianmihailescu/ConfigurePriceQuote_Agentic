using ConfigureQuote.Api.Models;
using ConfigureQuote.Api.Services.Interfaces;

namespace ConfigureQuote.Api.Services.Agents;

public sealed class QuotationAgent : IQuotationAgent
{
    public QuoteSummary CreateSummary(QuoteEntity quote) =>
        new(
            quote.Id,
            quote.QuoteNumber,
            quote.CustomerName,
            quote.CustomerEmail,
            quote.ProductName,
            quote.Total,
            quote.Status,
            quote.CreatedAt);
}
