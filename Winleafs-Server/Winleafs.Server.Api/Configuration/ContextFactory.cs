using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using Winleafs.Server.Data;

namespace Winleafs.Server.Api.Configuration
{
    /// <summary>
    /// Adds the context information to this project
    /// </summary>
    public class ContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        /// <inheritdoc />
        public ApplicationContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString, sqlServerOptions =>
                sqlServerOptions.MigrationsAssembly("Winleafs.Server.Data"));

            return new ApplicationContext(builder.Options);
        }
    }
}
