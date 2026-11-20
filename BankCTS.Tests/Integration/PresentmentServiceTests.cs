
using BankCTS.Api.Application.Exceptions;
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

        await using (var actContext = new BankCtsDbContext(options))
        {
            var service = new PresentmentService(actContext);

            await service.CreateAsync(
                instrumentId,
                "BANK02",
                "test-operator");
        }

        await using (var verifyContext = new BankCtsDbContext(options))
        {
            var savedPresentment = await verifyContext.ChequePresentments
                .AsNoTracking()
                .SingleAsync();

            Assert.Equal(instrumentId, savedPresentment.InstrumentId);
            Assert.Equal("BANK02", savedPresentment.PresentingBankCode);
            Assert.Equal(PresentmentStatus.Received, savedPresentment.Status);

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

            await setupContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TRIGGER FailAuditInsert
                BEFORE INSERT ON PresentmentAuditEvents
                BEGIN
                    SELECT RAISE(ABORT, 'Simulated audit insert failure');
                END;
                """);
        }

        await using (var actContext = new BankCtsDbContext(options))
        {
            var service = new PresentmentService(actContext);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => service.CreateAsync(
                    instrumentId,
                    "BANK02",
                    "test-operator"));
        }

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

    [Fact]
    public async Task CreateAsync_ThrowsKeyNotFoundException_WhenInstrumentDoesNotExist()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BankCtsDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new BankCtsDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var service = new PresentmentService(context);

        Guid nonexistentInstrumentId = Guid.NewGuid();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(
                nonexistentInstrumentId,
                "BANK02",
                "test-operator"));

        Assert.Equal(
            0,
            await context.ChequePresentments.CountAsync());

        Assert.Equal(
            0,
            await context.PresentmentAuditEvents.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_RejectsEmptyInstrumentId()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BankCtsDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new BankCtsDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var service = new PresentmentService(context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                Guid.Empty,
                "BANK02",
                "test-operator"));

        Assert.Equal(0, await context.ChequePresentments.CountAsync());
        Assert.Equal(0, await context.PresentmentAuditEvents.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_RejectsBlankPresentingBankCode()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BankCtsDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new BankCtsDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var instrument = new ChequeInstrument(
            "BANK01",
            "BRANCH01",
            "0012345678",
            "CHQ001",
            1500m,
            "GHS",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "John Doe",
            "Jane Doe");

        context.ChequeInstruments.Add(instrument);
        await context.SaveChangesAsync();

        var service = new PresentmentService(context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                instrument.Id,
                "   ",
                "test-operator"));

        Assert.Equal(0, await context.ChequePresentments.CountAsync());
        Assert.Equal(0, await context.PresentmentAuditEvents.CountAsync());
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("123456789012345678901234567890123456789012345678901")]
    public async Task CreateAsync_RejectsInvalidPresentingBankCode(
        string presentingBankCode)
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BankCtsDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new BankCtsDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var instrument = new ChequeInstrument(
            "BANK01",
            "BRANCH01",
            "0012345678",
            "CHQ001",
            1500m,
            "GHS",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "John Doe",
            "Jane Doe");

        context.ChequeInstruments.Add(instrument);
        await context.SaveChangesAsync();

        var service = new PresentmentService(context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                instrument.Id,
                presentingBankCode,
                "test-operator"));

        Assert.Equal(0, await context.ChequePresentments.CountAsync());
        Assert.Equal(0, await context.PresentmentAuditEvents.CountAsync());
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")]
    public async Task CreateAsync_RejectsInvalidReceivingOperator(
        string receivedBy)
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BankCtsDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new BankCtsDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var instrument = new ChequeInstrument(
            "BANK01",
            "BRANCH01",
            "0012345678",
            "CHQ001",
            1500m,
            "GHS",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "John Doe",
            "Jane Doe");

        context.ChequeInstruments.Add(instrument);
        await context.SaveChangesAsync();

        var service = new PresentmentService(context);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                instrument.Id,
                "BANK02",
                receivedBy));

        Assert.Equal(0, await context.ChequePresentments.CountAsync());
        Assert.Equal(0, await context.PresentmentAuditEvents.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_RejectsSecondActivePresentment()
    {
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
                "BANK01",
                null,
                "000123456789",
                "000003",
                1250m,
                "GHS",
                new DateOnly(2026, 11, 17),
                "Kwame Mensah",
                "Ama Boateng");

            setupContext.ChequeInstruments.Add(instrument);
            await setupContext.SaveChangesAsync();

            instrumentId = instrument.Id;
        }

        await using (var actContext = new BankCtsDbContext(options))
        {
            var service = new PresentmentService(actContext);

            await service.CreateAsync(
                instrumentId,
                "BANK02",
                "test-operator");

            await Assert.ThrowsAsync<PresentmentConflictException>(
                () => service.CreateAsync(
                    instrumentId,
                    "BANK02",
                    "test-operator"));
        }

        await using (var verifyContext = new BankCtsDbContext(options))
        {
            Assert.Equal(
                1,
                await verifyContext.ChequePresentments.CountAsync());

            Assert.Equal(
                1,
                await verifyContext.PresentmentAuditEvents.CountAsync());
        }
    }

    [Fact]
    public async Task Database_RejectsSecondActivePresentment_WhenServiceIsBypassed()
    {
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
                "BANK01",
                null,
                "000123456789",
                "000004",
                1500m,
                "GHS",
                new DateOnly(2026, 11, 17),
                "Kofi Mensah",
                "Akosua Owusu");

            setupContext.ChequeInstruments.Add(instrument);
            await setupContext.SaveChangesAsync();

            instrumentId = instrument.Id;

            var firstPresentment = new ChequePresentment(
                instrumentId,
                "BANK02",
                DateTimeOffset.UtcNow,
                "test-operator");

            setupContext.ChequePresentments.Add(firstPresentment);
            await setupContext.SaveChangesAsync();
        }

        await using (var actContext = new BankCtsDbContext(options))
        {
            var duplicatePresentment = new ChequePresentment(
                instrumentId,
                "BANK02",
                DateTimeOffset.UtcNow,
                "test-operator");

            actContext.ChequePresentments.Add(duplicatePresentment);

            await Assert.ThrowsAsync<DbUpdateException>(
                () => actContext.SaveChangesAsync());
        }

        await using (var verifyContext = new BankCtsDbContext(options))
        {
            Assert.Equal(
                1,
                await verifyContext.ChequePresentments.CountAsync());

            Assert.Equal(
                1,
                await verifyContext.PresentmentAuditEvents.CountAsync());
        }
    }

    [Fact]
    public async Task ReturnedPresentment_AllowsNewLinkedPresentment()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BankCtsDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid instrumentId;
        Guid previousPresentmentId;

        await using (var setupContext = new BankCtsDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var instrument = new ChequeInstrument(
                "BANK01",
                null,
                "000123456789",
                "000005",
                2000m,
                "GHS",
                new DateOnly(2026, 11, 17),
                "Yaw Mensah",
                "Esi Boateng");

            setupContext.ChequeInstruments.Add(instrument);
            await setupContext.SaveChangesAsync();

            instrumentId = instrument.Id;

            var previousPresentment = new ChequePresentment(
                instrumentId,
                "BANK02",
                DateTimeOffset.UtcNow,
                "test-operator");

            previousPresentment.TransitionTo(
                PresentmentStatus.Validating,
                "test-operator");

            previousPresentment.TransitionTo(
                PresentmentStatus.Pending,
                "test-operator");

            previousPresentment.TransitionTo(
                PresentmentStatus.Submitted,
                "test-operator");

            // Use the dedicated Return method with a structured reason code.
            previousPresentment.Return(
                "INSUFFICIENT_FUNDS",
                "test-operator",
                "Insufficient funds");

            setupContext.ChequePresentments.Add(previousPresentment);
            await setupContext.SaveChangesAsync();

            previousPresentmentId = previousPresentment.Id;
        }

        await using (var actContext = new BankCtsDbContext(options))
        {
            var newPresentment = new ChequePresentment(
                instrumentId,
                "BANK02",
                DateTimeOffset.UtcNow,
                "test-operator",
                previousPresentmentId);

            actContext.ChequePresentments.Add(newPresentment);
            await actContext.SaveChangesAsync();
        }

        await using (var verifyContext = new BankCtsDbContext(options))
        {
            var presentments = await verifyContext.ChequePresentments
                .AsNoTracking()
                .ToListAsync();

            Assert.Equal(2, presentments.Count);

            var returnedPresentment = presentments.Single(
                presentment => presentment.Id == previousPresentmentId);

            var newPresentment = presentments.Single(
                presentment =>
                    presentment.PreviousPresentmentId == previousPresentmentId);

            Assert.Equal(
                PresentmentStatus.Returned,
                returnedPresentment.Status);

            Assert.Equal(
                "INSUFFICIENT_FUNDS",
                returnedPresentment.ReturnReasonCode);

            Assert.Equal(
                PresentmentStatus.Received,
                newPresentment.Status);

            Assert.Equal(
                instrumentId,
                newPresentment.InstrumentId);

            Assert.Equal(
                "BANK02",
                newPresentment.PresentingBankCode);

            Assert.NotEqual(
                returnedPresentment.Id,
                newPresentment.Id);

            var returnAuditEvent = await verifyContext.PresentmentAuditEvents
                .AsNoTracking()
                .SingleAsync(auditEvent =>
                    auditEvent.PresentmentId == previousPresentmentId &&
                    auditEvent.NewStatus == PresentmentStatus.Returned);

            Assert.Equal(
                "INSUFFICIENT_FUNDS",
                returnAuditEvent.ReturnReasonCode);

            Assert.Equal(
                6,
                await verifyContext.PresentmentAuditEvents.CountAsync());
        }
    }
}
