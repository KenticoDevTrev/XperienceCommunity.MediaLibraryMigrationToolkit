using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Declares members for <see cref="AttachmentIDToGuidCloneInfo"/> management.
    /// </summary>
    public partial interface IAttachmentIDToGuidCloneInfoProvider : IInfoProvider<AttachmentIDToGuidCloneInfo>, IInfoByIdProvider<AttachmentIDToGuidCloneInfo>
    {
    }
}