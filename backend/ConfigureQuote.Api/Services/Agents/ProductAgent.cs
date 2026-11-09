using ConfigureQuote.Api.Models;
using ConfigureQuote.Api.Services.Interfaces;

namespace ConfigureQuote.Api.Services.Agents;

public sealed class ProductAgent : IProductAgent
{
    public CatalogProduct? FindProduct(IEnumerable<CatalogProduct> catalog, string productId) =>
        catalog.FirstOrDefault(product => string.Equals(product.Id, productId, StringComparison.OrdinalIgnoreCase));
}
