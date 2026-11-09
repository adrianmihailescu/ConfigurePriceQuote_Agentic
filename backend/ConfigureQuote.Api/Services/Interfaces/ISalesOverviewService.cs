using ConfigureQuote.Api.Models;

namespace ConfigureQuote.Api.Services.Interfaces;

public interface ISalesOverviewService
{
    Task<SalesOverview> GetAsync(CancellationToken cancellationToken);
}
