
namespace BankCTS.Api.Domain;

public enum PresentmentStatus
{
    Received,
    Validating,
    Pending,
    Submitted,
    Cleared,
    Returned,
    Rejected,
    Cancelled,
    Failed
}
