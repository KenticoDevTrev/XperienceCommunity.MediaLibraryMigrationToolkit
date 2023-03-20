using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="AttachmentIDToGuidCloneInfo"/> management.
    /// </summary>
    public partial class AttachmentIDToGuidCloneInfoProvider : AbstractInfoProvider<AttachmentIDToGuidCloneInfo, AttachmentIDToGuidCloneInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="AttachmentIDToGuidCloneInfoProvider"/>.
        /// </summary>
        public AttachmentIDToGuidCloneInfoProvider()
            : base(AttachmentIDToGuidCloneInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="AttachmentIDToGuidCloneInfo"/> objects.
        /// </summary>
        public static ObjectQuery<AttachmentIDToGuidCloneInfo> GetAttachmentIDToGuidClones()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="AttachmentIDToGuidCloneInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="AttachmentIDToGuidCloneInfo"/> ID.</param>
        public static AttachmentIDToGuidCloneInfo GetAttachmentIDToGuidCloneInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="AttachmentIDToGuidCloneInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="AttachmentIDToGuidCloneInfo"/> to be set.</param>
        public static void SetAttachmentIDToGuidCloneInfo(AttachmentIDToGuidCloneInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="AttachmentIDToGuidCloneInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="AttachmentIDToGuidCloneInfo"/> to be deleted.</param>
        public static void DeleteAttachmentIDToGuidCloneInfo(AttachmentIDToGuidCloneInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="AttachmentIDToGuidCloneInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="AttachmentIDToGuidCloneInfo"/> ID.</param>
        public static void DeleteAttachmentIDToGuidCloneInfo(int id)
        {
            AttachmentIDToGuidCloneInfo infoObj = GetAttachmentIDToGuidCloneInfo(id);
            DeleteAttachmentIDToGuidCloneInfo(infoObj);
        }
    }
}