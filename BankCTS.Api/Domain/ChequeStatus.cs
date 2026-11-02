namespace BankCTS.Api.Domain;

public enum ChequeStatus
{
    Captured,
    Validated,
    Accepted,
    Submitted,
    Cleared,
    Settled,
    Rejected,
    Failed
}