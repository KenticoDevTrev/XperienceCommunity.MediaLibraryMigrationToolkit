namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public class MediaLibraryMigrationSettings
    {
        public bool UpdateMediaFileUrlsToPermanent { get; set; } = true;
        public bool ConvertAttachments { get; set; } = false;
        public string AttachmentMediaLibraryName { get; set; } = string.Empty;
        public bool LowercaseNewUrls { get; set; } = true;

    }
}
