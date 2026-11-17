
using BankCTS.Api.Domain;

namespace BankCTS.Tests;

public class PresentmentAuditEventTests
{
    private static PresentmentAuditEvent CreateAuditEvent()
    {
        return new PresentmentAuditEvent(
            Guid.NewGuid(),
            PresentmentStatus.Received,
            PresentmentStatus.Validating,
            DateTimeOffset.UtcNow,
            "CTS-System",
            "Validation started");
    }

    [Fact]
    public void Audit_event_can_be_created_with_valid_details()
    {
        var auditEvent = CreateAuditEvent();

        Assert.NotEqual(Guid.Empty, auditEvent.Id);
        Assert.NotEqual(Guid.Empty, auditEvent.PresentmentId);
        Assert.Equal(PresentmentStatus.Received, auditEvent.PreviousStatus);
        Assert.Equal(PresentmentStatus.Validating, auditEvent.NewStatus);
        Assert.Equal("CTS-System", auditEvent.PerformedBy);
        Assert.Equal("Validation started", auditEvent.Reason);
    }

    [Fact]
    public void Initial_audit_event_can_have_no_previous_status()
    {
        var auditEvent = new PresentmentAuditEvent(
            Guid.NewGuid(),
            null,
            PresentmentStatus.Received,
            DateTimeOffset.UtcNow,
            "CTS-System");

        Assert.Null(auditEvent.PreviousStatus);
        Assert.Equal(PresentmentStatus.Received, auditEvent.NewStatus);
    }

    [Fact]
    public void Audit_event_requires_a_presentment_id()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentmentAuditEvent(
                Guid.Empty,
                PresentmentStatus.Received,
                PresentmentStatus.Validating,
                DateTimeOffset.UtcNow,
                "CTS-System"));
    }

    [Fact]
    public void Audit_event_rejects_identical_previous_and_new_statuses()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentmentAuditEvent(
                Guid.NewGuid(),
                PresentmentStatus.Received,
                PresentmentStatus.Received,
                DateTimeOffset.UtcNow,
                "CTS-System"));
    }

    [Fact]
    public void Audit_event_requires_a_timestamp()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentmentAuditEvent(
                Guid.NewGuid(),
                PresentmentStatus.Received,
                PresentmentStatus.Validating,
                default,
                "CTS-System"));
    }

    [Fact]
    public void Audit_event_requires_an_actor()
    {
        Assert.Throws<ArgumentException>(() =>
            new PresentmentAuditEvent(
                Guid.NewGuid(),
                PresentmentStatus.Received,
                PresentmentStatus.Validating,
                DateTimeOffset.UtcNow,
                " "));
    }

    [Fact]
    public void Audit_event_trims_actor_name()
    {
        var auditEvent = new PresentmentAuditEvent(
            Guid.NewGuid(),
            PresentmentStatus.Received,
            PresentmentStatus.Validating,
            DateTimeOffset.UtcNow,
            "  CTS-System  ");

        Assert.Equal("CTS-System", auditEvent.PerformedBy);
    }

    [Fact]
    public void Audit_event_rejects_reason_longer_than_500_characters()
    {
        var longReason = new string('A', 501);

        Assert.Throws<ArgumentException>(() =>
            new PresentmentAuditEvent(
                Guid.NewGuid(),
                PresentmentStatus.Received,
                PresentmentStatus.Validating,
                DateTimeOffset.UtcNow,
                "CTS-System",
                longReason));
    }
}
