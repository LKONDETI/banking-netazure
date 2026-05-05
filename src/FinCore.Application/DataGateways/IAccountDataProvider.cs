using FinCore.Core.Entities.AccountAggregate;
using FinCore.Core.Enums;

namespace FinCore.Application.DataGateways;

public interface IAccountDataProvider
{
    Task<IReadOnlyList<Account>> GetAccountsAsync(
        string entity,
        string taxId,
        AccountTypes filter,
        CancellationToken ct = default);
}
