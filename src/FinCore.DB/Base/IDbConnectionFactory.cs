using System.Data.Common;

namespace FinCore.DB.Base;

/// <summary>
/// Creates a new database connection. Abstracted to allow test doubles and
/// to decouple DB2-specific types from the provider implementations.
/// </summary>
public interface IDbConnectionFactory
{
    DbConnection CreateConnection(string connectionString);
}
