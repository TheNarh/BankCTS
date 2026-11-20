
using BankCTS.Api.Application.Exceptions;
using BankCTS.Api.Data;
using BankCTS.Api.Domain;
using Microsoft.Data.Sqlite;
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
        // 1. Reject an empty instrument ID.
        if (instrumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid cheque instrument ID is required.",
                nameof(instrumentId));
        }

        // 2. Construct the presentment.
        // The domain constructor validates the supplied details
        // and creates the initial audit event.
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

        // 4. Check whether this cheque already has an active presentment.
        bool hasActivePresentment = await _db.ChequePresentments
            .AnyAsync(
                existing =>
                    existing.InstrumentId == instrumentId &&
                    (
                        existing.Status == PresentmentStatus.Received ||
                        existing.Status == PresentmentStatus.Validating ||
                        existing.Status == PresentmentStatus.Pending ||
                        existing.Status == PresentmentStatus.Submitted ||
                        existing.Status == PresentmentStatus.Failed
                    ),
                cancellationToken);

        if (hasActivePresentment)
        {
            throw new PresentmentConflictException(
                "This cheque already has an active presentment.");
        }

        // 5. Add the presentment and its related audit event.
        _db.ChequePresentments.Add(presentment);

        // 6. Persist both records atomically.
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsActivePresentmentUniqueViolation(exception))
        {
            // Another request may have inserted an active presentment
            // after our application-level check.
            throw new PresentmentConflictException(
                "This cheque already has an active presentment.");
        }

        return presentment;
    }

    private static bool IsActivePresentmentUniqueViolation(
        DbUpdateException exception)
    {
        // Inspect the exception chain for SQLite's unique-constraint error.
        var sqliteException = exception.InnerException;

        while (sqliteException is not null)
        {
            if (sqliteException is SqliteException sqlite &&
                sqlite.SqliteErrorCode == 19 &&
                sqlite.SqliteExtendedErrorCode == 2067 &&
                sqlite.Message.Contains(
                    "ChequePresentments.InstrumentId",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            sqliteException = sqliteException.InnerException;
        }

        return false;
    }
}
