using ConfigureQuote.Api.Models;

namespace ConfigureQuote.Api.Services.Interfaces;

public interface IPricingAgent
{
    (decimal Subtotal, decimal Discount, decimal Total) Calculate(
        CatalogProduct product,
        IReadOnlyDictionary<string, string> selectedOptions);
}
