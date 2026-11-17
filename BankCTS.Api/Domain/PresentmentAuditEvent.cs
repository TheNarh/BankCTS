
namespace BankCTS.Api.Domain;

public class PresentmentAuditEvent
{
    public Guid Id { get; private set; }

    public Guid PresentmentId { get; private set; }

    public PresentmentStatus? PreviousStatus { get; private set; }

    public PresentmentStatus NewStatus { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public string PerformedBy { get; private set; } = string.Empty;

    public string? Reason { get; private set; }

    private PresentmentAuditEvent()
    {
    }

    public PresentmentAuditEvent(
        Guid presentmentId,
        PresentmentStatus? previousStatus,
        PresentmentStatus newStatus,
        DateTimeOffset occurredAt,
        string performedBy,
        string? reason = null)
    {
        if (presentmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Presentment ID is required.",
                nameof(presentmentId));
        }

        if (previousStatus == newStatus)
        {
            throw new ArgumentException(
                "Previous and new statuses cannot be the same.",
                nameof(newStatus));
        }

        if (occurredAt == default)
        {
            throw new ArgumentException(
                "Event timestamp is required.",
                nameof(occurredAt));
        }

        if (string.IsNullOrWhiteSpace(performedBy))
        {
            throw new ArgumentException(
                "The actor responsible for the event is required.",
                nameof(performedBy));
        }

        if (performedBy.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Performed by cannot exceed 100 characters.",
                nameof(performedBy));
        }

        if (reason?.Length > 500)
        {
            throw new ArgumentException(
                "Reason cannot exceed 500 characters.",
                nameof(reason));
        }

        Id = Guid.NewGuid();
        PresentmentId = presentmentId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        OccurredAt = occurredAt;
        PerformedBy = performedBy.Trim();
        Reason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();
    }
}
