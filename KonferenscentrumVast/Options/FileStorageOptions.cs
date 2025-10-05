namespace KonferenscentrumVast.Options
{
    /// <summary>
    /// Configuration options for Google Cloud file storage.
    /// Holds the name of the storage bucket used by FileService.
    /// </summary>
    public class FileStorageOptions
    {
        public string BucketName { get; set; } = string.Empty;
    }
}
