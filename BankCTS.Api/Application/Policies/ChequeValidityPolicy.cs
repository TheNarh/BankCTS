
using BankCTS.Api.Configuration;
using Microsoft.Extensions.Options;

namespace BankCTS.Api.Application.Policies;

public class ChequeValidityPolicy
{
    private readonly ClearingPolicyOptions _options;

    public ChequeValidityPolicy(
        IOptions<ClearingPolicyOptions> options)
    {
        _options = options.Value;
    }

    public bool IsValid(
        DateOnly chequeDate,
        DateOnly assessmentDate)
    {
        int? maximumAgeInDays =
            _options.ChequeValidity.MaximumAgeInDays;

        if (maximumAgeInDays is null || maximumAgeInDays <= 0)
        {
            throw new InvalidOperationException(
                "Cheque validity policy has not been configured correctly.");
        }

        if (chequeDate == default)
        {
            throw new ArgumentException(
                "Cheque date is required.",
                nameof(chequeDate));
        }

        if (assessmentDate == default)
        {
            throw new ArgumentException(
                "Assessment date is required.",
                nameof(assessmentDate));
        }

        if (chequeDate > assessmentDate)
        {
            return false;
        }

        int chequeAgeInDays =
            assessmentDate.DayNumber - chequeDate.DayNumber;

        return chequeAgeInDays <= maximumAgeInDays.Value;
    }
}
