using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="FileLocationConfigurationInfo"/> management.
    /// </summary>
    public partial class FileLocationConfigurationInfoProvider : AbstractInfoProvider<FileLocationConfigurationInfo, FileLocationConfigurationInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="FileLocationConfigurationInfoProvider"/>.
        /// </summary>
        public FileLocationConfigurationInfoProvider()
            : base(FileLocationConfigurationInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="FileLocationConfigurationInfo"/> objects.
        /// </summary>
        public static ObjectQuery<FileLocationConfigurationInfo> GetFileLocationConfigurations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="FileLocationConfigurationInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileLocationConfigurationInfo"/> ID.</param>
        public static FileLocationConfigurationInfo GetFileLocationConfigurationInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="FileLocationConfigurationInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileLocationConfigurationInfo"/> to be set.</param>
        public static void SetFileLocationConfigurationInfo(FileLocationConfigurationInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="FileLocationConfigurationInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileLocationConfigurationInfo"/> to be deleted.</param>
        public static void DeleteFileLocationConfigurationInfo(FileLocationConfigurationInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="FileLocationConfigurationInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileLocationConfigurationInfo"/> ID.</param>
        public static void DeleteFileLocationConfigurationInfo(int id)
        {
            FileLocationConfigurationInfo infoObj = GetFileLocationConfigurationInfo(id);
            DeleteFileLocationConfigurationInfo(infoObj);
        }
    }
}