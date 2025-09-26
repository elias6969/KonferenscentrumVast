using System;
using KonferenscentrumVast.Models;
using Microsoft.EntityFrameworkCore;

namespace KonferenscentrumVast.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingContract> BookingContracts { get; set; }
        public DbSet<FileMetadata> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FileMetadata>()
                .HasOne(f => f.Booking)
                .WithMany(b => b.Files)  // Make sure Booking.cs has ICollection<FileMetadata> Files
                .HasForeignKey(f => f.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // File ↔ Facility
            modelBuilder.Entity<FileMetadata>()
                .HasOne(f => f.Facility)
                .WithMany(fc => fc.Files) // Make sure Facility.cs has ICollection<FileMetadata> Files
                .HasForeignKey(f => f.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
