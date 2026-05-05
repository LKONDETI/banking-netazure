using FluentValidation;
using FinCore.Core.Enums;

namespace FinCore.Application.Accounts.Queries.ListAccounts;

public sealed class ListAccountsQueryValidator : AbstractValidator<ListAccountsQuery>
{
    // All valid AccountTypes flag values except None
    private static readonly int MaxValidFlags =
        (int)(AccountTypes.Deposits | AccountTypes.Loans | AccountTypes.FromAccounts |
              AccountTypes.ToAccounts | AccountTypes.BillPay | AccountTypes.ACH);

    public ListAccountsQueryValidator()
    {
        RuleFor(q => q.Filter)
            .Must(f => f != AccountTypes.None)
            .WithMessage("AccountTypes filter cannot be None (0). At least one flag must be set.")
            .WithErrorCode("ACCOUNT_TYPES_NONE");

        RuleFor(q => q.Filter)
            .Must(f => ((int)f & ~MaxValidFlags) == 0)
            .WithMessage($"AccountTypes filter contains invalid flag bits. Valid values are combinations of: {string.Join(", ", Enum.GetNames<AccountTypes>()[1..])}")
            .WithErrorCode("ACCOUNT_TYPES_INVALID_FLAGS");
    }
}
