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

        public FileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FileMetadata?> GetByIdAsync(int id)
        {
            return await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<FileMetadata>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Files.Where(f => f.BookingId == bookingId).ToListAsync();
        }

        public async Task<IEnumerable<FileMetadata>> GetByFacilityIdAsync(int facilityId)
        {
            return await _context.Files.Where(f => f.FacilityId == facilityId).ToListAsync();
        }

        public async Task AddAsync(FileMetadata file)
        {
            await _context.Files.AddAsync(file);
        }

        public async Task DeleteAsync(FileMetadata file)
        {
            _context.Files.Remove(file);
            await Task.CompletedTask; 
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
