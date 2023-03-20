using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="FileLocationConfigurationInfo"/> management.
    /// </summary>
    [ProviderInterface(typeof(IFileLocationConfigurationInfoProvider))]
    public partial class FileLocationConfigurationInfoProvider : AbstractInfoProvider<FileLocationConfigurationInfo, FileLocationConfigurationInfoProvider>, IFileLocationConfigurationInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLocationConfigurationInfoProvider"/> class.
        /// </summary>
        public FileLocationConfigurationInfoProvider()
            : base(FileLocationConfigurationInfo.TYPEINFO)
        {
        }
    }
}