using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Adapter.SQL.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=WalletDb;Trusted_Connection=True;");//ENV VARIABLE OR CONFIG FILE IN PRODUCTION

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
