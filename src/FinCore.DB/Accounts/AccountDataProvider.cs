using System.Data;
using System.Data.Common;
using FinCore.Application.DataGateways;
using FinCore.Core.Entities.AccountAggregate;
using FinCore.Core.Enums;
using FinCore.DB.Base;
using FinCore.DB.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinCore.DB.Accounts;

/// <summary>
/// Fetches accounts from IBM DB2 using raw ADO.NET.
/// A fresh connection is opened per call — pool is managed by the IBM driver.
/// The IVendorFieldConfiguration stub provides dynamic SQL field names.
/// </summary>
public sealed class AccountDataProvider : AsyncDataProvider, IAccountDataProvider
{
    private readonly IVendorFieldConfiguration _fieldConfig;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<AccountDataProvider> _logger;

    public AccountDataProvider(
        IConfiguration configuration,
        IVendorFieldConfiguration fieldConfig,
        IDbConnectionFactory connectionFactory,
        ILogger<AccountDataProvider> logger)
        : base(configuration)
    {
        _fieldConfig = fieldConfig;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Account>> GetAccountsAsync(
        string entity,
        string taxId,
        AccountTypes filter,
        CancellationToken ct = default)
    {
        var connectionString = GetConnectionString();
        var accounts = new List<Account>();

        await using var connection = _connectionFactory.CreateConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = BuildAccountQuery(filter);

        AddParameter(command, "@entity", entity);
        AddParameter(command, "@taxId", taxId);

        _logger.LogDebug("Executing GetAccountsAsync for entity={Entity}", entity);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var typeCategory = new AccountTypeCategory(
                reader.GetEnumSafe(_fieldConfig.PrimaryAccountTypeField, PrimaryAccountType.Unknown),
                reader.GetEnumSafe(_fieldConfig.SubAccountTypeField, SubAccountType.Unknown));

            var accountTypeFlags = ParseAccountTypeFlags(
                reader.GetStringSafe(_fieldConfig.AccountTypeFlagField));

            var account = Account.Create(
                id: reader.GetStringSafe(_fieldConfig.AccountIdField),
                accountNumber: reader.GetStringSafe(_fieldConfig.AccountNumberField),
                displayName: reader.GetStringSafe(_fieldConfig.DisplayNameField),
                availableBalance: reader.GetDecimalSafe(_fieldConfig.AvailableBalanceField),
                currentBalance: reader.GetDecimalSafe(_fieldConfig.CurrentBalanceField),
                typeCategory: typeCategory,
                accountTypeFlags: accountTypeFlags,
                entity: entity,
                isActive: reader.GetBooleanSafe(_fieldConfig.IsActiveField));

            accounts.Add(account);
        }

        return accounts.AsReadOnly();
    }

    private string BuildAccountQuery(AccountTypes filter)
    {
        // Build type filter clause from flags — maps flags to DB2 type codes
        var typeFilters = new List<string>();

        if (filter.HasFlag(AccountTypes.Deposits))
            typeFilters.Add($"'{_fieldConfig.DepositTypeCode}'");
        if (filter.HasFlag(AccountTypes.Loans))
            typeFilters.Add($"'{_fieldConfig.LoanTypeCode}'");

        var typeClause = typeFilters.Count > 0
            ? $"AND {_fieldConfig.PrimaryAccountTypeField} IN ({string.Join(", ", typeFilters)})"
            : string.Empty;

        return $"""
            SELECT
                {_fieldConfig.AccountIdField},
                {_fieldConfig.AccountNumberField},
                {_fieldConfig.DisplayNameField},
                {_fieldConfig.AvailableBalanceField},
                {_fieldConfig.CurrentBalanceField},
                {_fieldConfig.PrimaryAccountTypeField},
                {_fieldConfig.SubAccountTypeField},
                {_fieldConfig.AccountTypeFlagField},
                {_fieldConfig.IsActiveField}
            FROM {_fieldConfig.AccountTableName}
            WHERE {_fieldConfig.EntityField} = @entity
              AND {_fieldConfig.TaxIdField} = @taxId
              {typeClause}
            ORDER BY {_fieldConfig.AccountNumberField}
            """;
    }

    private static AccountTypes ParseAccountTypeFlags(string rawFlags)
    {
        if (string.IsNullOrWhiteSpace(rawFlags))
            return AccountTypes.None;

        if (int.TryParse(rawFlags.Trim(), out var numericFlag))
            return (AccountTypes)numericFlag;

        return AccountTypes.None;
    }

    private static void AddParameter(DbCommand command, string name, string value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        param.DbType = DbType.String;
        command.Parameters.Add(param);
    }
}
