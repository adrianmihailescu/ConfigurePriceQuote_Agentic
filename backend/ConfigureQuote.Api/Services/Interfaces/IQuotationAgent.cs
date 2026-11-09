using ConfigureQuote.Api.Models;

namespace ConfigureQuote.Api.Services.Interfaces;

public interface IQuotationAgent
{
    QuoteSummary CreateSummary(QuoteEntity quote);
}
