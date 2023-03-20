using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Declares members for <see cref="FileTrackingInfo"/> management.
    /// </summary>
    public partial interface IFileTrackingInfoProvider : IInfoProvider<FileTrackingInfo>, IInfoByIdProvider<FileTrackingInfo>
    {
    }
}