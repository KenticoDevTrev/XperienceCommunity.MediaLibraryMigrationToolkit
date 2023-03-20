using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="FileTrackingInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IFileTrackingInfoProvider))]
    public partial class FileTrackingInfoProvider : AbstractInfoProvider<FileTrackingInfo, FileTrackingInfoProvider>, IFileTrackingInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTrackingInfoProvider"/> class.
        /// </summary>
        public FileTrackingInfoProvider()
            : base(FileTrackingInfo.TYPEINFO)
        {
        }
    }
}