using FinCore.Core.Enums;

namespace FinCore.Application.Accounts.DTOs;

/// <summary>
/// Application-layer data transfer object for an account.
/// Decoupled from both the domain entity and the API response model.
/// </summary>
public sealed record AccountDto(
    string Id,
    string AccountNumber,
    string DisplayName,
    string Alias,
    decimal AvailableBalance,
    decimal CurrentBalance,
    PrimaryAccountType PrimaryType,
    SubAccountType SubType,
    AccountTypes AccountTypeFlags,
    bool IsActive);
