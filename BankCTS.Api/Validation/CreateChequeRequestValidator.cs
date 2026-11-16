
using BankCTS.Api.Contracts;

namespace BankCTS.Api.Validation;

public static class CreateChequeRequestValidator
{
    public static Dictionary<string, string[]> Validate(
        CreateChequeRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        void AddError(string property, string message)
        {
            if (!errors.ContainsKey(property))
            {
                errors[property] = new List<string>();
            }

            errors[property].Add(message);
        }

        // Cheque number
        if (string.IsNullOrWhiteSpace(request.ChequeNumber))
        {
            AddError("ChequeNumber", "Cheque number is required.");
        }
        else if (request.ChequeNumber.Trim().Length > 50)
        {
            AddError("ChequeNumber", "Cheque number cannot exceed 50 characters.");
        }

        // Account number
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            AddError("AccountNumber", "Account number is required.");
        }
        else if (request.AccountNumber.Trim().Length > 50)
        {
            AddError("AccountNumber", "Account number cannot exceed 50 characters.");
        }

        // Amount
        if (request.Amount <= 0)
        {
            AddError("Amount", "Cheque amount must be greater than zero.");
        }

        // Cheque date
        if (request.ChequeDate == default)
        {
            AddError("ChequeDate", "Cheque date is required.");
        }

        // Presentation date
        if (request.PresentationDate == default)
        {
            AddError("PresentationDate", "Presentation date is required.");
        }
        else if (request.ChequeDate != default &&
                 request.PresentationDate < request.ChequeDate)
        {
            AddError(
                "PresentationDate",
                "Presentation date cannot be earlier than cheque date.");
        }

        // Drawer name
        if (string.IsNullOrWhiteSpace(request.DrawerName))
        {
            AddError("DrawerName", "Drawer name is required.");
        }
        else if (request.DrawerName.Trim().Length > 150)
        {
            AddError("DrawerName", "Drawer name cannot exceed 150 characters.");
        }

        // Payee name
        if (string.IsNullOrWhiteSpace(request.PayeeName))
        {
            AddError("PayeeName", "Payee name is required.");
        }
        else if (request.PayeeName.Trim().Length > 150)
        {
            AddError("PayeeName", "Payee name cannot exceed 150 characters.");
        }

        return errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray());
    }
}
