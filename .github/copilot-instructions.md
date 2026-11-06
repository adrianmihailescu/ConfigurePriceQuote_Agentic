# My Personal Company Configure & Quote

## Architecture
- `frontend/` is the React + TypeScript sales workspace. Keep API calls in `src/api.ts` and shared response types in `src/types.ts`.
- `backend/ConfigureQuote.Api/` is the ASP.NET Core API. Keep HTTP endpoints thin; business-agent responsibilities belong in `Services/`.
- The orchestrator coordinates independent product, pricing, validation, and quotation services. Agent behavior is documented in `.github/agents/`.
- SQL Server persistence is managed with EF Core. Do not treat an in-memory UI fallback as a persisted quote.

## Validation
- Run `dotnet build backend\ConfigureQuote.Api\ConfigureQuote.Api.csproj`.
- Run `cd frontend; npm run build` and `npm run test:e2e`.
- Keep Playwright tests focused on user-visible configuration and quotation behavior.

## Conventions
- Use clear, typed API contracts and return actionable HTTP errors for invalid input or database failures.
- Keep sample catalog content consistent between the API seed data and frontend fallback data.
- Use accessible labels, keyboard-operable controls, and responsive layouts.
