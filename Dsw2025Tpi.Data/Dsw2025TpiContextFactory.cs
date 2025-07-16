using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;



namespace Dsw2025Tpi.Data
{
    public class Dsw2025TpiContextFactory : IDesignTimeDbContextFactory<Dsw2025TpiContext>
    {
        public Dsw2025TpiContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<Dsw2025TpiContext>();
            var connectionString = configuration.GetConnectionString("Dsw2025TpiEntities");

            optionsBuilder.UseSqlServer(connectionString);

            return new Dsw2025TpiContext(optionsBuilder.Options);
        }
    }
}
