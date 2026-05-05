using FinCore.Core.Enums;

namespace FinCore.Core.Entities.AccountAggregate;

public sealed class Account
{
    public AccountId Id { get; private set; }
    public string AccountNumber { get; private set; }
    public string DisplayName { get; private set; }
    public string Alias { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public AccountTypeCategory TypeCategory { get; private set; }
    public AccountTypes AccountTypeFlags { get; private set; }
    public string Entity { get; private set; }
    public bool IsActive { get; private set; }

    private Account()
    {
        // Required by factory method — suppress nullable warnings
        Id = null!;
        AccountNumber = null!;
        DisplayName = null!;
        Alias = string.Empty;
        TypeCategory = null!;
        Entity = null!;
    }

    public static Account Create(
        string id,
        string accountNumber,
        string displayName,
        decimal availableBalance,
        decimal currentBalance,
        AccountTypeCategory typeCategory,
        AccountTypes accountTypeFlags,
        string entity,
        bool isActive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity);

        return new Account
        {
            Id = new AccountId(id),
            AccountNumber = accountNumber,
            DisplayName = displayName,
            Alias = string.Empty,
            AvailableBalance = availableBalance,
            CurrentBalance = currentBalance,
            TypeCategory = typeCategory,
            AccountTypeFlags = accountTypeFlags,
            Entity = entity,
            IsActive = isActive
        };
    }

    /// <summary>
    /// Applies an alias name resolved from the alias data provider.
    /// </summary>
    public Account WithAlias(string alias)
    {
        Alias = alias ?? string.Empty;
        return this;
    }
}
