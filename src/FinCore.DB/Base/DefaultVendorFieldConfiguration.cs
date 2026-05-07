namespace FinCore.DB.Base;

/// <summary>
/// Default stub implementation of <see cref="IVendorFieldConfiguration"/>.
/// Uses conventional DB2 field names. Override this per-FI via DI if the schema differs.
/// </summary>
public sealed class DefaultVendorFieldConfiguration : IVendorFieldConfiguration
{
    public string AccountTableName => "ACCT_MASTER";

    public string AccountIdField => "ACCT_ID";
    public string AccountNumberField => "ACCT_NBR";
    public string DisplayNameField => "ACCT_DISPLAY_NM";
    public string AvailableBalanceField => "AVAIL_BAL";
    public string CurrentBalanceField => "CURR_BAL";
    public string PrimaryAccountTypeField => "PRIM_ACCT_TYPE";
    public string SubAccountTypeField => "SUB_ACCT_TYPE";
    public string AccountTypeFlagField => "ACCT_TYPE_FLG";
    public string IsActiveField => "ACTIVE_FLG";
    public string EntityField => "ENTITY_ID";
    public string TaxIdField => "TAX_ID";

    public string DepositTypeCode => "D";
    public string LoanTypeCode => "L";

    public string AliasTableName => "ACCT_ALIAS";
    public string AliasEntityField => "ENTITY_ID";
    public string AliasAccessIdField => "ACCESS_ID";
    public string AliasAccountNumberField => "ACCT_NBR";
    public string AliasNameField => "ALIAS_NM";
}
