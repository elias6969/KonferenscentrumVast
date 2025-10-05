using System;
using KonferenscentrumVast.Models;
using Microsoft.EntityFrameworkCore;

namespace KonferenscentrumVast.Data
{
    /// <summary>
    /// Primary Entity Framework Core database context for the Konferenscentrum Väst system.
    /// Handles access to all main entities and defines their relationships.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes the database context with the specified options (connection string, provider, etc.).
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- Entity Sets ---
        // Each DbSet represents a table in Cloud SQL.
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingContract> BookingContracts { get; set; }
        public DbSet<FileMetadata> Files { get; set; }
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Configures entity relationships and constraints using Fluent API.
        /// Called automatically by EF Core when building the model.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- File <-> Booking relationship ---
            // Each FileMetadata can be linked to one Booking.
            // A Booking can have multiple files (e.g., contracts, attachments).
            // If a Booking is deleted, its related files are also deleted (Cascade).
            modelBuilder.Entity<FileMetadata>()
                .HasOne(f => f.Booking)
                .WithMany(b => b.Files)
                .HasForeignKey(f => f.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- File <-> Facility relationship ---
            // Each FileMetadata can be linked to one Facility.
            // A Facility can have multiple files (e.g., images, brochures).
            // Cascade delete ensures facility file cleanup when a facility is removed.
            modelBuilder.Entity<FileMetadata>()
                .HasOne(f => f.Facility)
                .WithMany(fc => fc.Files)
                .HasForeignKey(f => f.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
