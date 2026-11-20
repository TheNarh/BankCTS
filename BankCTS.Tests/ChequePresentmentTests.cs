
using BankCTS.Api.Domain;

namespace BankCTS.Tests;

public class ChequePresentmentTests
{
    private static ChequePresentment CreatePresentment()
    {
        return new ChequePresentment(
            Guid.NewGuid(),
            "BANK001",
            DateTimeOffset.UtcNow,
            "CTS-System");
    }

    private static ChequePresentment CreateSubmittedPresentment()
    {
        var presentment = CreatePresentment();

        presentment.TransitionTo(
            PresentmentStatus.Validating,
            "CTS-System",
            "Validation started");

        presentment.TransitionTo(
            PresentmentStatus.Pending,
            "CTS-System",
            "Validation completed");

        presentment.TransitionTo(
            PresentmentStatus.Submitted,
            "CTS-System",
            "Submitted for clearing");

        return presentment;
    }

    [Fact]
    public void New_presentment_starts_with_received_status()
    {
        var presentment = CreatePresentment();

        Assert.Equal(PresentmentStatus.Received, presentment.Status);
    }

    [Fact]
    public void New_presentment_records_initial_audit_event()
    {
        var presentment = CreatePresentment();

        var auditEvent = Assert.Single(presentment.AuditEvents);

        Assert.Null(auditEvent.PreviousStatus);
        Assert.Equal(PresentmentStatus.Received, auditEvent.NewStatus);
        Assert.Equal("CTS-System", auditEvent.PerformedBy);
    }

    [Fact]
    public void Received_presentment_can_move_to_validating()
    {
        var presentment = CreatePresentment();

        presentment.TransitionTo(
            PresentmentStatus.Validating,
            "CTS-System",
            "Validation started");

        Assert.Equal(PresentmentStatus.Validating, presentment.Status);
    }

    [Fact]
    public void Successful_transition_creates_audit_event()
    {
        var presentment = CreatePresentment();

        presentment.TransitionTo(
            PresentmentStatus.Validating,
            "CTS-System",
            "Validation started");

        Assert.Equal(2, presentment.AuditEvents.Count);

        var auditEvent = presentment.AuditEvents.Last();

        Assert.Equal(PresentmentStatus.Received, auditEvent.PreviousStatus);
        Assert.Equal(PresentmentStatus.Validating, auditEvent.NewStatus);
        Assert.Equal("CTS-System", auditEvent.PerformedBy);
        Assert.Equal("Validation started", auditEvent.Reason);
    }

    [Fact]
    public void Submitted_presentment_can_be_cleared()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.TransitionTo(
            PresentmentStatus.Cleared,
            "CTS-System",
            "Cheque cleared");

        Assert.Equal(PresentmentStatus.Cleared, presentment.Status);
    }

    [Fact]
    public void Submitted_presentment_can_be_returned()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.Return(
            "INSUFFICIENT_FUNDS",
            "operator",
            "Insufficient funds");

        Assert.Equal(PresentmentStatus.Returned, presentment.Status);
    }

    [Fact]
    public void Received_presentment_cannot_be_cleared_directly()
    {
        var presentment = CreatePresentment();

        Assert.Throws<InvalidOperationException>(() =>
            presentment.TransitionTo(
                PresentmentStatus.Cleared,
                "CTS-System"));

        Assert.Single(presentment.AuditEvents);
        Assert.Equal(PresentmentStatus.Received, presentment.Status);
    }

    [Fact]
    public void Returned_presentment_cannot_be_submitted_again()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.Return(
            "INSUFFICIENT_FUNDS",
            "operator",
            "Insufficient funds");

        Assert.Throws<InvalidOperationException>(() =>
            presentment.TransitionTo(
                PresentmentStatus.Submitted,
                "CTS-System"));
    }

    [Fact]
    public void Cleared_presentment_cannot_be_returned()
    {
        var presentment = CreateSubmittedPresentment();

        presentment.TransitionTo(
            PresentmentStatus.Cleared,
            "CTS-System");

        Assert.Throws<InvalidOperationException>(() =>
            presentment.TransitionTo(
                PresentmentStatus.Returned,
                "PayingBank001"));
    }
}
