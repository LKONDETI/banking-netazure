using System.Data.Common;

namespace FinCore.DB.Base;

/// <summary>
/// Creates IBM DB2 connections using the registered DbProviderFactory.
/// The IBM.Data.DB2.Core provider must be registered at startup via
/// DbProviderFactories.RegisterFactory("IBM.Data.DB2", IBM.Data.Db2.DB2Factory.Instance).
/// </summary>
public sealed class Db2ConnectionFactory : IDbConnectionFactory
{
    private const string Db2ProviderName = "IBM.Data.DB2";

    public DbConnection CreateConnection(string connectionString)
    {
        var factory = DbProviderFactories.GetFactory(Db2ProviderName);
        var connection = factory.CreateConnection()
            ?? throw new InvalidOperationException(
                $"DbProviderFactory '{Db2ProviderName}' returned a null connection. " +
                "Ensure IBM.Data.DB2.Core is installed and the provider is registered.");

        connection.ConnectionString = connectionString;
        return connection;
    }
}
