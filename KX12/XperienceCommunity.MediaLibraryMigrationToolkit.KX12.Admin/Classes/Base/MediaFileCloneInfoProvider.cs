using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="Media_FileCloneInfo"/> management.
    /// </summary>
    public partial class Media_FileCloneInfoProvider : AbstractInfoProvider<Media_FileCloneInfo, Media_FileCloneInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="Media_FileCloneInfoProvider"/>.
        /// </summary>
        public Media_FileCloneInfoProvider()
            : base(Media_FileCloneInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="Media_FileCloneInfo"/> objects.
        /// </summary>
        public static ObjectQuery<Media_FileCloneInfo> GetMedia_FileClones()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="Media_FileCloneInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="Media_FileCloneInfo"/> ID.</param>
        public static Media_FileCloneInfo GetMedia_FileCloneInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="Media_FileCloneInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="Media_FileCloneInfo"/> to be set.</param>
        public static void SetMedia_FileCloneInfo(Media_FileCloneInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="Media_FileCloneInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="Media_FileCloneInfo"/> to be deleted.</param>
        public static void DeleteMedia_FileCloneInfo(Media_FileCloneInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="Media_FileCloneInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="Media_FileCloneInfo"/> ID.</param>
        public static void DeleteMedia_FileCloneInfo(int id)
        {
            Media_FileCloneInfo infoObj = GetMedia_FileCloneInfo(id);
            DeleteMedia_FileCloneInfo(infoObj);
        }
    }
}