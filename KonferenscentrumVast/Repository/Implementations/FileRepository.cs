using System;
using Microsoft.EntityFrameworkCore;
using KonferenscentrumVast.Data;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;

namespace KonferenscentrumVast.Repository.Implementations
{
    public class FileRepository : IFileRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Injects the application's database context.
        /// </summary>
        public FileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a file metadata record by its unique ID.
        /// </summary>
        public async Task<FileMetadata?> GetByIdAsync(int id)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
        }

        /// <summary>
        /// Retrieves all files linked to a specific booking.
        /// </summary>
        public async Task<IEnumerable<FileMetadata>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Files.Where(f => f.BookingId == bookingId).ToListAsync();
        }

        /// <summary>
        /// Retrieves all files linked to a specific facility.
        /// </summary>
        public async Task<IEnumerable<FileMetadata>> GetByFacilityIdAsync(int facilityId)
        {
            return await _context.Files.Where(f => f.FacilityId == facilityId).ToListAsync();
        }

        /// <summary>
        /// Adds a new FileMetadata entry to the database context.
        /// Actual save occurs when SaveChangesAsync() is called.
        /// </summary>
        public async Task AddAsync(FileMetadata file)
        {
            await _context.Files.AddAsync(file);
        }

        /// <summary>
        /// Removes a FileMetadata entry from the database context.
        /// Actual delete occurs when SaveChangesAsync() is called.
        /// </summary>
        public async Task DeleteAsync(FileMetadata file)
        {
            _context.Files.Remove(file);
            await Task.CompletedTask; 
        }

        /// <summary>
        /// Persists all pending changes to the database.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
