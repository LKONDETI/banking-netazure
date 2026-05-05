using FinCore.SharedKernel.ValueObjects;

namespace FinCore.Core.Entities.UserAggregate;

/// <summary>
/// Composite key used to look up an account alias: entity + account number.
/// </summary>
public sealed class AliasKey : ValueObject
{
    public string Entity { get; }
    public string AccountNumber { get; }

    public AliasKey(string entity, string accountNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        Entity = entity;
        AccountNumber = accountNumber;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Entity;
        yield return AccountNumber;
    }

    public override string ToString() => $"{Entity}:{AccountNumber}";
}
