
using BankCTS.Api.Application.Services;
using BankCTS.Api.Data;
using BankCTS.Api.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BankCTS.Tests.Integration;

public class PresentmentServiceTests
{
    [Fact]
    public async Task CreateAsync_PersistsPresentmentAndInitialAuditEvent()
    {
        // Arrange: create an isolated in-memory SQLite database.
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BankCtsDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid instrumentId;

        // Create the database and save a physical cheque instrument.
        await using (var setupContext = new BankCtsDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var instrument = new ChequeInstrument(
                issuingBankCode: "BANK01",
                issuingBranchCode: null,
                accountNumber: "000123456789",
                chequeNumber: "000001",
                amount: 1250.50m,
                currency: "GHS",
                chequeDate: new DateOnly(2026, 11, 17),
                drawerName: "Kwame Mensah",
                payeeName: "Ama Boateng");

            setupContext.ChequeInstruments.Add(instrument);

            await setupContext.SaveChangesAsync();

            instrumentId = instrument.Id;
        }

        // Act: create a presentment through the application service.
        await using (var actContext = new BankCtsDbContext(options))
        {
            var service = new PresentmentService(actContext);

            await service.CreateAsync(
                instrumentId,
                "BANK02",
                "test-operator");
        }

        // Assert: verify that the presentment was persisted.
        await using (var verifyContext = new BankCtsDbContext(options))
        {
            var savedPresentment = await verifyContext.ChequePresentments
                .AsNoTracking()
                .SingleAsync();

            Assert.Equal(instrumentId, savedPresentment.InstrumentId);
            Assert.Equal("BANK02", savedPresentment.PresentingBankCode);
            Assert.Equal(PresentmentStatus.Received, savedPresentment.Status);

            // Verify that its initial audit event was also persisted.
            var auditEvents = await verifyContext.PresentmentAuditEvents
                .AsNoTracking()
                .Where(auditEvent =>
                    auditEvent.PresentmentId == savedPresentment.Id)
                .ToListAsync();

            Assert.Single(auditEvents);

            var initialAuditEvent = auditEvents[0];

            Assert.Null(initialAuditEvent.PreviousStatus);
            Assert.Equal(
                PresentmentStatus.Received,
                initialAuditEvent.NewStatus);

            Assert.Equal("test-operator", initialAuditEvent.PerformedBy);
            Assert.Equal(savedPresentment.ReceivedAt, initialAuditEvent.OccurredAt);
        }
    }


[Fact]
public async Task CreateAsync_RollsBackPresentmentWhenAuditEventInsertFails()
{
    // Arrange: create an isolated in-memory SQLite database.
    await using var connection =
        new SqliteConnection("Data Source=:memory:");

    await connection.OpenAsync();

    var options = new DbContextOptionsBuilder<BankCtsDbContext>()
        .UseSqlite(connection)
        .Options;

    Guid instrumentId;

    await using (var setupContext = new BankCtsDbContext(options))
    {
        await setupContext.Database.EnsureCreatedAsync();

        var instrument = new ChequeInstrument(
            issuingBankCode: "BANK01",
            issuingBranchCode: null,
            accountNumber: "000123456789",
            chequeNumber: "000002",
            amount: 2000.00m,
            currency: "GHS",
            chequeDate: new DateOnly(2026, 11, 17),
            drawerName: "Kofi Mensah",
            payeeName: "Akosua Owusu");

        setupContext.ChequeInstruments.Add(instrument);

        await setupContext.SaveChangesAsync();

        instrumentId = instrument.Id;

        // Simulate a database failure when inserting an audit event.
        await setupContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TRIGGER FailAuditInsert
            BEFORE INSERT ON PresentmentAuditEvents
            BEGIN
                SELECT RAISE(ABORT, 'Simulated audit insert failure');
            END;
            """);
    }

    // Act: attempt to create a presentment.
    await using (var actContext = new BankCtsDbContext(options))
    {
        var service = new PresentmentService(actContext);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => service.CreateAsync(
                instrumentId,
                "BANK02",
                "test-operator"));
    }

    // Assert: neither record should remain in the database.
    await using (var verifyContext = new BankCtsDbContext(options))
    {
        int presentmentCount = await verifyContext.ChequePresentments
            .CountAsync();

        int auditEventCount = await verifyContext.PresentmentAuditEvents
            .CountAsync();

        Assert.Equal(0, presentmentCount);
        Assert.Equal(0, auditEventCount);
    }
}

}
