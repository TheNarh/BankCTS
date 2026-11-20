
using BankCTS.Api.Application.Policies;
using BankCTS.Api.Configuration;
using Microsoft.Extensions.Options;

namespace BankCTS.Tests;

public class ReturnReasonPolicyTests
{
    private static ReturnReasonPolicy CreatePolicy(
        List<string>? eligibleCodes)
    {
        var options = Options.Create(new ClearingPolicyOptions
        {
            EligibleReturnReasonCodes = eligibleCodes!
        });

        return new ReturnReasonPolicy(options);
    }

    [Fact]
    public void IsEligible_ReturnsTrue_WhenCodeIsConfigured()
    {
        var policy = CreatePolicy(["R01", "R03"]);

        bool result = policy.IsEligible("R01");

        Assert.True(result);
    }

    [Fact]
    public void IsEligible_ReturnsFalse_WhenCodeIsNotConfigured()
    {
        var policy = CreatePolicy(["R01", "R03"]);

        bool result = policy.IsEligible("R99");

        Assert.False(result);
    }

    [Fact]
    public void IsEligible_ReturnsFalse_WhenNoCodesAreConfigured()
    {
        var policy = CreatePolicy([]);

        bool result = policy.IsEligible("R01");

        Assert.False(result);
    }

    [Fact]
    public void IsEligible_ReturnsFalse_WhenCodeIsNull()
    {
        var policy = CreatePolicy(["R01"]);

        bool result = policy.IsEligible(null);

        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsEligible_ReturnsFalse_WhenCodeIsBlank(
        string returnReasonCode)
    {
        var policy = CreatePolicy(["R01"]);

        bool result = policy.IsEligible(returnReasonCode);

        Assert.False(result);
    }

    [Fact]
    public void IsEligible_IgnoresLeadingAndTrailingWhitespace()
    {
        var policy = CreatePolicy(["R01"]);

        bool result = policy.IsEligible("  R01  ");

        Assert.True(result);
    }

    [Fact]
    public void IsEligible_IsCaseInsensitive()
    {
        var policy = CreatePolicy(["R01"]);

        bool result = policy.IsEligible("r01");

        Assert.True(result);
    }

    [Fact]
    public void IsEligible_IgnoresBlankConfiguredCodes()
    {
        var policy = CreatePolicy([" ", "R01", ""]);

        bool result = policy.IsEligible("R01");

        Assert.True(result);
    }
}
