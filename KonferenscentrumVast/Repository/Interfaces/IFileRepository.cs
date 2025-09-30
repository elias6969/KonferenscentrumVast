using System;
using KonferenscentrumVast.Models;

namespace KonferenscentrumVast.Repository.Interfaces
{
    public interface IFileRepository
    {
        Task<FileMetadata?> GetByIdAsync(int id);
        Task<IEnumerable<FileMetadata>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<FileMetadata>> GetByFacilityIdAsync(int facilityId);

        Task AddAsync(FileMetadata file);
        Task DeleteAsync(FileMetadata file);
        Task SaveChangesAsync();
    }
}
