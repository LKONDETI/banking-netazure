using FinCore.SharedKernel.ValueObjects;

namespace FinCore.Core.Entities.AccountAggregate;

public sealed class AccountId : ValueObject
{
    public string Value { get; }

    public AccountId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AccountId cannot be empty.", nameof(value));
        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
