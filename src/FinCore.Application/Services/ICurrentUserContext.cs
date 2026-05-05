namespace FinCore.Application.Services;

/// <summary>
/// Provides the authenticated user's identity values extracted from claims.
/// Abstracted here (not IHttpContextAccessor) to keep the Application layer testable.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>FI / tenant ID — "entity" claim.</summary>
    string Entity { get; }

    /// <summary>Username / login ID — "accessid" claim.</summary>
    string AccessId { get; }

    /// <summary>SSN/TaxId for DB2 lookup — "taxid" claim.</summary>
    string TaxId { get; }
}
