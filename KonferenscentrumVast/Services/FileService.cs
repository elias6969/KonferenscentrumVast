using Google.Cloud.Storage.V1;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Repository.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KonferenscentrumVast.Services
{
    public class FileService
    {
        private readonly IFileRepository _repo;
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FileService(IFileRepository repo, IConfiguration config)
        {
            _repo = repo;
            _storageClient = StorageClient.Create();
            _bucketName = config["GoogleCloud:BucketName"]
                ?? throw new Exception("Bucket name not configured");
        }

        /// <summary>
        /// Uploads a file to GCS and saves metadata in SQL
        /// </summary>
        public async Task<FileResponseDto> UploadFileAsync(IFormFile file, int? bookingId, int? facilityId, string userId)
        {
            // Upload binary to GCS
            var objectName = $"{Guid.NewGuid()}-{file.FileName}";
            using var stream = file.OpenReadStream();
            await _storageClient.UploadObjectAsync(
                bucket: _bucketName,
                objectName: objectName,
                contentType: file.ContentType,
                source: stream);

            // Save metadata in SQL
            var metadata = new FileMetadata
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                StoragePath = objectName,
                BookingId = bookingId,
                FacilityId = facilityId,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(metadata);
            await _repo.SaveChangesAsync();

            // Return response DTO
            return new FileResponseDto
            {
                Id = metadata.Id,
                FileName = metadata.FileName,
                ContentType = metadata.ContentType,
                UploadedAt = metadata.UploadedAt,
                UploadedBy = metadata.UploadedBy,
                BookingId = metadata.BookingId,
                FacilityId = metadata.FacilityId,
                StoragePath = metadata.StoragePath
            };
        }

        /// <summary>
        /// Generates a signed URL for temporary download access
        /// </summary>
        public async Task<string?> GetFileUrlAsync(int fileId, TimeSpan validFor)
        {
            var file = await _repo.GetByIdAsync(fileId);
            if (file == null) return null;

            // Use a service account key (path should come from env var or Secret Manager)
            var keyPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")
                          ?? "key.json";


            var urlSigner = UrlSigner.FromCredentialFile(keyPath);


            string url = urlSigner.Sign(
                _bucketName,
                file.StoragePath,
                validFor, 
                HttpMethod.Get
            );



            return url;
        }

        public async Task<IEnumerable<FileResponseDto>> ListBookingFilesAsync(int bookingId)
        {
            var files = await _repo.GetByBookingIdAsync(bookingId);
            return files.Select(f => new FileResponseDto
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                UploadedAt = f.UploadedAt,
                UploadedBy = f.UploadedBy,
                BookingId = f.BookingId,
                FacilityId = f.FacilityId,
                StoragePath = f.StoragePath
            });
        }

        public async Task<IEnumerable<FileResponseDto>> ListFacilityFilesAsync(int facilityId)
        {
            var files = await _repo.GetByFacilityIdAsync(facilityId);
            return files.Select(f => new FileResponseDto
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                UploadedAt = f.UploadedAt,
                UploadedBy = f.UploadedBy,
                BookingId = f.BookingId,
                FacilityId = f.FacilityId,
                StoragePath = f.StoragePath
            });
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _repo.GetByIdAsync(fileId);
            if (file == null) return false;

            // Delete from GCS
            await _storageClient.DeleteObjectAsync(_bucketName, file.StoragePath);

            // Delete metadata from SQL
            await _repo.DeleteAsync(file);
            await _repo.SaveChangesAsync();

            return true;
        }
    }
}
