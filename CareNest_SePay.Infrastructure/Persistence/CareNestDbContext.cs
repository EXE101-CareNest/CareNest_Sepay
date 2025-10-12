using CareNest_SePay.Domain.Entities;
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
                entity.Property(e => e.Gateway).HasMaxLength(100);
                entity.Property(e => e.AccountNumber).HasMaxLength(50);
                entity.Property(e => e.SubAccount).HasMaxLength(50);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            });
        }
    }
}


