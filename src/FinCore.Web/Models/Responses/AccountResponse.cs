using FinCore.Application.Accounts.DTOs;
using FinCore.Core.Enums;

namespace FinCore.Web.Models.Responses;

/// <summary>
/// API response model for an account.
/// Deliberately decoupled from <see cref="AccountDto"/> — the API contract
/// can evolve independently of the application layer.
/// </summary>
public sealed record AccountResponse(
    string Id,
    string AccountNumber,
    string DisplayName,
    string Alias,
    decimal AvailableBalance,
    decimal CurrentBalance,
    string PrimaryType,
    string SubType,
    int AccountTypeFlags,
    bool IsActive)
{
    public static AccountResponse FromDto(AccountDto dto) =>
        new(
            Id: dto.Id,
            AccountNumber: dto.AccountNumber,
            DisplayName: dto.DisplayName,
            Alias: dto.Alias,
            AvailableBalance: dto.AvailableBalance,
            CurrentBalance: dto.CurrentBalance,
            PrimaryType: dto.PrimaryType.ToString(),
            SubType: dto.SubType.ToString(),
            AccountTypeFlags: (int)dto.AccountTypeFlags,
            IsActive: dto.IsActive);
}
