using Microsoft.EntityFrameworkCore;
using Tablebooking.Models;

namespace Tablebooking.Models
{
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<DinningTable> DinningTables { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DinningTable>()
                .HasMany(t => t.Bookings)
                .WithOne(b => b.DinningTable)
                .HasForeignKey(b => b.DinningTableID)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }


}
}
