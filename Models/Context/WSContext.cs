using Microsoft.EntityFrameworkCore;

namespace VueServer.Models.Context
{
    public class WSContext : DbContext
    {
        public WSContext(DbContextOptions<WSContext> options) : base(options) 
        {

        }

        public WSContext ()
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }

        // Database tables

        public DbSet<Notes> Notes { get; set; }
    }
}
