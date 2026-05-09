using FinCore.Application.Accounts.Queries.ListAccounts;
using FinCore.Application.DataGateways;
using FinCore.Application.Services;
using FinCore.Core.Entities.AccountAggregate;
using FinCore.Core.Entities.UserAggregate;
using FinCore.Core.Enums;
using FinCore.SharedKernel.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinCore.UnitTests.Accounts;

public sealed class ListAccountsQueryHandlerTests
{
    private readonly Mock<IAccountDataProvider> _accountProviderMock = new();
    private readonly Mock<IAliasDataProvider> _aliasProviderMock = new();
    private readonly Mock<ICurrentUserContext> _userContextMock = new();
    private readonly ListAccountsQueryHandler _sut;

    public ListAccountsQueryHandlerTests()
    {
        _userContextMock.Setup(u => u.Entity).Returns("FI001");
        _userContextMock.Setup(u => u.AccessId).Returns("jdoe");
        _userContextMock.Setup(u => u.TaxId).Returns("123456789");

        _sut = new ListAccountsQueryHandler(
            _accountProviderMock.Object,
            _aliasProviderMock.Object,
            _userContextMock.Object,
            NullLogger<ListAccountsQueryHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenAccountsExistWithAlias_ShouldReturnDtosWithAlias()
    {
        // Arrange
        var account = Account.Create(
            id: "ACC001",
            accountNumber: "0001234567",
            displayName: "Checking Account",
            availableBalance: 1500.00m,
            currentBalance: 1500.00m,
            typeCategory: new AccountTypeCategory(PrimaryAccountType.Deposit, SubAccountType.Checking),
            accountTypeFlags: AccountTypes.Deposits,
            entity: "FI001");

        var aliasKey = new AliasKey("FI001", "0001234567");
        var aliases = new Dictionary<AliasKey, string>
        {
            [aliasKey] = "My Checking"
        };

        _accountProviderMock
            .Setup(p => p.GetAccountsAsync("FI001", "123456789", AccountTypes.Deposits, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account> { account }.AsReadOnly());

        _aliasProviderMock
            .Setup(p => p.GetAliasesAsync("FI001", "jdoe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(aliases);

        var query = new ListAccountsQuery(AccountTypes.Deposits);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Accounts);

        var dto = result.Value.Accounts[0];
        Assert.Equal("ACC001", dto.Id);
        Assert.Equal("0001234567", dto.AccountNumber);
        Assert.Equal("My Checking", dto.Alias);
        Assert.Equal(1500.00m, dto.AvailableBalance);
        Assert.Equal(PrimaryAccountType.Deposit, dto.PrimaryType);
    }

    [Fact]
    public async Task Handle_WhenNoAliasForAccount_ShouldReturnEmptyAlias()
    {
        // Arrange
        var account = Account.Create(
            id: "ACC002",
            accountNumber: "9998887776",
            displayName: "Savings Account",
            availableBalance: 5000m,
            currentBalance: 5000m,
            typeCategory: new AccountTypeCategory(PrimaryAccountType.Deposit, SubAccountType.Savings),
            accountTypeFlags: AccountTypes.Deposits,
            entity: "FI001");

        _accountProviderMock
            .Setup(p => p.GetAccountsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AccountTypes>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account> { account }.AsReadOnly());

        _aliasProviderMock
            .Setup(p => p.GetAliasesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<AliasKey, string>());

        var query = new ListAccountsQuery(AccountTypes.Deposits);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value!.Accounts[0].Alias);
    }

    [Fact]
    public async Task Handle_WhenNoAccountsReturned_ShouldReturnSuccessWithEmptyList()
    {
        // Arrange
        _accountProviderMock
            .Setup(p => p.GetAccountsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AccountTypes>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account>().AsReadOnly());

        _aliasProviderMock
            .Setup(p => p.GetAliasesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<AliasKey, string>());

        var query = new ListAccountsQuery(AccountTypes.Deposits | AccountTypes.Loans);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Accounts);
    }

    [Fact]
    public async Task Handle_ShouldCallBothProvidersViaConcurrentPattern()
    {
        // Arrange — verify Task.WhenAll pattern: both providers are invoked
        _accountProviderMock
            .Setup(p => p.GetAccountsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AccountTypes>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account>().AsReadOnly());

        _aliasProviderMock
            .Setup(p => p.GetAliasesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<AliasKey, string>());

        var query = new ListAccountsQuery(AccountTypes.Deposits);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert both were called exactly once
        _accountProviderMock.Verify(
            p => p.GetAccountsAsync("FI001", "123456789", AccountTypes.Deposits, It.IsAny<CancellationToken>()),
            Times.Once);

        _aliasProviderMock.Verify(
            p => p.GetAliasesAsync("FI001", "jdoe", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenResultIsSuccess_ShouldHaveCorrectCategory()
    {
        // Arrange
        _accountProviderMock
            .Setup(p => p.GetAccountsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AccountTypes>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account>().AsReadOnly());

        _aliasProviderMock
            .Setup(p => p.GetAliasesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<AliasKey, string>());

        var query = new ListAccountsQuery(AccountTypes.Deposits);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(ResultCategory.Success, result.Category);
    }
}
