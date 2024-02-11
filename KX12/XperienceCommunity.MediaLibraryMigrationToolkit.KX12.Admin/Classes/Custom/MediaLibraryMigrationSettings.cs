using System;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public class MediaLibraryMigrationSettings
    {
        public bool UpdateMediaFileUrlsToPermanent { get; set; } = true;
        public bool ConvertAttachments { get; set; } = false;
        public string AttachmentMediaLibraryName { get; set; } = string.Empty;
        public int AttachmentTruncateFileNameLengthToPreservePath { get; set; } = 200;
        public bool LowercaseNewUrls { get; set; } = true;

        public AttachmentFailureMode AttachmentFailureForGuidColumn { get; set; } = AttachmentFailureMode.Leave;

        public MediaMissingMode AttachmentFailureForUrl { get; set; } = MediaMissingMode.Leave;

        public MediaMissingMode MediaMissing { get; set; } = MediaMissingMode.Leave;
        
        public Guid MediaNotFoundGuid { get; set; } = Guid.Empty;


    }

    public enum AttachmentFailureMode
    {
        SetNull,
        Leave,
        SetToMediaNotFoundGuid
    }

    public enum MediaMissingMode
    {
        Leave,
        SetToMediaNotFoundGuid
    }

    
}
