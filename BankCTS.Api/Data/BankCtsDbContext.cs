
using BankCTS.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace BankCTS.Api.Data;

public class BankCtsDbContext : DbContext
{
    public BankCtsDbContext(
        DbContextOptions<BankCtsDbContext> options)
        : base(options)
    {
    }

    // Existing legacy entity. Keep this table intact.
    public DbSet<Cheque> Cheques => Set<Cheque>();

    // New CTS domain entities.
    public DbSet<ChequeInstrument> ChequeInstruments
        => Set<ChequeInstrument>();

    public DbSet<ChequePresentment> ChequePresentments
        => Set<ChequePresentment>();

    public DbSet<PresentmentAuditEvent> PresentmentAuditEvents
        => Set<PresentmentAuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Existing legacy Cheque configuration.
        modelBuilder.Entity<Cheque>(entity =>
        {
            entity.ToTable("Cheques");

            entity.HasKey(cheque => cheque.Id);

            entity.Property(cheque => cheque.ChequeNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(cheque => cheque.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(cheque => cheque.DrawerName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(cheque => cheque.PayeeName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(cheque => cheque.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(cheque => cheque.Status)
                .HasConversion<string>();
        });

        // Physical cheque configuration.
        modelBuilder.Entity<ChequeInstrument>(entity =>
        {
            entity.ToTable("ChequeInstruments");

            entity.HasKey(instrument => instrument.Id);

            entity.Property(instrument => instrument.IssuingBankCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(instrument => instrument.IssuingBranchCode)
                .HasMaxLength(50);

            entity.Property(instrument => instrument.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(instrument => instrument.ChequeNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(instrument => instrument.Amount)
                .HasPrecision(18, 2);

            entity.Property(instrument => instrument.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(instrument => instrument.DrawerName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(instrument => instrument.PayeeName)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasMany(instrument => instrument.Presentments)
                .WithOne(presentment => presentment.Instrument)
                .HasForeignKey(presentment => presentment.InstrumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Cheque presentment configuration.
        modelBuilder.Entity<ChequePresentment>(entity =>
        {
            entity.ToTable("ChequePresentments");

            entity.HasKey(presentment => presentment.Id);

            entity.Property(presentment => presentment.PresentingBankCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(presentment => presentment.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(presentment => presentment.ReceivedAt)
                .IsRequired();

            // Link a re-presentment to its previous attempt.
            entity.HasOne(presentment => presentment.PreviousPresentment)
                .WithMany()
                .HasForeignKey(presentment => presentment.PreviousPresentmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // One presentment can have multiple audit events.
            entity.HasMany(presentment => presentment.AuditEvents)
                .WithOne()
                .HasForeignKey(auditEvent => auditEvent.PresentmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tell EF Core to use the private audit collection.
            entity.Navigation(presentment => presentment.AuditEvents)
                .HasField("_auditEvents")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // Presentment audit event configuration.
        modelBuilder.Entity<PresentmentAuditEvent>(entity =>
        {
            entity.ToTable("PresentmentAuditEvents");

            entity.HasKey(auditEvent => auditEvent.Id);

            entity.Property(auditEvent => auditEvent.PreviousStatus)
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.Property(auditEvent => auditEvent.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(auditEvent => auditEvent.PerformedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(auditEvent => auditEvent.Reason)
                .HasMaxLength(500);

            entity.Property(auditEvent => auditEvent.OccurredAt)
                .IsRequired();
        });
    }
}
