
namespace BankCTS.Api.Domain;

public class ChequePresentment
{
    private readonly List<PresentmentAuditEvent> _auditEvents = new();

    public IReadOnlyCollection<PresentmentAuditEvent> AuditEvents
        => _auditEvents.AsReadOnly();

    public Guid Id { get; private set; }

    public Guid InstrumentId { get; private set; }

    public ChequeInstrument Instrument { get; private set; } = null!;

    public string PresentingBankCode { get; private set; } = string.Empty;

    public PresentmentStatus Status { get; private set; }

    public Guid? PreviousPresentmentId { get; private set; }

    public ChequePresentment? PreviousPresentment { get; private set; }

    public DateTimeOffset ReceivedAt { get; private set; }

    private ChequePresentment()
    {
    }

    public ChequePresentment(
        Guid instrumentId,
        string presentingBankCode,
        DateTimeOffset receivedAt,
        string receivedBy,
        Guid? previousPresentmentId = null)
    {
        if (instrumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Instrument ID is required.",
                nameof(instrumentId));
        }

        if (string.IsNullOrWhiteSpace(presentingBankCode))
        {
            throw new ArgumentException(
                "Presenting bank code is required.",
                nameof(presentingBankCode));
        }

        if (presentingBankCode.Trim().Length > 50)
        {
            throw new ArgumentException(
                "Presenting bank code cannot exceed 50 characters.",
                nameof(presentingBankCode));
        }

        if (receivedAt == default)
        {
            throw new ArgumentException(
                "Received date and time are required.",
                nameof(receivedAt));
        }

        if (string.IsNullOrWhiteSpace(receivedBy))
        {
            throw new ArgumentException(
                "Receiving actor is required.",
                nameof(receivedBy));
        }

        if (receivedBy.Trim().Length > 100)
        {
            throw new ArgumentException(
                "Receiving actor cannot exceed 100 characters.",
                nameof(receivedBy));
        }

        if (previousPresentmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Previous presentment ID cannot be empty.",
                nameof(previousPresentmentId));
        }

        Id = Guid.NewGuid();
        InstrumentId = instrumentId;
        PresentingBankCode = presentingBankCode.Trim();
        ReceivedAt = receivedAt;
        PreviousPresentmentId = previousPresentmentId;
        Status = PresentmentStatus.Received;

        _auditEvents.Add(new PresentmentAuditEvent(
            Id,
            null,
            PresentmentStatus.Received,
            receivedAt,
            receivedBy));
    }

   public void TransitionTo(
    PresentmentStatus newStatus,
    string performedBy,
    string? reason = null)
{
    bool isValidTransition = Status switch
    {
        PresentmentStatus.Received =>
            newStatus == PresentmentStatus.Validating ||
            newStatus == PresentmentStatus.Rejected ||
            newStatus == PresentmentStatus.Cancelled ||
            newStatus == PresentmentStatus.Failed,

        PresentmentStatus.Validating =>
            newStatus == PresentmentStatus.Pending ||
            newStatus == PresentmentStatus.Rejected ||
            newStatus == PresentmentStatus.Cancelled ||
            newStatus == PresentmentStatus.Failed,

        PresentmentStatus.Pending =>
            newStatus == PresentmentStatus.Submitted ||
            newStatus == PresentmentStatus.Rejected ||
            newStatus == PresentmentStatus.Cancelled ||
            newStatus == PresentmentStatus.Failed,

        PresentmentStatus.Submitted =>
            newStatus == PresentmentStatus.Cleared ||
            newStatus == PresentmentStatus.Returned ||
            newStatus == PresentmentStatus.Failed,

        _ => false
    };

    if (!isValidTransition)
    {
        throw new InvalidOperationException(
            $"Cannot transition presentment from {Status} to {newStatus}.");
    }

    // Capture the current status before changing it.
    PresentmentStatus previousStatus = Status;

    // Create the audit event before changing the presentment.
    // If the actor or reason is invalid, the status remains unchanged.
    var auditEvent = new PresentmentAuditEvent(
        Id,
        previousStatus,
        newStatus,
        DateTimeOffset.UtcNow,
        performedBy,
        reason);

    // Apply the transition and record its audit event.
    Status = newStatus;
    _auditEvents.Add(auditEvent);
}
}
