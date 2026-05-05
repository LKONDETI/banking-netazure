using FinCore.Application.Accounts.DTOs;
using FinCore.Application.DataGateways;
using FinCore.Application.Services;
using FinCore.Core.Entities.UserAggregate;
using FinCore.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinCore.Application.Accounts.Queries.ListAccounts;

public sealed class ListAccountsQueryHandler
    : IRequestHandler<ListAccountsQuery, UseCaseResult<ListAccountsResult>>
{
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly IAliasDataProvider _aliasDataProvider;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<ListAccountsQueryHandler> _logger;

    public ListAccountsQueryHandler(
        IAccountDataProvider accountDataProvider,
        IAliasDataProvider aliasDataProvider,
        ICurrentUserContext userContext,
        ILogger<ListAccountsQueryHandler> logger)
    {
        _accountDataProvider = accountDataProvider;
        _aliasDataProvider = aliasDataProvider;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<UseCaseResult<ListAccountsResult>> Handle(
        ListAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var entity = _userContext.Entity;
        var accessId = _userContext.AccessId;
        var taxId = _userContext.TaxId;

        _logger.LogDebug(
            "Fetching accounts for entity={Entity}, accessId={AccessId}, filter={Filter}",
            entity, accessId, request.Filter);

        // Run both DB2 queries concurrently — architecture decision #1
        var accountsTask = _accountDataProvider.GetAccountsAsync(entity, taxId, request.Filter, cancellationToken);
        var aliasesTask = _aliasDataProvider.GetAliasesAsync(entity, accessId, cancellationToken);

        await Task.WhenAll(accountsTask, aliasesTask);

        var accounts = await accountsTask;
        var aliases = await aliasesTask;

        // Merge aliases into accounts
        var dtos = accounts
            .Select(account =>
            {
                var aliasKey = new AliasKey(entity, account.AccountNumber);
                aliases.TryGetValue(aliasKey, out var aliasName);
                account.WithAlias(aliasName ?? string.Empty);

                return new AccountDto(
                    Id: account.Id.Value,
                    AccountNumber: account.AccountNumber,
                    DisplayName: account.DisplayName,
                    Alias: account.Alias,
                    AvailableBalance: account.AvailableBalance,
                    CurrentBalance: account.CurrentBalance,
                    PrimaryType: account.TypeCategory.Primary,
                    SubType: account.TypeCategory.Sub,
                    AccountTypeFlags: account.AccountTypeFlags,
                    IsActive: account.IsActive);
            })
            .ToList()
            .AsReadOnly();

        _logger.LogInformation(
            "Returned {Count} accounts for entity={Entity}", dtos.Count, entity);

        return UseCaseResult<ListAccountsResult>.Success(new ListAccountsResult(dtos));
    }
}
