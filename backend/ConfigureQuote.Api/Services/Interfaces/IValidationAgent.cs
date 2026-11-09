using ConfigureQuote.Api.Models;

namespace ConfigureQuote.Api.Services.Interfaces;

public interface IValidationAgent
{
    IReadOnlyList<string> Validate(CatalogProduct product, IReadOnlyDictionary<string, string> selectedOptions);
}
