using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="FileFindingResultInfo"/> management.
    /// </summary>
    public partial class FileFindingResultInfoProvider : AbstractInfoProvider<FileFindingResultInfo, FileFindingResultInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="FileFindingResultInfoProvider"/>.
        /// </summary>
        public FileFindingResultInfoProvider()
            : base(FileFindingResultInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="FileFindingResultInfo"/> objects.
        /// </summary>
        public static ObjectQuery<FileFindingResultInfo> GetFileFindingResults()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="FileFindingResultInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileFindingResultInfo"/> ID.</param>
        public static FileFindingResultInfo GetFileFindingResultInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="FileFindingResultInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileFindingResultInfo"/> to be set.</param>
        public static void SetFileFindingResultInfo(FileFindingResultInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="FileFindingResultInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileFindingResultInfo"/> to be deleted.</param>
        public static void DeleteFileFindingResultInfo(FileFindingResultInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="FileFindingResultInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileFindingResultInfo"/> ID.</param>
        public static void DeleteFileFindingResultInfo(int id)
        {
            FileFindingResultInfo infoObj = GetFileFindingResultInfo(id);
            DeleteFileFindingResultInfo(infoObj);
        }
    }
}