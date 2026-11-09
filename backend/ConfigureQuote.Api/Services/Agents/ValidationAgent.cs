using ConfigureQuote.Api.Models;
using ConfigureQuote.Api.Services.Interfaces;

namespace ConfigureQuote.Api.Services.Agents;

public sealed class ValidationAgent : IValidationAgent
{
    public IReadOnlyList<string> Validate(CatalogProduct product, IReadOnlyDictionary<string, string> selectedOptions)
    {
        var issues = new List<string>();
        var optionGroups = product.Options.ToDictionary(group => group.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in product.Options.Where(group => group.Required))
        {
            if (!selectedOptions.TryGetValue(group.Key, out var selectedChoice) ||
                !group.Choices.Any(choice => string.Equals(choice.Id, selectedChoice, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add($"Choose a valid option for {group.Label}.");
            }
        }

        foreach (var selection in selectedOptions)
        {
            if (!optionGroups.TryGetValue(selection.Key, out var group) ||
                !group.Choices.Any(choice => string.Equals(choice.Id, selection.Value, StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add($"The selected {selection.Key} option is not available for {product.Name}.");
            }
        }

        return issues;
    }
}
