using FinCore.Core.Enums;
using FinCore.SharedKernel.Results;
using MediatR;

namespace FinCore.Application.Accounts.Queries.ListAccounts;

public sealed record ListAccountsQuery(AccountTypes Filter)
    : IRequest<UseCaseResult<ListAccountsResult>>;
