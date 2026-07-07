# MyIndustry Testing Guide

## Backend (.NET)

```bash
cd MyIndustry
dotnet test MyIndustry.Tests/MyIndustry.Tests.csproj
```

### Test categories

| Folder | Purpose |
|--------|---------|
| `Unit/` | Handler, service, helper unit tests (Moq + FluentAssertions) |
| `Smoke/` | API endpoint smoke tests via `WebApplicationFactory` |
| `Integration/` | End-to-end handler + DB flows with InMemory database |
| `Fixtures/` | `ApiWebApplicationFactory`, `IdentityWebApplicationFactory` |
| `Helpers/` | `TestAuthHelper`, `TestDataBuilder`, `TestDbContextFactory` |

### Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Target: **80%+** backend line coverage.

## Frontend (React)

```bash
cd MyIndustry.UI
npm ci
npm run test:ci
```

### Test utilities

- `src/setupTests.js` — Jest setup, optional MSW
- `src/test-utils/renderWithProviders.jsx` — Router + Theme + Auth wrappers
- `src/mocks/` — MSW handlers for API mocking

Target: **70%+** frontend line coverage.

## E2E (Playwright)

```bash
cd MyIndustry.UI
npm ci
npx playwright install chromium
npm start   # in separate terminal
npm run test:e2e
```

Configure test users via environment variables:

- `E2E_CUSTOMER_EMAIL` / `E2E_CUSTOMER_PASSWORD`
- `E2E_SELLER_EMAIL` / `E2E_SELLER_PASSWORD`
- `E2E_ADMIN_EMAIL` / `E2E_ADMIN_PASSWORD`

## CI

- Backend: `.github/workflows/tests.yml` in `MyIndustry` repo
- Frontend + E2E: `.github/workflows/tests.yml` in `MyIndustry.UI` repo

## Adding new tests

1. Add handler unit test under `MyIndustry.Tests/Unit/{Domain}/`
2. Add smoke test for new API endpoint under `MyIndustry.Tests/Smoke/`
3. Update `TEST_COVERAGE.md` checklist
4. Run `dotnet build` before commit (backend) and `npm run build` (frontend if changed)
