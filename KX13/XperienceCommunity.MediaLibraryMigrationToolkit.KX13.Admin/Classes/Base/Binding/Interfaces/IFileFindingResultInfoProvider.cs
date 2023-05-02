using CMS.DataEngine;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Declares members for <see cref="FileFindingResultInfo"/> management.
    /// </summary>
    public partial interface IFileFindingResultInfoProvider : IInfoProvider<FileFindingResultInfo>, IInfoByIdProvider<FileFindingResultInfo>
    {
    }
}