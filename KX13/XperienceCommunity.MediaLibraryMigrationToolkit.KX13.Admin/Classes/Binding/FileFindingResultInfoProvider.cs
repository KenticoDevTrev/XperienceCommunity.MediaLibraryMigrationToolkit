using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="FileFindingResultInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IFileFindingResultInfoProvider))]
    public partial class FileFindingResultInfoProvider : AbstractInfoProvider<FileFindingResultInfo, FileFindingResultInfoProvider>, IFileFindingResultInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileFindingResultInfoProvider"/> class.
        /// </summary>
        public FileFindingResultInfoProvider()
            : base(FileFindingResultInfo.TYPEINFO)
        {
        }
    }
}