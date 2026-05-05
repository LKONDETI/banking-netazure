using FinCore.Core.Entities.UserAggregate;

namespace FinCore.Application.DataGateways;

public interface IAliasDataProvider
{
    Task<IReadOnlyDictionary<AliasKey, string>> GetAliasesAsync(
        string entity,
        string accessId,
        CancellationToken ct = default);
}
