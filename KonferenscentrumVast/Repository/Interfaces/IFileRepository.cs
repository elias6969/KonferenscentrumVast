using System;
using KonferenscentrumVast.Models;

namespace KonferenscentrumVast.Repository.Interfaces
{
    /// <summary>
    /// Defines database operations for managing file metadata.
    /// Responsible for CRUD actions related to uploaded files 
    /// stored in Cloud SQL (metadata only, not file content).
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Retrieves a file metadata record by its unique ID.
        /// </summary>
        Task<FileMetadata?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all files linked to a specific booking.
        /// </summary>
        Task<IEnumerable<FileMetadata>> GetByBookingIdAsync(int bookingId);

        /// <summary>
        /// Retrieves all files linked to a specific facility.
        /// </summary>
        Task<IEnumerable<FileMetadata>> GetByFacilityIdAsync(int facilityId);

        /// <summary>
        /// Adds a new FileMetadata entry to the database context.
        /// </summary>
        Task AddAsync(FileMetadata file);

        /// <summary>
        /// Marks a FileMetadata entry for removal from the database context.
        /// </summary>
        Task DeleteAsync(FileMetadata file);

        /// <summary>
        /// Persists all pending changes to the database.
        /// </summary>
        Task SaveChangesAsync();
    }
}
