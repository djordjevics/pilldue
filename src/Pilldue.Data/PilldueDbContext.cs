using Microsoft.EntityFrameworkCore;
using Pilldue.Business;

namespace Pilldue.Data;

/// <summary>
/// EF Core context for medications and related history tables.
/// Repository CRUD is out of scope for B1; this maps schema only.
/// </summary>
public sealed class PilldueDbContext : DbContext
{
    public PilldueDbContext(DbContextOptions<PilldueDbContext> options)
        : base(options)
    {
    }

    public DbSet<Medication> Medications => Set<Medication>();

    public DbSet<RefillEvent> RefillEvents => Set<RefillEvent>();

    public DbSet<SkipDoseEvent> SkipDoseEvents => Set<SkipDoseEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medication>(entity =>
        {
            entity.ToTable("medications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.PackageSizePills).IsRequired();
            entity.Property(e => e.PrescribedPackageCount).IsRequired();
            entity.Property(e => e.DailyDosagePills).IsRequired();
            entity.Property(e => e.CurrentStockPills).IsRequired();
            entity.Property(e => e.RefillDayOfMonthOverride);
            entity.Property(e => e.PrescriptionStartDate).IsRequired();
            entity.Property(e => e.PrescriptionDurationMonths).IsRequired();
        });

        modelBuilder.Entity<RefillEvent>(entity =>
        {
            entity.ToTable("refill_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationId).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.PackageCount).IsRequired();
            entity.HasIndex(e => e.MedicationId);
            entity.HasOne<Medication>()
                .WithMany()
                .HasForeignKey(e => e.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SkipDoseEvent>(entity =>
        {
            entity.ToTable("skip_dose_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationId).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.PillsReturned).IsRequired();
            entity.HasIndex(e => e.MedicationId);
            entity.HasOne<Medication>()
                .WithMany()
                .HasForeignKey(e => e.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
