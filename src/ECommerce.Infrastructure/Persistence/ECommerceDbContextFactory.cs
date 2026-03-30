using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Persistence;

public sealed class ECommerceDbContextFactory : IDesignTimeDbContextFactory<ECommerceDbContext>
{
    public ECommerceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ECommerceDbContext>();
        var connectionString = "Server=localhost,1433;Database=ECommerceDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        return new ECommerceDbContext(optionsBuilder.Options);
    }
}
