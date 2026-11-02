namespace BankCTS.Api.Contracts;

public record CreateChequeRequest(
    string ChequeNumber,
    string AccountNumber,
    decimal Amount,
    DateOnly ChequeDate,
    DateOnly PresentationDate,
    string DrawerName,
    string PayeeName);