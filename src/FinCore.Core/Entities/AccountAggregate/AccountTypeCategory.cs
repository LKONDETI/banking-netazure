using FinCore.Core.Enums;
using FinCore.SharedKernel.ValueObjects;

namespace FinCore.Core.Entities.AccountAggregate;

public sealed class AccountTypeCategory : ValueObject
{
    public PrimaryAccountType Primary { get; }
    public SubAccountType Sub { get; }

    public AccountTypeCategory(PrimaryAccountType primary, SubAccountType sub)
    {
        Primary = primary;
        Sub = sub;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Primary;
        yield return Sub;
    }
}
