
using BankCTS.Api.Domain;

namespace BankCTS.Tests;

public class ChequePresentmentTests
{
    private static ChequePresentment CreatePresentment()
    {
        return new ChequePresentment(
            Guid.NewGuid(),
            "BANK001",
            DateTimeOffset.UtcNow);
    }

    private static ChequePresentment CreateSubmittedPresentment()
    {
        var presentment = CreatePresentment();

        presentment.TransitionTo(PresentmentStatus.Validating);
        presentment.TransitionTo(PresentmentStatus.Pending);
        presentment.TransitionTo(PresentmentStatus.Submitted);

        return presentment;
    }

    [Fact]
    public void New_presentment_starts_with_received_status()
    {
        var presentment = CreatePresentment();

        Assert.Equal(PresentmentStatus.Received, presentment.Status);
    }

    [Fact]
    public void Received_presentment_can_move_to_validating()
    {
        var presentment = CreatePresentment();

        presentment.TransitionTo(PresentmentStatus.Validating);

        Assert.Equal(PresentmentStatus.Validating, presentment.Status);
    }

    [Fact]
    public void Submitted_presentment_can_be_cleared()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.TransitionTo(PresentmentStatus.Cleared);

        Assert.Equal(PresentmentStatus.Cleared, presentment.Status);
    }

    [Fact]
    public void Submitted_presentment_can_be_returned()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.TransitionTo(PresentmentStatus.Returned);

        Assert.Equal(PresentmentStatus.Returned, presentment.Status);
    }

    [Fact]
    public void Received_presentment_cannot_be_cleared_directly()
    {
        var presentment = CreatePresentment();

        Assert.Throws<InvalidOperationException>(() =>
            presentment.TransitionTo(PresentmentStatus.Cleared));
    }

    [Fact]
    public void Returned_presentment_cannot_be_submitted_again()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.TransitionTo(PresentmentStatus.Returned);

        Assert.Throws<InvalidOperationException>(() =>
            presentment.TransitionTo(PresentmentStatus.Submitted));
    }

    [Fact]
    public void Cleared_presentment_cannot_be_returned()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.TransitionTo(PresentmentStatus.Cleared);

        Assert.Throws<InvalidOperationException>(() =>
            presentment.TransitionTo(PresentmentStatus.Returned));
    }
}
