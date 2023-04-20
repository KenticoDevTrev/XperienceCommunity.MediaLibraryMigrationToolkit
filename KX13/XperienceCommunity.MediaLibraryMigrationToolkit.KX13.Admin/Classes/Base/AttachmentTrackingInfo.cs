using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using MediaLibraryMigrationToolkit;

[assembly: RegisterObjectType(typeof(AttachmentTrackingInfo), AttachmentTrackingInfo.OBJECT_TYPE)]

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Data container class for <see cref="AttachmentTrackingInfo"/>.
    /// </summary>
    [Serializable]
    public partial class AttachmentTrackingInfo : AbstractInfo<AttachmentTrackingInfo, IAttachmentTrackingInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "medialibrarymigrationtoolkit.attachmenttracking";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AttachmentTrackingInfoProvider), OBJECT_TYPE, "MediaLibraryMigrationToolkit.AttachmentTracking", "AttachmentTrackingID", "AttachmentTrackingLastModified", "AttachmentTrackingGuid", null, null, null, null, null, null)
        {
            ModuleName = "MediaLibraryMigrationToolkit",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("AttachmentTrackingAttachmentID", "cms.attachment", ObjectDependencyEnum.Required),
            },
        };


        /// <summary>
        /// Attachment tracking ID.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentTrackingID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AttachmentTrackingID"), 0);
            }
            set
            {
                SetValue("AttachmentTrackingID", value);
            }
        }


        /// <summary>
        /// Attachment tracking guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid AttachmentTrackingGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("AttachmentTrackingGuid"), Guid.Empty);
            }
            set
            {
                SetValue("AttachmentTrackingGuid", value);
            }
        }


        /// <summary>
        /// Attachment tracking last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime AttachmentTrackingLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("AttachmentTrackingLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AttachmentTrackingLastModified", value);
            }
        }


        /// <summary>
        /// Attachment tracking attachment ID.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentTrackingAttachmentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AttachmentTrackingAttachmentID"), 0);
            }
            set
            {
                SetValue("AttachmentTrackingAttachmentID", value);
            }
        }

        /// <summary>
        /// Attachment permanent url.
        /// </summary>
        [DatabaseField]
        public virtual string AttachmentPermanentUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("AttachmentPermanentUrl"), String.Empty);
            }
            set
            {
                SetValue("AttachmentPermanentUrl", value);
            }
        }


        /// <summary>
        /// If true, then the attachment was copied to the media library and the urls were updated..
        /// </summary>
        [DatabaseField]
        public virtual bool AttachmentProcessed
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AttachmentProcessed"), false);
            }
            set
            {
                SetValue("AttachmentProcessed", value);
            }
        }


        /// <summary>
        /// If checked, will convert this to the media library (and preserve it) so it will get uploaded to blob storage even if not found in the tables (useful for files that are referenced in code or external systems).
        /// </summary>
        [DatabaseField]
        public virtual bool AttachmentPreserved
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("AttachmentPreserved"), false);
            }
            set
            {
                SetValue("AttachmentPreserved", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected AttachmentTrackingInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="AttachmentTrackingInfo"/> class.
        /// </summary>
        public AttachmentTrackingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AttachmentTrackingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AttachmentTrackingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}