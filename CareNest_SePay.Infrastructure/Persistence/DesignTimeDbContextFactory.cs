using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CareNest_SePay.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CareNestDbContext>
    {
        public CareNestDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CareNestDbContext>();
            
            // Use default connection string for design time
            optionsBuilder.UseNpgsql("Host=localhost;Database=CareNest_SePay;Username=postgres;Password=password");
            
            return new CareNestDbContext(optionsBuilder.Options);
        }
    }
}
