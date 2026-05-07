namespace FinCore.DB.Base;

/// <summary>
/// Provides dynamic SQL field and table names for DB2 queries.
/// Allows vendor-specific schema variations without code changes.
/// Stub implementation uses sensible defaults; override per-FI if needed.
/// </summary>
public interface IVendorFieldConfiguration
{
    // Table
    string AccountTableName { get; }

    // Account field names
    string AccountIdField { get; }
    string AccountNumberField { get; }
    string DisplayNameField { get; }
    string AvailableBalanceField { get; }
    string CurrentBalanceField { get; }
    string PrimaryAccountTypeField { get; }
    string SubAccountTypeField { get; }
    string AccountTypeFlagField { get; }
    string IsActiveField { get; }
    string EntityField { get; }
    string TaxIdField { get; }

    // Type codes stored in DB2
    string DepositTypeCode { get; }
    string LoanTypeCode { get; }

    // Alias table
    string AliasTableName { get; }
    string AliasEntityField { get; }
    string AliasAccessIdField { get; }
    string AliasAccountNumberField { get; }
    string AliasNameField { get; }
}
