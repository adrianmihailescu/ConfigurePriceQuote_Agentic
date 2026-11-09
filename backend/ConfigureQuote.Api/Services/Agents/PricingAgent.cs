using ConfigureQuote.Api.Models;
using ConfigureQuote.Api.Services.Interfaces;

namespace ConfigureQuote.Api.Services.Agents;

public sealed class PricingAgent : IPricingAgent
{
    public (decimal Subtotal, decimal Discount, decimal Total) Calculate(
        CatalogProduct product,
        IReadOnlyDictionary<string, string> selectedOptions)
    {
        var optionsTotal = product.Options
            .SelectMany(group => group.Choices
                .Where(choice => selectedOptions.TryGetValue(group.Key, out var selected) &&
                                 string.Equals(choice.Id, selected, StringComparison.OrdinalIgnoreCase)))
            .Sum(choice => choice.PriceDelta);
        var subtotal = product.BasePrice + optionsTotal;
        var discount = subtotal >= 5000m ? decimal.Round(subtotal * 0.05m, 2) : 0m;
        return (subtotal, discount, subtotal - discount);
    }
}
