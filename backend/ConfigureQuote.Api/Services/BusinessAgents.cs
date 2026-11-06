using ConfigureQuote.Api.Data;
using ConfigureQuote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfigureQuote.Api.Services;

public interface IProductAgent
{
    CatalogProduct? FindProduct(IEnumerable<CatalogProduct> catalog, string productId);
}

public interface IValidationAgent
{
    IReadOnlyList<string> Validate(CatalogProduct product, IReadOnlyDictionary<string, string> selectedOptions);
}

public interface IPricingAgent
{
    (decimal Subtotal, decimal Discount, decimal Total) Calculate(
        CatalogProduct product,
        IReadOnlyDictionary<string, string> selectedOptions);
}

public interface IQuotationAgent
{
    QuoteSummary CreateSummary(QuoteEntity quote);
}

public sealed class ProductAgent : IProductAgent
{
    public CatalogProduct? FindProduct(IEnumerable<CatalogProduct> catalog, string productId) =>
        catalog.FirstOrDefault(product => string.Equals(product.Id, productId, StringComparison.OrdinalIgnoreCase));
}

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

public sealed class DatabaseInitializer(IServiceScopeFactory scopeFactory, ILogger<DatabaseInitializer> logger)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _initialized;

    public async Task EnsureCreatedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<QuoteDbContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            if (!await db.Products.AnyAsync(cancellationToken))
            {
                await db.Products.AddRangeAsync(SampleCatalog.Products, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var sampleProducts = SampleCatalog.Products.ToDictionary(product => product.Id);
                var existingProducts = await db.Products
                    .Where(product => sampleProducts.Keys.Contains(product.Id))
                    .ToListAsync(cancellationToken);
                foreach (var product in existingProducts)
                {
                    product.ImageUrl = sampleProducts[product.Id].ImageUrl;
                }

                await db.SaveChangesAsync(cancellationToken);
            }

            _initialized = true;
            logger.LogInformation("Quote database and sample catalog are ready.");
        }
        finally
        {
            _gate.Release();
        }
    }
}
