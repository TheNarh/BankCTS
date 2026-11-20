
using BankCTS.Api.Application.Policies;
using BankCTS.Api.Configuration;
using Microsoft.Extensions.Options;

namespace BankCTS.Tests;

public class ChequeValidityPolicyTests
{
    private static ChequeValidityPolicy CreatePolicy(int? maximumAgeInDays)
    {
        var options = Options.Create(new ClearingPolicyOptions
        {
            ChequeValidity = new ChequeValidityOptions
            {
                MaximumAgeInDays = maximumAgeInDays
            }
        });

        return new ChequeValidityPolicy(options);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenChequeIsWithinValidityPeriod()
    {
        // Arrange
        var policy = CreatePolicy(180);

        var chequeDate = new DateOnly(2026, 1, 1);
        var assessmentDate = new DateOnly(2026, 3, 1);

        // Act
        bool result = policy.IsValid(chequeDate, assessmentDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenChequeIsExactlyAtValidityLimit()
    {
        // Arrange
        var policy = CreatePolicy(180);

        var chequeDate = new DateOnly(2026, 1, 1);
        var assessmentDate = chequeDate.AddDays(180);

        // Act
        bool result = policy.IsValid(chequeDate, assessmentDate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenChequeExceedsValidityPeriod()
    {
        // Arrange
        var policy = CreatePolicy(180);

        var chequeDate = new DateOnly(2026, 1, 1);
        var assessmentDate = chequeDate.AddDays(181);

        // Act
        bool result = policy.IsValid(chequeDate, assessmentDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenChequeDateIsInTheFuture()
    {
        // Arrange
        var policy = CreatePolicy(180);

        var assessmentDate = new DateOnly(2026, 11, 20);
        var chequeDate = assessmentDate.AddDays(1);

        // Act
        bool result = policy.IsValid(chequeDate, assessmentDate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenChequeDateEqualsAssessmentDate()
    {
        // Arrange
        var policy = CreatePolicy(180);

        var date = new DateOnly(2026, 11, 20);

        // Act
        bool result = policy.IsValid(date, date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_ThrowsInvalidOperationException_WhenValidityIsNotConfigured()
    {
        // Arrange
        var policy = CreatePolicy(null);

        var chequeDate = new DateOnly(2026, 1, 1);
        var assessmentDate = new DateOnly(2026, 3, 1);

        // Act and Assert
        Assert.Throws<InvalidOperationException>(
            () => policy.IsValid(chequeDate, assessmentDate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-180)]
    public void IsValid_ThrowsInvalidOperationException_WhenValidityPeriodIsInvalid(
        int maximumAgeInDays)
    {
        // Arrange
        var policy = CreatePolicy(maximumAgeInDays);

        var chequeDate = new DateOnly(2026, 1, 1);
        var assessmentDate = new DateOnly(2026, 3, 1);

        // Act and Assert
        Assert.Throws<InvalidOperationException>(
            () => policy.IsValid(chequeDate, assessmentDate));
    }

    [Fact]
    public void IsValid_ThrowsArgumentException_WhenChequeDateIsDefault()
    {
        // Arrange
        var policy = CreatePolicy(180);

        // Act and Assert
        var exception = Assert.Throws<ArgumentException>(
            () => policy.IsValid(
                default,
                new DateOnly(2026, 11, 20)));

        Assert.Equal("chequeDate", exception.ParamName);
    }

    [Fact]
    public void IsValid_ThrowsArgumentException_WhenAssessmentDateIsDefault()
    {
        // Arrange
        var policy = CreatePolicy(180);

        // Act and Assert
        var exception = Assert.Throws<ArgumentException>(
            () => policy.IsValid(
                new DateOnly(2026, 1, 1),
                default));

        Assert.Equal("assessmentDate", exception.ParamName);
    }
}
