using System.Text.Json;
using ConfigureQuote.Api.Models;

namespace ConfigureQuote.Api.Services;

public static class SampleCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<ProductEntity> Products =>
    [
        CreateProduct(
            "edge-server-r440",
            "Edge Server R440",
            "Compute",
            "A compact, high-performance server built for demanding edge workloads.",
            4890m,
            "https://images.unsplash.com/photo-1597852074816-d933c7d2b988?auto=format&fit=crop&w=960&q=85",
            [
                new("memory", "Memory", true, [new("32gb", "32 GB", 0), new("64gb", "64 GB", 460), new("128gb", "128 GB", 1120)]),
                new("storage", "Primary storage", true, [new("1tb-nvme", "1 TB NVMe", 0), new("2tb-nvme", "2 TB NVMe", 290), new("4tb-nvme", "4 TB NVMe", 740)]),
                new("support", "Support", true, [new("standard", "Standard · 1 year", 0), new("premium", "Premium · 3 years", 620)])
            ]),
        CreateProduct(
            "studio-workstation-x",
            "Studio Workstation X",
            "Workstation",
            "A powerful, quiet workstation tuned for creative and engineering teams.",
            3250m,
            "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?auto=format&fit=crop&w=960&q=85",
            [
                new("memory", "Memory", true, [new("32gb", "32 GB", 0), new("64gb", "64 GB", 380)]),
                new("graphics", "Graphics", true, [new("pro-8gb", "Pro · 8 GB", 0), new("pro-16gb", "Pro · 16 GB", 890)]),
                new("support", "Support", true, [new("standard", "Standard · 1 year", 0), new("premium", "Premium · 3 years", 420)])
            ]),
        CreateProduct(
            "atlas-storage-24",
            "Atlas Storage 24",
            "Storage",
            "Scalable all-flash storage with dependable performance for growing teams.",
            6120m,
            "https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?auto=format&fit=crop&w=960&q=85",
            [
                new("capacity", "Usable capacity", true, [new("12tb", "12 TB", 0), new("24tb", "24 TB", 1380), new("48tb", "48 TB", 2980)]),
                new("network", "Network", true, [new("10gbe", "10 GbE", 0), new("25gbe", "25 GbE", 540)]),
                new("support", "Support", true, [new("standard", "Standard · 1 year", 0), new("premium", "Premium · 3 years", 760)])
            ])
    ];

    public static IReadOnlyList<CatalogProduct> ToCatalogProducts(IEnumerable<ProductEntity> products) =>
        products.Select(product => new CatalogProduct(
            product.Id,
            product.Name,
            product.Category,
            product.Description,
            product.BasePrice,
            product.ImageUrl,
            JsonSerializer.Deserialize<List<ProductOptionGroup>>(product.OptionsJson, JsonOptions) ?? []))
            .ToArray();

    private static ProductEntity CreateProduct(
        string id,
        string name,
        string category,
        string description,
        decimal basePrice,
        string imageUrl,
        IReadOnlyList<ProductOptionGroup> options) =>
        new()
        {
            Id = id,
            Name = name,
            Category = category,
            Description = description,
            BasePrice = basePrice,
            ImageUrl = imageUrl,
            OptionsJson = JsonSerializer.Serialize(options, JsonOptions)
        };
}
