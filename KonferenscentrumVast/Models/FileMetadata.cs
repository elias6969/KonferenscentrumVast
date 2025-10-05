using System;

namespace KonferenscentrumVast.Models
{
    /// <summary>
    /// Represents information about an uploaded file.
    /// Used for storing metadata about files in Cloud SQL,
    /// while the actual file is stored in Google Cloud Storage.
    /// </summary>
    public class FileMetadata
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = string.Empty;

        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int? FacilityId { get; set; }
        public Facility? Facility { get; set; }
    }
}
