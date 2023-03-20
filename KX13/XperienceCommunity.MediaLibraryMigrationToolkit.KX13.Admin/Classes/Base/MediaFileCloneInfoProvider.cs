using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="Media_FileCloneInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IMedia_FileCloneInfoProvider))]
    public partial class Media_FileCloneInfoProvider : AbstractInfoProvider<Media_FileCloneInfo, Media_FileCloneInfoProvider>, IMedia_FileCloneInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Media_FileCloneInfoProvider"/> class.
        /// </summary>
        public Media_FileCloneInfoProvider()
            : base(Media_FileCloneInfo.TYPEINFO)
        {
        }
    }
}