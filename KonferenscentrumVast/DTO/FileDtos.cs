using System;

namespace KonferenscentrumVast.DTO
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
        public int? BookingId { get; set; }
        public int? FacilityId { get; set; }
    }

    public class FileResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;

        public int? BookingId { get; set; }
        public int? FacilityId { get; set; }

        public string? DownloadUrl { get; set; }
    }
}
