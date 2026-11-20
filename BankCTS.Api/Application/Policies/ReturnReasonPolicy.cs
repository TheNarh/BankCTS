
using BankCTS.Api.Configuration;
using Microsoft.Extensions.Options;

namespace BankCTS.Api.Application.Policies;

public class ReturnReasonPolicy
{
    private readonly ClearingPolicyOptions _options;

    public ReturnReasonPolicy(
        IOptions<ClearingPolicyOptions> options)
    {
        _options = options.Value;
    }

    public bool IsEligible(string? returnReasonCode)
    {
        // No return reason means re-presentment is not eligible.
        if (string.IsNullOrWhiteSpace(returnReasonCode))
        {
            return false;
        }

        // Missing configuration must fail closed.
        if (_options.EligibleReturnReasonCodes is null ||
            _options.EligibleReturnReasonCodes.Count == 0)
        {
            return false;
        }

        string normalizedCode = returnReasonCode.Trim();

        return _options.EligibleReturnReasonCodes.Any(
            configuredCode =>
                !string.IsNullOrWhiteSpace(configuredCode) &&
                string.Equals(
                    configuredCode.Trim(),
                    normalizedCode,
                    StringComparison.OrdinalIgnoreCase));
    }
}
