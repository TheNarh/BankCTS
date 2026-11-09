
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

    public DbSet<Cheque> Cheques => Set<Cheque>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
    }
}
