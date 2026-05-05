using FinCore.Application.Accounts.DTOs;

namespace FinCore.Application.Accounts.Queries.ListAccounts;

public sealed record ListAccountsResult(IReadOnlyList<AccountDto> Accounts);
