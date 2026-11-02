using BankCTS.Api.Domain;

namespace BankCTS.Tests;

public class ChequeTests
{
    [Fact]
    public void Captured_cheque_can_be_validated()
    {
        var cheque = new Cheque();

        cheque.SetChequeNumber("CHQ001");
        cheque.SetAccountNumber("ACC12345");
        cheque.ChequeDate = new DateOnly(2026, 10, 15);
        cheque.PresentationDate = new DateOnly(2026, 10, 16);

        cheque.TransitionTo(ChequeStatus.Validated);

        Assert.Equal(ChequeStatus.Validated, cheque.Status);
    }

    [Fact]
    public void Captured_cheque_cannot_be_settled_directly()
    {
        // Arrange
        var cheque = new Cheque();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            cheque.TransitionTo(ChequeStatus.Settled));
    }

    [Fact]
    public void Cheque_amount_must_be_positive()
    {
        // Arrange
        var cheque = new Cheque();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            cheque.SetAmount(0));
    }

    [Fact]
    public void Cheque_amount_cannot_be_negative()
    {
        // Arrange
        var cheque = new Cheque();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            cheque.SetAmount(-100));
    }

    [Fact]
    public void Cheque_number_cannot_be_empty()
    {
        // Arrange
        var cheque = new Cheque();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            cheque.SetChequeNumber(" "));
    }

    [Fact]
    public void Account_number_cannot_be_empty()
    {
        // Arrange
        var cheque = new Cheque();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            cheque.SetAccountNumber(" "));
    }

    [Fact]
    public void Cheque_can_be_created_with_valid_details()
    {
        // Arrange
        var cheque = new Cheque();

        // Act
        cheque.SetChequeNumber("CHQ001");
        cheque.SetAccountNumber("0123456789");
        cheque.SetAmount(250.00m);

        // Assert
        Assert.Equal("CHQ001", cheque.ChequeNumber);
        Assert.Equal("0123456789", cheque.AccountNumber);
        Assert.Equal(250.00m, cheque.Amount);
        Assert.Equal("GHS", cheque.Currency);
        Assert.Equal(ChequeStatus.Captured, cheque.Status);
    }

    [Fact]
    public void Presentation_date_cannot_be_before_cheque_date()
    {
        // Arrange
        var cheque = new Cheque
        {
            ChequeDate = new DateOnly(2026, 10, 15),
            PresentationDate = new DateOnly(2026, 10, 14)
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            cheque.ValidateDates());
    }

    [Fact]
    public void Cheque_with_invalid_dates_cannot_be_validated()
    {
        var cheque = new Cheque();

        cheque.SetChequeNumber("CHQ002");
        cheque.SetAccountNumber("ACC12345");
        cheque.ChequeDate = new DateOnly(2026, 10, 15);
        cheque.PresentationDate = new DateOnly(2026, 10, 14);

        Assert.Throws<ArgumentException>(
            () => cheque.TransitionTo(ChequeStatus.Validated));
    }

    [Fact]
    public void Cheque_with_valid_dates_can_be_validated()
    {
        var cheque = new Cheque();

        cheque.SetChequeNumber("CHQ003");
        cheque.SetAccountNumber("ACC12345");
        cheque.ChequeDate = new DateOnly(2026, 10, 15);
        cheque.PresentationDate = new DateOnly(2026, 10, 16);

        cheque.TransitionTo(ChequeStatus.Validated);

        Assert.Equal(ChequeStatus.Validated, cheque.Status);
    }

    [Fact]
    public void Cheque_number_is_trimmed_when_set()
    {
        // Arrange
        var cheque = new Cheque();

        // Act
        cheque.SetChequeNumber("  CHQ001  ");

        // Assert
        Assert.Equal("CHQ001", cheque.ChequeNumber);
    }

    [Fact]
    public void Account_number_is_trimmed_when_set()
    {
        // Arrange
        var cheque = new Cheque();

        // Act
        cheque.SetAccountNumber("  ACC12345  ");

        // Assert
        Assert.Equal("ACC12345", cheque.AccountNumber);
    }

    [Fact]
    public void Cheque_without_cheque_number_cannot_be_validated()
    {
        var cheque = new Cheque
        {
            ChequeDate = new DateOnly(2026, 10, 15),
            PresentationDate = new DateOnly(2026, 10, 16)
        };

        Assert.Throws<InvalidOperationException>(
            () => cheque.TransitionTo(ChequeStatus.Validated));
    }

    [Fact]
    public void Cheque_without_account_number_cannot_be_validated()
    {
        var cheque = new Cheque();
        cheque.SetChequeNumber("CHQ001");
        cheque.SetAmount(100m);
        cheque.ChequeDate = new DateOnly(2026, 10, 15);
        cheque.PresentationDate = new DateOnly(2026, 10, 16);

        Assert.Throws<InvalidOperationException>(
            () => cheque.TransitionTo(ChequeStatus.Validated));
    }
}