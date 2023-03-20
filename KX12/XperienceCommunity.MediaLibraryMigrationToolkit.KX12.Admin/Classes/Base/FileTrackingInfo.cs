using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using MediaLibraryMigrationToolkit;

[assembly: RegisterObjectType(typeof(FileTrackingInfo), FileTrackingInfo.OBJECT_TYPE)]

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Data container class for <see cref="FileTrackingInfo"/>.
    /// </summary>
    [Serializable]
    public partial class FileTrackingInfo : AbstractInfo<FileTrackingInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "medialibrarymigrationtoolkit.filetracking";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FileTrackingInfoProvider), OBJECT_TYPE, "MediaLibraryMigrationToolkit.FileTracking", "FileTrackingID", "FileTrackingLastModified", "FileTrackingGuid", null, null, null, null, null, null)
        {
            ModuleName = "MediaLibraryMigrationToolkit",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("FileTrackingMediaID", "media.file", ObjectDependencyEnum.Required),
            },
            ContainsMacros = false,
            LogEvents = false,
            LogIntegration = false,
        };


        /// <summary>
        /// File tracking ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileTrackingID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileTrackingID"), 0);
            }
            set
            {
                SetValue("FileTrackingID", value);
            }
        }


        /// <summary>
        /// File tracking guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FileTrackingGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("FileTrackingGuid"), Guid.Empty);
            }
            set
            {
                SetValue("FileTrackingGuid", value);
            }
        }


        /// <summary>
        /// File tracking last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FileTrackingLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("FileTrackingLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileTrackingLastModified", value);
            }
        }


        /// <summary>
        /// File tracking media ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileTrackingMediaID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileTrackingMediaID"), 0);
            }
            set
            {
                SetValue("FileTrackingMediaID", value);
            }
        }


        /// <summary>
        /// Non permanent url.
        /// </summary>
        [DatabaseField]
        public virtual string FileTrackingOriginalUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileTrackingOriginalUrl"), String.Empty);
            }
            set
            {
                SetValue("FileTrackingOriginalUrl", value);
            }
        }


        /// <summary>
        /// File tracking permanent url.
        /// </summary>
        [DatabaseField]
        public virtual string FileTrackingPermanentUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileTrackingPermanentUrl"), String.Empty);
            }
            set
            {
                SetValue("FileTrackingPermanentUrl", value);
            }
        }


        /// <summary>
        /// If true, then all the table+column combinations have been combed and replaced..
        /// </summary>
        [DatabaseField]
        public virtual bool FileTrackingProcessed
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("FileTrackingProcessed"), false);
            }
            set
            {
                SetValue("FileTrackingProcessed", value);
            }
        }


        /// <summary>
        /// If checked, will keep and upload this file to blob storage even if not found in the tables (useful for files that are referenced in code or external systems).
        /// </summary>
        [DatabaseField]
        public virtual bool FileTrackingPreserved
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("FileTrackingPreserved"), false);
            }
            set
            {
                SetValue("FileTrackingPreserved", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            FileTrackingInfoProvider.DeleteFileTrackingInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            FileTrackingInfoProvider.SetFileTrackingInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected FileTrackingInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="FileTrackingInfo"/> class.
        /// </summary>
        public FileTrackingInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FileTrackingInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FileTrackingInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}