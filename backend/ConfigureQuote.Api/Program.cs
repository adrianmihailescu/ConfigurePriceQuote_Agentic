using System.Text.Json;
using ConfigureQuote.Api.Data;
using ConfigureQuote.Api.Models;
using ConfigureQuote.Api.Services;
using ConfigureQuote.Api.Services.Agents;
using ConfigureQuote.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Quotes")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=ConfigureQuoteDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<QuoteDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<IProductAgent, ProductAgent>();
builder.Services.AddScoped<IValidationAgent, ValidationAgent>();
builder.Services.AddScoped<IPricingAgent, PricingAgent>();
builder.Services.AddScoped<IQuotationAgent, QuotationAgent>();
builder.Services.AddScoped<AgentOrchestrator>();
builder.Services.AddProblemDetails();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/catalog", async (
    QuoteDbContext db,
    DatabaseInitializer initializer,
    CancellationToken cancellationToken) =>
{
    await initializer.EnsureCreatedAsync(cancellationToken);
    var products = await db.Products.AsNoTracking().ToListAsync(cancellationToken);
    return Results.Ok(SampleCatalog.ToCatalogProducts(products));
});

app.MapGet("/api/quotes", async (
    QuoteDbContext db,
    DatabaseInitializer initializer,
    IQuotationAgent quotationAgent,
    CancellationToken cancellationToken) =>
{
    await initializer.EnsureCreatedAsync(cancellationToken);
    var quotes = await db.Quotes.AsNoTracking()
        .OrderByDescending(quote => quote.CreatedAt)
        .Take(50)
        .ToListAsync(cancellationToken);
    return Results.Ok(quotes.Select(quotationAgent.CreateSummary));
});

app.MapPost("/api/assistant/chat", async (
    ChatRequest request,
    DatabaseInitializer initializer,
    AgentOrchestrator orchestrator,
    CancellationToken cancellationToken) =>
{
    var selectedOptions = request.SelectedOptions;
    if (string.IsNullOrWhiteSpace(request.ProductId) || selectedOptions is null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = ["A product and selected options are required."]
        });
    }

    await initializer.EnsureCreatedAsync(cancellationToken);
    try
    {
        return Results.Ok(await orchestrator.ConfigureAsync(
            new ChatRequest(request.ProductId, selectedOptions, request.Message),
            cancellationToken));
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new { error = exception.Message });
    }
});

app.MapPost("/api/quotes", async (
    CreateQuoteRequest request,
    QuoteDbContext db,
    DatabaseInitializer initializer,
    AgentOrchestrator orchestrator,
    IQuotationAgent quotationAgent,
    CancellationToken cancellationToken) =>
{
    var errors = new Dictionary<string, string[]>();
    var selectedOptions = request.SelectedOptions;
    if (string.IsNullOrWhiteSpace(request.CustomerName))
    {
        errors["customerName"] = ["Customer name is required."];
    }

    if (string.IsNullOrWhiteSpace(request.CustomerEmail) ||
        !System.Net.Mail.MailAddress.TryCreate(request.CustomerEmail, out _))
    {
        errors["customerEmail"] = ["Enter a valid customer email address."];
    }

    if (string.IsNullOrWhiteSpace(request.ProductId) || selectedOptions is null)
    {
        errors["configuration"] = ["A product and selected options are required."];
    }

    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    if (selectedOptions is null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["configuration"] = ["A product and selected options are required."]
        });
    }

    await initializer.EnsureCreatedAsync(cancellationToken);
    ChatResponse result;
    try
    {
        result = await orchestrator.ConfigureAsync(
            new ChatRequest(request.ProductId, selectedOptions, request.Notes),
            cancellationToken);
    }
    catch (KeyNotFoundException exception)
    {
        return Results.NotFound(new { error = exception.Message });
    }

    if (!result.IsValid)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["configuration"] = result.Issues.ToArray()
        });
    }

    var product = await db.Products.AsNoTracking()
        .SingleAsync(item => item.Id == request.ProductId, cancellationToken);
    var quote = new QuoteEntity
    {
        QuoteNumber = $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant(),
        CustomerName = request.CustomerName.Trim(),
        CustomerEmail = request.CustomerEmail.Trim(),
        ProductId = product.Id,
        ProductName = product.Name,
        SelectedOptionsJson = JsonSerializer.Serialize(selectedOptions),
        Notes = request.Notes?.Trim() ?? string.Empty,
        Total = result.Total,
        Status = "Draft",
        CreatedAt = DateTime.UtcNow
    };

    db.Quotes.Add(quote);
    await db.SaveChangesAsync(cancellationToken);
    return Results.Created($"/api/quotes/{quote.Id}", quotationAgent.CreateSummary(quote));
});

app.Run();

public partial class Program;
