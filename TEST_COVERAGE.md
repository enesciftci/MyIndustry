# MyIndustry Test Coverage Tracker

Last updated: implementation of full A-Z test plan.

## Backend Summary

| Layer | Total | Tested | Coverage |
|-------|-------|--------|----------|
| MediatR Handlers | 61 | 61 | Unit + Integration |
| API Endpoints (MyIndustry.Api) | 78 | 78 | Smoke |
| Identity API Endpoints | 18 | 18 | Smoke |
| Identity Services | 27 methods | 27 | Unit |
| Container/Middleware | 6 | 6 | Unit |
| Gateway | 1 | 1 | Unit |
| Queue Consumers | 4 | 4 | Unit |
| Repository (GenericRepository) | 17 methods | 17 | Unit |
| Helpers | SlugHelper, SearchTermHelper, ResponseBase, SecurityProvider | All | Unit |
| Image Storage | 2 | 2 | Unit |

## Frontend Summary

| Layer | Files | Tests |
|-------|-------|-------|
| Utils + API + Middleware | 5 | Unit |
| Contexts + Routing | 6 | Unit |
| Components | 20 | Component |
| Pages (auth, public, admin, seller, legacy) | 42 | Integration |
| E2E Scenarios | 35 | Playwright |

## Handler Checklist (61/61)

All handlers in `MyIndustry.ApplicationService/Handler/` have unit tests. See `MyIndustry.Tests/HandlerTestCoverage.md`.

### Remaining handler deep-dive

Each handler should have: happy path, not found, business rule, edge case. Review `HandlerTestCoverage.md` test counts and extend where count < 3.

## API Endpoint Checklist

### MyIndustry.Api

- [x] AdminController (5)
- [x] CategoryController (8)
- [x] FavoriteController (4)
- [x] InternalController (1)
- [x] LegalDocumentController (7)
- [x] LocationController (6)
- [x] MessageController (6)
- [x] SellerController (6)
- [x] SellerSubscriptionController (3)
- [x] ServiceController (11)
- [x] SEOController (3)
- [x] SubscriptionPlanController (5)
- [x] SupportTicketController (3)
- [x] BaseController helpers (5)

### Identity.Api

- [x] AuthController (16)
- [x] AdminController (3)

## Run Commands

```bash
# All backend
dotnet test MyIndustry.Tests/MyIndustry.Tests.csproj

# Smoke only
dotnet test --filter "FullyQualifiedName~Smoke"

# Integration only
dotnet test --filter "FullyQualifiedName~Integration"

# Frontend
cd MyIndustry.UI && npm run test:ci

# E2E
cd MyIndustry.UI && npm run test:e2e
```
