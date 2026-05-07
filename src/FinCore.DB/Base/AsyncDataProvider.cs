using FinCore.SharedKernel.Abstractions;
using Microsoft.Extensions.Configuration;

namespace FinCore.DB.Base;

/// <summary>
/// Base class for IBM DB2 data providers.
/// A fresh connection is opened per call — the IBM driver manages pooling.
/// </summary>
public abstract class AsyncDataProvider : IAsyncDataProvider
{
    private readonly IConfiguration _configuration;

    protected AsyncDataProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Returns the WIS DB2 connection string.
    /// Resolved from configuration at call time — supports User Secrets + Key Vault rotation.
    /// </summary>
    protected string GetConnectionString()
    {
        var connectionString = _configuration.GetConnectionString("WisConnectionString");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "WisConnectionString is not configured. " +
                "Set it via User Secrets (development) or Azure Key Vault (production).");

        return connectionString;
    }
}
