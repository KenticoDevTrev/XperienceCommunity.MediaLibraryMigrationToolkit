using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Declares members for <see cref="FileLocationConfigurationInfo"/> management.
    /// </summary>
    public partial interface IFileLocationConfigurationInfoProvider : IInfoProvider<FileLocationConfigurationInfo>, IInfoByIdProvider<FileLocationConfigurationInfo>
    {
    }
}