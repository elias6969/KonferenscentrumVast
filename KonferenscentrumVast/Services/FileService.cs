using Google.Cloud.Storage.V1;
using KonferenscentrumVast.DTO;
using KonferenscentrumVast.Models;
using KonferenscentrumVast.Options;
using KonferenscentrumVast.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KonferenscentrumVast.Services
{
    public class FileService
    {
        private readonly IFileRepository _repo;
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly ILogger<FileService> _logger;

        public FileService(IFileRepository repo, FileStorageOptions options, ILogger<FileService> logger)
        {
            _repo = repo;
            _storageClient = StorageClient.Create();
            _bucketName = options.BucketName
                ?? throw new Exception("Bucket name not configured");
            _logger = logger;
        }

        /// <summary>
        /// Uploads a file to GCS and stores metadata in SQL.
        /// </summary>
        public async Task<FileResponseDto> UploadFileAsync(IFormFile file, int? bookingId, int? facilityId, string userId)
        {
            var objectName = $"{Guid.NewGuid()}-{file.FileName}";
            using var stream = file.OpenReadStream();

            _logger.LogInformation("$ Uploading file {FileName} by user {UserId} (booking={BookingId}, facility={FacilityId})",
                file.FileName, userId, bookingId, facilityId);

            await _storageClient.UploadObjectAsync(
                bucket: _bucketName,
                objectName: objectName,
                contentType: file.ContentType,
                source: stream);

            _logger.LogInformation("$ File stored in bucket {Bucket} at {Path}", _bucketName, objectName);

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

            _logger.LogInformation("$ File metadata saved with Id {FileId}", metadata.Id);

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
        /// Generates a signed URL for temporary download access.
        /// </summary>
        public async Task<string?> GetFileUrlAsync(int fileId, TimeSpan validFor)
        {
            var file = await _repo.GetByIdAsync(fileId);
            if (file == null)
            {
                _logger.LogWarning("$ Tried to get signed URL for missing file {FileId}", fileId);
                return null;
            }

            Google.Apis.Auth.OAuth2.GoogleCredential googleCredential;

            var keyPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (!string.IsNullOrEmpty(keyPath))
            {
                googleCredential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(keyPath);
            }
            else
            {
                googleCredential = await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync();
            }

            var urlSigner = UrlSigner.FromCredential(googleCredential);
            var url = urlSigner.Sign(_bucketName, file.StoragePath, validFor, HttpMethod.Get);

            _logger.LogInformation("$ Generated signed URL for file {FileId}, valid for {Minutes} minutes",
                fileId, validFor.TotalMinutes);

            return url;
        }

        /// <summary>
        /// Lists all files linked to a booking.
        /// </summary>
        public async Task<IEnumerable<FileResponseDto>> ListBookingFilesAsync(int bookingId)
        {
            var files = await _repo.GetByBookingIdAsync(bookingId);
            _logger.LogInformation("$ Retrieved {Count} files for booking {BookingId}", files.Count(), bookingId);

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

        /// <summary>
        /// Lists all files linked to a facility.
        /// </summary>
        public async Task<IEnumerable<FileResponseDto>> ListFacilityFilesAsync(int facilityId)
        {
            var files = await _repo.GetByFacilityIdAsync(facilityId);
            _logger.LogInformation("$ Retrieved {Count} files for facility {FacilityId}", files.Count(), facilityId);

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

        /// <summary>
        /// Deletes a file from both GCS and SQL.
        /// </summary>
        public async Task<bool> DeleteFileAsync(int fileId)
        {
            var file = await _repo.GetByIdAsync(fileId);
            if (file == null)
            {
                _logger.LogWarning("$ Delete requested for non-existing file {FileId}", fileId);
                return false;
            }

            await _storageClient.DeleteObjectAsync(_bucketName, file.StoragePath);
            _logger.LogInformation("$ Deleted file {FileId} from bucket {Bucket}", fileId, _bucketName);

            await _repo.DeleteAsync(file);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("$ Deleted file metadata {FileId} from database", fileId);

            return true;
        }
    }
}
