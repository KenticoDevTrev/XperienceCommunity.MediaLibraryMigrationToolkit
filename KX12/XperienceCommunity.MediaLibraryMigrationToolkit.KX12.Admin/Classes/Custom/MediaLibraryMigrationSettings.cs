namespace XperienceCommunity.MediaLibraryMigrationToolkit.Helpers
{
    public class MediaLibraryMigrationSettings
    {
        public bool ConvertAttachments { get; set; } = false;
        public string AttachmentMediaLibraryName { get; set; } = string.Empty;
        public bool LowercaseNewUrls { get; set; } = true;


    }
}
