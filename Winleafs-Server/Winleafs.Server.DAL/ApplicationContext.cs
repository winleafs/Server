using Microsoft.EntityFrameworkCore;

namespace Winleafs.Server.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options)
        {
        }

        public ApplicationContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Use this to add Entity Configurations.
        }
    }
}