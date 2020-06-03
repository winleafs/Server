using Microsoft.EntityFrameworkCore;
using Winleafs.Server.Models.Models;

namespace Winleafs.Server.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }

        public ApplicationContext()
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Use this to add Entity Configurations.
        }
    }
}