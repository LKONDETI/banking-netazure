namespace FinCore.DB.Base;

/// <summary>
/// Strongly-typed settings for IBM DB2 connection.
/// Bound from IConfiguration — connection string injected via User Secrets locally,
/// Azure Key Vault in production. Never hardcoded.
/// </summary>
public sealed class ConnectionSettings
{
    public const string SectionName = "ConnectionStrings";

    /// <summary>
    /// The IBM DB2 connection string for the WIS (banking) database.
    /// Left empty in appsettings.json; provided via User Secrets or Key Vault.
    /// </summary>
    public string WisConnectionString { get; init; } = string.Empty;
}
