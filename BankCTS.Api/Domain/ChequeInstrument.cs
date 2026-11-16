
namespace BankCTS.Api.Domain;

public class ChequeInstrument
{
    public Guid Id { get; private set; }

    public string IssuingBankCode { get; private set; } = string.Empty;
    public string? IssuingBranchCode { get; private set; }

    public string AccountNumber { get; private set; } = string.Empty;
    public string ChequeNumber { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "GHS";

    public DateOnly ChequeDate { get; private set; }

    public string DrawerName { get; private set; } = string.Empty;
    public string PayeeName { get; private set; } = string.Empty;

    public ICollection<ChequePresentment> Presentments { get; private set; }
        = new List<ChequePresentment>();

    private ChequeInstrument()
    {
    }

    public ChequeInstrument(
        string issuingBankCode,
        string? issuingBranchCode,
        string accountNumber,
        string chequeNumber,
        decimal amount,
        string currency,
        DateOnly chequeDate,
        string drawerName,
        string payeeName)
    {
        if (amount <= 0)
        {
            throw new ArgumentException(
                "Cheque amount must be greater than zero.",
                nameof(amount));
        }

        if (chequeDate == default)
        {
            throw new ArgumentException(
                "Cheque date is required.",
                nameof(chequeDate));
        }

        Id = Guid.NewGuid();

        IssuingBankCode = ValidateRequired(
            issuingBankCode, 50, nameof(issuingBankCode));

        IssuingBranchCode = string.IsNullOrWhiteSpace(issuingBranchCode)
            ? null
            : ValidateRequired(
                issuingBranchCode, 50, nameof(issuingBranchCode));

        AccountNumber = ValidateRequired(
            accountNumber, 50, nameof(accountNumber));

        ChequeNumber = ValidateRequired(
            chequeNumber, 50, nameof(chequeNumber));

        DrawerName = ValidateRequired(
            drawerName, 150, nameof(drawerName));

        PayeeName = ValidateRequired(
            payeeName, 150, nameof(payeeName));

        Currency = ValidateCurrency(currency);

        Amount = amount;
        ChequeDate = chequeDate;
    }

    private static string ValidateRequired(
        string value,
        int maxLength,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "This field is required.",
                parameterName);
        }

        string normalized = value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"This field cannot exceed {maxLength} characters.",
                parameterName);
        }

        return normalized;
    }

    private static string ValidateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException(
                "Currency is required.",
                nameof(currency));
        }

        string normalized = currency.Trim().ToUpperInvariant();

        if (normalized.Length != 3 ||
            normalized.Any(character => character < 'A' || character > 'Z'))
        {
            throw new ArgumentException(
                "Currency must be a three-letter currency code.",
                nameof(currency));
        }

        return normalized;
    }
}
