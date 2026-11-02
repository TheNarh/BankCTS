namespace BankCTS.Api.Domain;

public class Cheque
{
    public Guid Id { get; set; }

    public string ChequeNumber { get; private set; } = string.Empty;

    public string AccountNumber { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public void SetAmount(decimal amount)
{
    if (amount <= 0)
    {
        throw new ArgumentException(
            "Cheque amount must be greater than zero.",
            nameof(amount));
    }

    Amount = amount;
}

    public string Currency { get; private set; } = "GHS";

    public ChequeStatus Status { get; private set; }

    
public void TransitionTo(ChequeStatus newStatus)
{
    if (newStatus == ChequeStatus.Validated)
    {
        if (string.IsNullOrWhiteSpace(ChequeNumber))
        {
            throw new InvalidOperationException(
                "A cheque must have a cheque number before validation.");
        }

        if (string.IsNullOrWhiteSpace(AccountNumber))
        {
            throw new InvalidOperationException(
                "A cheque must have an account number before validation.");
        }

        ValidateDates();
    }

    bool isValidTransition = Status switch
    {
        ChequeStatus.Captured => newStatus == ChequeStatus.Validated,
        ChequeStatus.Validated => newStatus == ChequeStatus.Accepted,
        ChequeStatus.Accepted => newStatus == ChequeStatus.Submitted,
        ChequeStatus.Submitted => newStatus == ChequeStatus.Cleared,
        ChequeStatus.Cleared => newStatus == ChequeStatus.Settled,
        _ => false
    };

    if (!isValidTransition)
    {
        throw new InvalidOperationException(
            $"Cannot transition cheque from {Status} to {newStatus}.");
    }

    Status = newStatus;
}


    public Cheque()
    {
        Status = ChequeStatus.Captured;
    }

    public DateOnly ChequeDate { get; set; }

    public DateOnly PresentationDate { get; set; }
    public string DrawerName { get; set; } = string.Empty;
    public string PayeeName { get; set; } = string.Empty;

   public void SetChequeNumber(string chequeNumber)
{
    if (string.IsNullOrWhiteSpace(chequeNumber))
    {
        throw new ArgumentException(
            "Cheque number cannot be empty.",
            nameof(chequeNumber));
    }

    ChequeNumber = chequeNumber.Trim();
}

    public void SetAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
        {
            throw new ArgumentException(
                "Account number cannot be empty.",
                nameof(accountNumber));
        }

        AccountNumber = accountNumber.Trim();
    }

    public void ValidateDates()
    {
        if (PresentationDate < ChequeDate)
        {
            throw new ArgumentException(
                "Presentation date cannot be earlier than cheque date.");
        }
    }
}