using ConfigureQuote.Api.Data;
using ConfigureQuote.Api.Models;
using ConfigureQuote.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ConfigureQuote.Api.Services;

public sealed class SalesOverviewService(QuoteDbContext db) : ISalesOverviewService
{
    private static readonly string[] WonStatuses = ["approved", "won"];
    private static readonly string[] LostStatuses = ["lost", "rejected"];
    private static readonly string[] ClosedStatuses =
        ["approved", "won", "lost", "rejected", "cancelled", "canceled"];

    public async Task<SalesOverview> GetAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = thisMonthStart.AddMonths(1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var statusGroups = await db.Quotes
            .AsNoTracking()
            .GroupBy(quote => quote.Status.ToLower())
            .Select(quotes => new
            {
                Status = quotes.Key,
                TotalQuotes = quotes.Count(),
                QuotesThisMonth = quotes.Count(quote =>
                    quote.CreatedAt >= thisMonthStart && quote.CreatedAt < nextMonthStart),
                QuotesLastMonth = quotes.Count(quote =>
                    quote.CreatedAt >= lastMonthStart && quote.CreatedAt < thisMonthStart),
                TotalValue = quotes.Sum(quote => (decimal?)quote.Total) ?? 0m
            })
            .ToListAsync(cancellationToken);

        var totalQuotes = statusGroups.Sum(group => group.TotalQuotes);
        var quotesThisMonth = statusGroups.Sum(group => group.QuotesThisMonth);
        var quotesLastMonth = statusGroups.Sum(group => group.QuotesLastMonth);
        var openGroups = statusGroups.Where(group => !ClosedStatuses.Contains(group.Status)).ToArray();
        var openQuotes = openGroups.Sum(group => group.TotalQuotes);
        var pipelineValue = openGroups.Sum(group => group.TotalValue);
        var wonQuotes = statusGroups
            .Where(group => WonStatuses.Contains(group.Status))
            .Sum(group => group.TotalQuotes);
        var lostQuotes = statusGroups
            .Where(group => LostStatuses.Contains(group.Status))
            .Sum(group => group.TotalQuotes);
        var decidedQuotes = wonQuotes + lostQuotes;
        decimal? winRate = decidedQuotes == 0
            ? null
            : decimal.Round(wonQuotes * 100m / decidedQuotes, 1);

        return new SalesOverview(
            quotesThisMonth,
            quotesLastMonth,
            totalQuotes,
            openQuotes,
            pipelineValue,
            winRate,
            decidedQuotes);
    }
}
