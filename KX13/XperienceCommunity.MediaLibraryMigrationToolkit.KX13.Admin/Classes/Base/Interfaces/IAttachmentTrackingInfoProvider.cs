using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Declares members for <see cref="AttachmentTrackingInfo"/> management.
    /// </summary>
    public partial interface IAttachmentTrackingInfoProvider : IInfoProvider<AttachmentTrackingInfo>, IInfoByIdProvider<AttachmentTrackingInfo>
    {
    }
}