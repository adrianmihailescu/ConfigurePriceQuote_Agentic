using ConfigureQuote.Api.Models;

namespace ConfigureQuote.Api.Services.Interfaces;

public interface IProductAgent
{
    CatalogProduct? FindProduct(IEnumerable<CatalogProduct> catalog, string productId);
}
