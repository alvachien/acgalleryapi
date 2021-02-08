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
        public DbSet<Photo> Photos { get; set; }
        public DbSet<PhotoTag> PhotoTags { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<AlbumPhoto> AlbumPhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<Customer>().OwnsOne(c => c.HomeAddress).WithOwner();

            // .NET 5.0
            //modelBuilder.Entity<Album>()
            //    .HasMany(p => p.Photos)
            //    .WithMany(p => p.Albums)
            //    .UsingEntity<AlbumPhoto>(
            //        j => j
            //            .HasOne(pt => pt.)
            //            .WithMany(t => t.)
            //            .HasForeignKey(pt => pt.TagId),
            //        j => j
            //            .HasOne(pt => pt.Post)
            //            .WithMany(p => p.PostTags)
            //            .HasForeignKey(pt => pt.PostId),
            //        j =>
            //        {
            //            j.Property(pt => pt.PublicationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            //            j.HasKey(t => new { t.PostId, t.TagId });
            //        });

            modelBuilder.Entity<AlbumPhoto>()
                .HasKey(t => new { t.AlbumID, t.PhotoID });
            
            modelBuilder.Entity<AlbumPhoto>()
                .HasOne(pt => pt.CurrentAlbum)
                .WithMany(p => p.AlbumPhotos)
                .HasForeignKey(pt => pt.AlbumID);

            modelBuilder.Entity<AlbumPhoto>()
                .HasOne(pt => pt.CurrentPhoto)
                .WithMany(t => t.AlbumPhotos)
                .HasForeignKey(pt => pt.PhotoID);

            modelBuilder.Entity<PhotoTag>().HasKey(t => new { t.PhotoID, t.TagString });

            modelBuilder.Entity<PhotoTag>().HasOne(p => p.CurrentPhoto).WithMany(t => t.Tags).HasForeignKey(pt => pt.PhotoID);
        }
    }
}
