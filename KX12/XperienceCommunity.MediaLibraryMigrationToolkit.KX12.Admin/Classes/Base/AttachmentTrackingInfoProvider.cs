using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Class providing <see cref="AttachmentTrackingInfo"/> management.
    /// </summary>
    public partial class AttachmentTrackingInfoProvider : AbstractInfoProvider<AttachmentTrackingInfo, AttachmentTrackingInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="AttachmentTrackingInfoProvider"/>.
        /// </summary>
        public AttachmentTrackingInfoProvider()
            : base(AttachmentTrackingInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="AttachmentTrackingInfo"/> objects.
        /// </summary>
        public static ObjectQuery<AttachmentTrackingInfo> GetAttachmentTrackings()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="AttachmentTrackingInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="AttachmentTrackingInfo"/> ID.</param>
        public static AttachmentTrackingInfo GetAttachmentTrackingInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="AttachmentTrackingInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="AttachmentTrackingInfo"/> to be set.</param>
        public static void SetAttachmentTrackingInfo(AttachmentTrackingInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="AttachmentTrackingInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="AttachmentTrackingInfo"/> to be deleted.</param>
        public static void DeleteAttachmentTrackingInfo(AttachmentTrackingInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="AttachmentTrackingInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="AttachmentTrackingInfo"/> ID.</param>
        public static void DeleteAttachmentTrackingInfo(int id)
        {
            AttachmentTrackingInfo infoObj = GetAttachmentTrackingInfo(id);
            DeleteAttachmentTrackingInfo(infoObj);
        }
    }
}