
using BankCTS.Api.Data;
using BankCTS.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankCTS.Api.Application.Services;

public class PresentmentService
{
    private readonly BankCtsDbContext _db;

    public PresentmentService(BankCtsDbContext db)
    {
        _db = db;
    }

    public async Task<ChequePresentment> CreateAsync(
        Guid instrumentId,
        string presentingBankCode,
        string receivedBy,
        CancellationToken cancellationToken = default)
    {
        // 1. Reject an empty instrument ID before accessing the database.
        if (instrumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid cheque instrument ID is required.",
                nameof(instrumentId));
        }

        // 2. Construct the presentment.
        // The domain constructor validates the bank code and receiving operator.
        // It also creates the initial audit event in memory.
        var presentment = new ChequePresentment(
            instrumentId,
            presentingBankCode,
            DateTimeOffset.UtcNow,
            receivedBy);

        // 3. Confirm that the physical cheque exists.
        bool instrumentExists = await _db.ChequeInstruments
            .AnyAsync(
                instrument => instrument.Id == instrumentId,
                cancellationToken);

        if (!instrumentExists)
        {
            throw new KeyNotFoundException(
                "The specified cheque instrument was not found.");
        }

        // 4. Add the presentment and its related audit event.
        _db.ChequePresentments.Add(presentment);

        // 5. Save both records together.
        await _db.SaveChangesAsync(cancellationToken);

        return presentment;
    }
}
