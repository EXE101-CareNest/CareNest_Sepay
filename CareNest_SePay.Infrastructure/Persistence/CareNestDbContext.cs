using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Domain.Commons;
using Microsoft.EntityFrameworkCore;

namespace CareNest_SePay.Infrastructure.Persistence
{
    public class CareNestDbContext : DbContext
    {
        public CareNestDbContext(DbContextOptions<CareNestDbContext> options) : base(options)
        {
        }

        public DbSet<SepayTransaction> SepayTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SepayTransaction>(entity =>
            {
                entity.ToTable("SepayTransactions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(32);
                entity.Property(e => e.Gateway).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.AccountNumber).HasMaxLength(50);
                entity.Property(e => e.SubAccount).HasMaxLength(50);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
                entity.Property(e => e.TransactionContent).HasMaxLength(500);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.Body).HasColumnType("text");
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                entity.Property(e => e.DeletedBy).HasMaxLength(100);
                
                // Fix decimal precision warnings
                entity.Property(e => e.AmountIn)
                    .HasPrecision(18, 2);
                entity.Property(e => e.AmountOut)
                    .HasPrecision(18, 2);
                entity.Property(e => e.Accumulated)
                    .HasPrecision(18, 2);
                
                // Indexes for performance
                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.AccountNumber);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-set audit fields
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}


