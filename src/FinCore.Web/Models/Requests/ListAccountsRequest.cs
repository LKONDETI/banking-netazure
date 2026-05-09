using System.ComponentModel.DataAnnotations;
using FinCore.Core.Enums;

namespace FinCore.Web.Models.Requests;

/// <summary>
/// Route-level request model for GET /api/accounts/{id}.
/// </summary>
public sealed record ListAccountsRequest
{
    /// <summary>
    /// Numeric bitmask representing the <see cref="AccountTypes"/> flags to include.
    /// Default 15 = Deposits | Loans | FromAccounts | ToAccounts.
    /// </summary>
    [Range(1, 63, ErrorMessage = "Account type filter must be between 1 and 63.")]
    public int Id { get; init; } = 15;

    public AccountTypes ToAccountTypes() => (AccountTypes)Id;
}
