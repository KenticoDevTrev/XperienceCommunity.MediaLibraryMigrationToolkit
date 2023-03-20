using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="AttachmentTrackingInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAttachmentTrackingInfoProvider))]
    public partial class AttachmentTrackingInfoProvider : AbstractInfoProvider<AttachmentTrackingInfo, AttachmentTrackingInfoProvider>, IAttachmentTrackingInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentTrackingInfoProvider"/> class.
        /// </summary>
        public AttachmentTrackingInfoProvider()
            : base(AttachmentTrackingInfo.TYPEINFO)
        {
        }
    }
}