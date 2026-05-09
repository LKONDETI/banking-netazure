using FluentValidation.TestHelper;
using FinCore.Application.Accounts.Queries.ListAccounts;
using FinCore.Core.Enums;

namespace FinCore.UnitTests.Accounts;

public sealed class ListAccountsQueryValidatorTests
{
    private readonly ListAccountsQueryValidator _sut = new();

    [Fact]
    public async Task Validate_WhenFilterIsNone_ShouldHaveValidationError()
    {
        var query = new ListAccountsQuery(AccountTypes.None);
        var result = await _sut.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(q => q.Filter)
              .WithErrorCode("ACCOUNT_TYPES_NONE");
    }

    [Theory]
    [InlineData(AccountTypes.Deposits)]
    [InlineData(AccountTypes.Loans)]
    [InlineData(AccountTypes.Deposits | AccountTypes.Loans)]
    [InlineData(AccountTypes.Deposits | AccountTypes.Loans | AccountTypes.FromAccounts | AccountTypes.ToAccounts)]
    [InlineData(AccountTypes.BillPay | AccountTypes.ACH)]
    public async Task Validate_WhenFilterHasValidFlags_ShouldNotHaveValidationErrors(AccountTypes filter)
    {
        var query = new ListAccountsQuery(filter);
        var result = await _sut.TestValidateAsync(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WhenFilterHasInvalidBits_ShouldHaveValidationError()
    {
        // 128 is not a valid AccountTypes flag (max is 32)
        var query = new ListAccountsQuery((AccountTypes)128);
        var result = await _sut.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(q => q.Filter)
              .WithErrorCode("ACCOUNT_TYPES_INVALID_FLAGS");
    }

    [Fact]
    public async Task Validate_WhenFilterIsAllValidFlags_ShouldNotHaveValidationErrors()
    {
        var allFlags = AccountTypes.Deposits | AccountTypes.Loans | AccountTypes.FromAccounts
                     | AccountTypes.ToAccounts | AccountTypes.BillPay | AccountTypes.ACH;

        var query = new ListAccountsQuery(allFlags);
        var result = await _sut.TestValidateAsync(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
