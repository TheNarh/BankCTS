
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
        // 1. Confirm that the physical cheque exists.
        bool instrumentExists = await _db.ChequeInstruments
            .AnyAsync(
                instrument => instrument.Id == instrumentId,
                cancellationToken);

        if (!instrumentExists)
        {
            throw new KeyNotFoundException(
                "The specified cheque instrument was not found.");
        }

        // 2. Create the presentment.
        // The constructor automatically creates its initial audit event.
        var presentment = new ChequePresentment(
            instrumentId,
            presentingBankCode,
            DateTimeOffset.UtcNow,
            receivedBy);

        // 3. Add the presentment and its related audit event.
        _db.ChequePresentments.Add(presentment);

        // 4. Save both records in one database transaction.
        await _db.SaveChangesAsync(cancellationToken);

        return presentment;
    }
}
