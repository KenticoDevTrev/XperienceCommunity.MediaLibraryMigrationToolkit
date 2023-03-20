using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using MediaLibraryMigrationToolkit;

[assembly: RegisterObjectType(typeof(AttachmentIDToGuidCloneInfo), AttachmentIDToGuidCloneInfo.OBJECT_TYPE)]

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Data container class for <see cref="AttachmentIDToGuidCloneInfo"/>.
    /// </summary>
    [Serializable]
    public partial class AttachmentIDToGuidCloneInfo : AbstractInfo<AttachmentIDToGuidCloneInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "medialibrarymigrationtoolkit.attachmentidtoguidclone";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(AttachmentIDToGuidCloneInfoProvider), OBJECT_TYPE, "MediaLibraryMigrationToolkit.AttachmentIDToGuidClone", "AttachmentIDToGuidCloneID", null, "AttachmentIDToGuidCloneAttachmentGuid", null, null, null, null, null, null)
        {
            ModuleName = "MediaLibraryMigrationToolkit",
            TouchCacheDependencies = true,
            ContainsMacros = false,
            LogEvents = false,
            LogIntegration = false,
        };


        /// <summary>
        /// Attachment ID to guid clone ID.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentIDToGuidCloneID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AttachmentIDToGuidCloneID"), 0);
            }
            set
            {
                SetValue("AttachmentIDToGuidCloneID", value);
            }
        }


        /// <summary>
        /// Attachment ID to guid clone attachment ID.
        /// </summary>
        [DatabaseField]
        public virtual int AttachmentIDToGuidCloneAttachmentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("AttachmentIDToGuidCloneAttachmentID"), 0);
            }
            set
            {
                SetValue("AttachmentIDToGuidCloneAttachmentID", value);
            }
        }


        /// <summary>
        /// The Attachment Guid, which will get translated to the media guid for lookup refs.
        /// </summary>
        [DatabaseField]
        public virtual Guid AttachmentIDToGuidCloneAttachmentGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("AttachmentIDToGuidCloneAttachmentGuid"), Guid.Empty);
            }
            set
            {
                SetValue("AttachmentIDToGuidCloneAttachmentGuid", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            AttachmentIDToGuidCloneInfoProvider.DeleteAttachmentIDToGuidCloneInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            AttachmentIDToGuidCloneInfoProvider.SetAttachmentIDToGuidCloneInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected AttachmentIDToGuidCloneInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="AttachmentIDToGuidCloneInfo"/> class.
        /// </summary>
        public AttachmentIDToGuidCloneInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="AttachmentIDToGuidCloneInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public AttachmentIDToGuidCloneInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}