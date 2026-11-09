using ConfigureQuote.Api.Data;
using ConfigureQuote.Api.Models;
using ConfigureQuote.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConfigureQuote.Api.Services;

public sealed class AgentOrchestrator(
    QuoteDbContext db,
    IProductAgent productAgent,
    IValidationAgent validationAgent,
    IPricingAgent pricingAgent)
{
    public async Task<ChatResponse> ConfigureAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var selectedOptions = request.SelectedOptions
            ?? throw new ArgumentException("Selected options are required.", nameof(request));
        var products = SampleCatalog.ToCatalogProducts(await db.Products.AsNoTracking().ToListAsync(cancellationToken));
        var product = productAgent.FindProduct(products, request.ProductId);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product '{request.ProductId}' was not found.");
        }

        var issues = validationAgent.Validate(product, selectedOptions);
        var steps = new List<AgentStep>
        {
            new("Product Agent", "complete", $"Matched {product.Name} from the product catalog."),
            new("Validation Agent", issues.Count == 0 ? "complete" : "needs-attention",
                issues.Count == 0 ? "All selected options are available." : string.Join(" ", issues))
        };

        if (issues.Count > 0)
        {
            return new ChatResponse(
                "I found a configuration issue. Please review the highlighted options before requesting a quote.",
                false,
                0,
                0,
                0,
                issues,
                steps);
        }

        var (subtotal, discount, total) = pricingAgent.Calculate(product, selectedOptions);
        steps.Add(new("Pricing Agent", "complete", $"Calculated an estimated total of ${total:N2} USD."));
        steps.Add(new("Quotation Agent", "ready", "Configuration is ready to turn into a quote."));
        var message = string.IsNullOrWhiteSpace(request.Message)
            ? "Your configuration is valid and ready for a quote."
            : $"I reviewed your request: “{request.Message.Trim()}” Your configuration is valid and ready for a quote.";

        return new ChatResponse(message, true, subtotal, discount, total, [], steps);
    }
}
