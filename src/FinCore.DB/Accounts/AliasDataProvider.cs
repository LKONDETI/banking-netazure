using System.Data;
using System.Data.Common;
using FinCore.Application.DataGateways;
using FinCore.Core.Entities.UserAggregate;
using FinCore.DB.Base;
using FinCore.DB.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinCore.DB.Accounts;

/// <summary>
/// Fetches user-defined account aliases from IBM DB2.
/// A fresh connection is opened per call — pool is managed by the IBM driver.
/// </summary>
public sealed class AliasDataProvider : AsyncDataProvider, IAliasDataProvider
{
    private readonly IVendorFieldConfiguration _fieldConfig;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<AliasDataProvider> _logger;

    public AliasDataProvider(
        IConfiguration configuration,
        IVendorFieldConfiguration fieldConfig,
        IDbConnectionFactory connectionFactory,
        ILogger<AliasDataProvider> logger)
        : base(configuration)
    {
        _fieldConfig = fieldConfig;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<AliasKey, string>> GetAliasesAsync(
        string entity,
        string accessId,
        CancellationToken ct = default)
    {
        var connectionString = GetConnectionString();
        var aliases = new Dictionary<AliasKey, string>();

        await using var connection = _connectionFactory.CreateConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = $"""
            SELECT
                {_fieldConfig.AliasAccountNumberField},
                {_fieldConfig.AliasNameField}
            FROM {_fieldConfig.AliasTableName}
            WHERE {_fieldConfig.AliasEntityField} = @entity
              AND {_fieldConfig.AliasAccessIdField} = @accessId
            """;

        AddParameter(command, "@entity", entity);
        AddParameter(command, "@accessId", accessId);

        _logger.LogDebug("Executing GetAliasesAsync for entity={Entity}, accessId={AccessId}", entity, accessId);

        await using var reader = await command.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var accountNumber = reader.GetStringSafe(_fieldConfig.AliasAccountNumberField);
            var aliasName = reader.GetStringSafe(_fieldConfig.AliasNameField);

            if (!string.IsNullOrWhiteSpace(accountNumber))
                aliases[new AliasKey(entity, accountNumber)] = aliasName;
        }

        return aliases;
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
