using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="AttachmentIDToGuidCloneInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IAttachmentIDToGuidCloneInfoProvider))]
    public partial class AttachmentIDToGuidCloneInfoProvider : AbstractInfoProvider<AttachmentIDToGuidCloneInfo, AttachmentIDToGuidCloneInfoProvider>, IAttachmentIDToGuidCloneInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentIDToGuidCloneInfoProvider"/> class.
        /// </summary>
        public AttachmentIDToGuidCloneInfoProvider()
            : base(AttachmentIDToGuidCloneInfo.TYPEINFO)
        {
        }
    }
}