using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Declares members for <see cref="Media_FileCloneInfo"/> management.
    /// </summary>
    public partial interface IMedia_FileCloneInfoProvider : IInfoProvider<Media_FileCloneInfo>, IInfoByIdProvider<Media_FileCloneInfo>
    {
    }
}