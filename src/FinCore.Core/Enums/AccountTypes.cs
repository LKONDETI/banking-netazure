namespace FinCore.Core.Enums;

[Flags]
public enum AccountTypes
{
    None         = 0,
    Deposits     = 1,
    Loans        = 2,
    FromAccounts = 4,
    ToAccounts   = 8,
    BillPay      = 16,
    ACH          = 32
}
