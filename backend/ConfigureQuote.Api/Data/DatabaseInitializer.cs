using ConfigureQuote.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace ConfigureQuote.Api.Data;

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
