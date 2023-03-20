using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="FileTrackingInfo"/> management.
    /// </summary>
    public partial class FileTrackingInfoProvider : AbstractInfoProvider<FileTrackingInfo, FileTrackingInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="FileTrackingInfoProvider"/>.
        /// </summary>
        public FileTrackingInfoProvider()
            : base(FileTrackingInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="FileTrackingInfo"/> objects.
        /// </summary>
        public static ObjectQuery<FileTrackingInfo> GetFileTrackings()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="FileTrackingInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileTrackingInfo"/> ID.</param>
        public static FileTrackingInfo GetFileTrackingInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="FileTrackingInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileTrackingInfo"/> to be set.</param>
        public static void SetFileTrackingInfo(FileTrackingInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="FileTrackingInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileTrackingInfo"/> to be deleted.</param>
        public static void DeleteFileTrackingInfo(FileTrackingInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="FileTrackingInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileTrackingInfo"/> ID.</param>
        public static void DeleteFileTrackingInfo(int id)
        {
            FileTrackingInfo infoObj = GetFileTrackingInfo(id);
            DeleteFileTrackingInfo(infoObj);
        }
    }
}