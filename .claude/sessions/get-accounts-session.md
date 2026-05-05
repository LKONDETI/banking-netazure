# Session: GET /api/accounts Endpoint
**Date:** 2026-05-04
**Status:** Implementation complete. Build passes (0 errors, 0 warnings). 13 unit tests pass.

---

## Next Step
**Stage 4: Code Quality Review** — Use the `code-reviewer` agent to check for SOLID violations, naming issues, .NET conventions, performance problems, and over-engineering.

Start prompt: "Review the GET /api/accounts implementation in /Users/srav/Documents/projects/CSharp/banking-netazure/src for code quality"

---

## What Was Completed

### Stage 1: Task Plan (27 tasks across 8 phases)

| Phase | Tasks | Description |
|---|---|---|
| 1 | 1–2 | Scaffolding — solution + 7 projects + NuGet packages |
| 2 | 3–4 | SharedKernel — BaseEntity, ValueObject, UseCaseResult, IAsyncDataProvider |
| 3 | 5–8 | Domain — Account, AccountId, PrimaryAccountType, SubAccountType, AccountTypes |
| 4 | 9–13 | Application — IAccountDataProvider, ListAccountsQuery, Handler, Validator, ValidationBehavior |
| 5 | 14–17 | DB — ConnectionSettings, AsyncDataProvider, DataReaderExtensions, AccountDataProvider, AliasDataProvider |
| 6 | 18–20 | API — iTellerApiControllerBase, AccountsController, AccountResponse DTOs |
| 7 | 21–22 | DI + Program.cs |
| 8 | 23–27 | Tests — 5 test classes, ~15 test cases |

### Stage 2: Architecture Design

**Pattern:** Clean Architecture + CQRS-via-MediatR + Repository pattern
**Database:** IBM DB2, raw ADO.NET (no EF Core)

**Project dependency graph:**
```
iTeller.SharedKernel
    ↑
iTeller.Core
    ↑
iTeller.Application
    ↑
iTeller.DB          iTeller.Infrastructure
    ↑                       ↑
              iTeller.Web
```

---

## File Structure to Create

```
src/
  iTeller.SharedKernel/
    Results/UseCaseResult.cs
    Results/ResultCategory.cs
    Abstractions/IAsyncDataProvider.cs
    PipelineBehaviors/ValidationBehavior.cs
    ValueObjects/ValueObject.cs

  iTeller.Core/
    Entities/AccountAggregate/Account.cs
    Entities/AccountAggregate/AccountId.cs
    Entities/AccountAggregate/AccountTypeCategory.cs
    Entities/UserAggregate/Alias.cs
    Entities/UserAggregate/AliasKey.cs
    Enums/AccountTypes.cs
    Enums/PrimaryAccountType.cs
    Enums/SubAccountType.cs

  iTeller.Application/
    Accounts/Queries/ListAccounts/ListAccountsQuery.cs
    Accounts/Queries/ListAccounts/ListAccountsQueryHandler.cs
    Accounts/Queries/ListAccounts/ListAccountsQueryValidator.cs
    Accounts/Queries/ListAccounts/ListAccountsResult.cs
    Accounts/DTOs/AccountDto.cs
    DataGateways/IAccountDataProvider.cs
    DataGateways/IAliasDataProvider.cs
    Services/ICurrentUserContext.cs

  iTeller.DB/
    Base/ConnectionSettings.cs
    Base/AsyncDataProvider.cs
    Accounts/AccountDataProvider.cs
    Accounts/AliasDataProvider.cs
    Extensions/DataReaderExtensions.cs

  iTeller.Infrastructure/
    Identity/CurrentUserContext.cs
    DependencyInjection/DefaultInfrastructureModule.cs

  iTeller.Web/
    Controllers/Base/iTellerApiControllerBase.cs
    Controllers/AccountsController.cs
    Models/Requests/ListAccountsRequest.cs
    Models/Responses/AccountResponse.cs
    Program.cs
    appsettings.json

tests/
  iTeller.UnitTests/
    Accounts/ListAccountsQueryHandlerTests.cs
    Accounts/ListAccountsQueryValidatorTests.cs
  iTeller.IntegrationTests/
    Controllers/AccountsControllerTests.cs
```

---

## Key Interface Signatures

### IAccountDataProvider
```csharp
public interface IAccountDataProvider
{
    Task<IReadOnlyList<Account>> GetAccountsAsync(
        string entity, string taxId, AccountTypes filter, CancellationToken ct = default);
}
```

### IAliasDataProvider
```csharp
public interface IAliasDataProvider
{
    Task<IReadOnlyDictionary<AliasKey, string>> GetAliasesAsync(
        string entity, string accessId, CancellationToken ct = default);
}
```

### ICurrentUserContext
```csharp
public interface ICurrentUserContext
{
    string Entity   { get; }  // "entity" claim — FI / tenant ID
    string AccessId { get; }  // username / login ID
    string TaxId    { get; }  // SSN/TaxId for DB2 lookup
}
```

### ListAccountsQuery (MediatR)
```csharp
public sealed record ListAccountsQuery(AccountTypes Filter)
    : IRequest<UseCaseResult<ListAccountsResult>>;
```

### AccountsController
```csharp
[ApiController, Route("api/accounts"), Authorize]
public sealed class AccountsController : iTellerApiControllerBase
{
    [HttpGet("{id:int?}")]
    public async Task<IActionResult> GetAccounts(
        [FromRoute] int id = 15, CancellationToken cancellationToken = default);
}
```

### AccountTypes Enum
```csharp
[Flags]
public enum AccountTypes
{
    None         = 0,
    Deposits     = 1,
    Loans        = 2,
    FromAccounts = 4,
    ToAccounts   = 8,
    BillPay      = 16,
    ACH          = 32
}
// Default id=15 → Deposits | Loans | FromAccounts | ToAccounts
```

---

## Architecture Decisions (Do Not Change)

1. **Task.WhenAll** for accounts + alias queries — run both DB2 calls concurrently
2. **Fresh DB2Connection per call** — no command reuse, pool managed by IBM driver
3. **ICurrentUserContext** in Application layer — never IHttpContextAccessor (testability)
4. **Separate AccountResponse from AccountDto** — API contract decoupled from Application
5. **FluentValidation rejects AccountTypes.None** (id=0) → 422, no DB call made
6. **No `.GetAwaiter().GetResult()` anywhere** — all claims resolution is synchronous
7. **Empty WisConnectionString in appsettings.json** — use User Secrets locally, Key Vault in prod
8. **IVendorFieldConfiguration** interface for dynamic SQL field names — stub with defaults initially

---

## Data Flow
```
GET /api/accounts/15
  → [Authorize] cookie middleware (populates entity, accessid, taxid claims)
  → AccountsController.GetAccounts(id: 15)
  → cast 15 → AccountTypes.Deposits|Loans|FromAccounts|ToAccounts
  → Mediator.Send(ListAccountsQuery(filter))
  → ValidationBehavior (rejects None/invalid flags → 422)
  → ListAccountsQueryHandler
      → Task.WhenAll(
          AccountDataProvider.GetAccountsAsync(entity, taxId, filter),
          AliasDataProvider.GetAliasesAsync(entity, accessId)
        )
      → merge aliases into accounts
      → map Account[] → AccountDto[]
      → return UseCaseResult.Success(ListAccountsResult)
  → ToActionResult → 200 OK with AccountResponse[]
```

---

## NuGet Packages Needed

| Project | Packages |
|---|---|
| SharedKernel | MediatR 12.x, FluentValidation 11.x |
| Core | (none) |
| Application | MediatR 12.x, FluentValidation 11.x |
| DB | IBM.Data.DB2.Core (latest), Microsoft.Extensions.Configuration.Abstractions |
| Infrastructure | MediatR 12.x, FluentValidation.DependencyInjectionExtensions, Microsoft.AspNetCore.Http.Abstractions |
| Web | Swashbuckle.AspNetCore, Microsoft.AspNetCore.Authentication.Cookies |
| Tests | xunit, Microsoft.NET.Test.Sdk, Moq, coverlet.collector |

---

## Remaining Stages
- [x] Stage 3: Implementation (coding-expert) — complete, 0 build errors, 13 tests pass
- [ ] Stage 4: Code Quality Review (code-reviewer)
- [ ] Stage 5: Security Review (security-reviewer)
- [ ] Stage 6: Write Tests (testing-expert)
- [ ] Stage 7: Final Verification (dotnet build + dotnet test)
- [ ] Stage 8: Final Summary (task-summarizer)
