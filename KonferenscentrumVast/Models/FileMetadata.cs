using System;

namespace KonferenscentrumVast.Models
{
    /// <summary>
    /// Represents metadata for a file stored in Google Cloud Storage
    /// Can be associated with either a booking (e.g. contracts)
    /// or a facility (e.g. facility images).
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
