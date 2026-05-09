using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FinCore.Application.DataGateways;
using FinCore.Application.Services;
using FinCore.Core.Entities.AccountAggregate;
using FinCore.Core.Entities.UserAggregate;
using FinCore.Core.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FinCore.IntegrationTests.Controllers;

public sealed class AccountsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AccountsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private WebApplicationFactory<Program> CreateAuthenticatedFactory(
        Mock<IAccountDataProvider>? accountMock = null,
        Mock<IAliasDataProvider>? aliasMock = null)
    {
        accountMock ??= CreateDefaultAccountMock();
        aliasMock ??= CreateDefaultAliasMock();

        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace data providers with mocks
                services.AddScoped(_ => accountMock.Object);
                services.AddScoped(_ => aliasMock.Object);

                // Stub out ICurrentUserContext
                var userContextMock = new Mock<ICurrentUserContext>();
                userContextMock.Setup(u => u.Entity).Returns("FI001");
                userContextMock.Setup(u => u.AccessId).Returns("testuser");
                userContextMock.Setup(u => u.TaxId).Returns("999887777");
                services.AddScoped(_ => userContextMock.Object);

                // Replace auth with a test scheme that auto-authenticates
                services.AddAuthentication(TestAuthHandler.SchemeName)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            TestAuthHandler.SchemeName, _ => { });
            });
        });
    }

    [Fact]
    public async Task GetAccounts_WithDefaultFilter_Returns200WithAccounts()
    {
        // Arrange
        var client = CreateAuthenticatedFactory().CreateClient();

        // Act
        var response = await client.GetAsync("/api/accounts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("accountNumber", body);
    }

    [Fact]
    public async Task GetAccounts_WithExplicitFilter15_Returns200()
    {
        // Arrange
        var client = CreateAuthenticatedFactory().CreateClient();

        // Act
        var response = await client.GetAsync("/api/accounts/15");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_WithFilterZero_Returns422()
    {
        // Arrange
        var client = CreateAuthenticatedFactory().CreateClient();

        // Act — 0 = AccountTypes.None, rejected by FluentValidation
        var response = await client.GetAsync("/api/accounts/0");

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task GetAccounts_WithoutAuthentication_Returns401()
    {
        // Arrange — use the plain factory with no auth override
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/api/accounts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static Mock<IAccountDataProvider> CreateDefaultAccountMock()
    {
        var mock = new Mock<IAccountDataProvider>();
        var account = Account.Create(
            id: "ACC001",
            accountNumber: "0001234567",
            displayName: "Test Checking",
            availableBalance: 1000m,
            currentBalance: 1000m,
            typeCategory: new AccountTypeCategory(PrimaryAccountType.Deposit, SubAccountType.Checking),
            accountTypeFlags: AccountTypes.Deposits,
            entity: "FI001");

        mock.Setup(p => p.GetAccountsAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AccountTypes>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account> { account }.AsReadOnly());

        return mock;
    }

    private static Mock<IAliasDataProvider> CreateDefaultAliasMock()
    {
        var mock = new Mock<IAliasDataProvider>();
        mock.Setup(p => p.GetAliasesAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<AliasKey, string>());
        return mock;
    }
}

// ---------------------------------------------------------------------------
// Test authentication handler — auto-authenticates every request
// ---------------------------------------------------------------------------
internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim("entity", "FI001"),
            new Claim("accessid", "testuser"),
            new Claim("taxid", "999887777")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
