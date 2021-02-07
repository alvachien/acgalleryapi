using Microsoft.EntityFrameworkCore;

namespace GalleryAPI.Models
{
    public class GalleryContext : DbContext
    {
        public GalleryContext(DbContextOptions<GalleryContext> options)
            : base(options)
        {
        }

        public DbSet<Album> Albums { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress).WithOwner();
        }
    }
}
