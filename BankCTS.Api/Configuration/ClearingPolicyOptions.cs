
namespace BankCTS.Api.Configuration;

public class ClearingPolicyOptions
{
    public const string SectionName = "ClearingPolicy";

    public ChequeValidityOptions ChequeValidity { get; set; } = new();

    public List<string> EligibleReturnReasonCodes { get; set; } = [];
}

public class ChequeValidityOptions
{
    public int? MaximumAgeInDays { get; set; }
}
