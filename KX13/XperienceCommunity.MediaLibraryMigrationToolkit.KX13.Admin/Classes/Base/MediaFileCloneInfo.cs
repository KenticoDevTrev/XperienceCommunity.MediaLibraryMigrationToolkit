﻿using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using MediaLibraryMigrationToolkit;

[assembly: RegisterObjectType(typeof(Media_FileCloneInfo), Media_FileCloneInfo.OBJECT_TYPE)]

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Data container class for <see cref="Media_FileCloneInfo"/>.
    /// </summary>
    [Serializable]
    public partial class Media_FileCloneInfo : AbstractInfo<Media_FileCloneInfo, IMedia_FileCloneInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "medialibrarymigrationtoolkit.media_fileclone";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(Media_FileCloneInfoProvider), OBJECT_TYPE, "MediaLibraryMigrationToolkit.Media_FileClone", "Media_FileID", "Media_FileLastModified", "FileGUID", null, "FileName", null, "FileSiteID", null, null)
        {
            ModuleName = "MediaLibraryMigrationToolkit",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("FileLibraryID", "media.library", ObjectDependencyEnum.Required),
                new ObjectDependency("FileSiteID", "cms.site", ObjectDependencyEnum.Required),
                new ObjectDependency("FileCreatedByUserID", "cms.user", ObjectDependencyEnum.NotRequired),
                new ObjectDependency("FileModifiedByUserID", "cms.user", ObjectDependencyEnum.NotRequired),
            },
            LogEvents = false,
            LogIntegration = false
        };


        /// <summary>
        /// Media file ID.
        /// </summary>
        [DatabaseField]
        public virtual int Media_FileID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("Media_FileID"), 0);
            }
            set
            {
                SetValue("Media_FileID", value);
            }
        }


        /// <summary>
        /// Media file last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime Media_FileLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("Media_FileLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("Media_FileLastModified", value);
            }
        }


        /// <summary>
        /// Old file ID.
        /// </summary>
        [DatabaseField]
        public virtual int OldFileID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("OldFileID"), 0);
            }
            set
            {
                SetValue("OldFileID", value);
            }
        }


        /// <summary>
        /// New file ID.
        /// </summary>
        [DatabaseField]
        public virtual int NewFileID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NewFileID"), 0);
            }
            set
            {
                SetValue("NewFileID", value, 0);
            }
        }


        /// <summary>
        /// If true, then a scan of the database was performed and this was not found.  .
        /// </summary>
        [DatabaseField]
        public virtual bool UsageChecked
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("UsageChecked"), false);
            }
            set
            {
                SetValue("UsageChecked", value);
            }
        }


        /// <summary>
        /// If true, then somewhere there is a reference to this media file's guid..
        /// </summary>
        [DatabaseField]
        public virtual bool FoundUsage
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("FoundUsage"), false);
            }
            set
            {
                SetValue("FoundUsage", value);
            }
        }


        /// <summary>
        /// If true, this file should be kept (and moved to blob storage if applicable).
        /// </summary>
        [DatabaseField]
        public virtual bool KeepFile
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("KeepFile"), false);
            }
            set
            {
                SetValue("KeepFile", value);
            }
        }


        /// <summary>
        /// If true, this file has been migrated to blob storage..
        /// </summary>
        [DatabaseField]
        public virtual bool Processed
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("Processed"), false);
            }
            set
            {
                SetValue("Processed", value);
            }
        }




        /// <summary>
        /// If true, this file has been migrated to blob storage..
        /// </summary>
        [DatabaseField]
        public virtual string Error
        {
            get
            {
                return ValidationHelper.GetString(GetValue("Error"), String.Empty);
            }
            set
            {
                SetValue("Error", value);
            }
        }


        /// <summary>
        /// File name.
        /// </summary>
        [DatabaseField]
        public virtual string FileName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileName"), String.Empty);
            }
            set
            {
                SetValue("FileName", value);
            }
        }


        /// <summary>
        /// File title.
        /// </summary>
        [DatabaseField]
        public virtual string FileTitle
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileTitle"), String.Empty);
            }
            set
            {
                SetValue("FileTitle", value);
            }
        }


        /// <summary>
        /// File description.
        /// </summary>
        [DatabaseField]
        public virtual string FileDescription
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileDescription"), String.Empty);
            }
            set
            {
                SetValue("FileDescription", value);
            }
        }


        /// <summary>
        /// File extension.
        /// </summary>
        [DatabaseField]
        public virtual string FileExtension
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileExtension"), String.Empty);
            }
            set
            {
                SetValue("FileExtension", value);
            }
        }


        /// <summary>
        /// File mime type.
        /// </summary>
        [DatabaseField]
        public virtual string FileMimeType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileMimeType"), String.Empty);
            }
            set
            {
                SetValue("FileMimeType", value);
            }
        }


        /// <summary>
        /// File path.
        /// </summary>
        [DatabaseField]
        public virtual string FilePath
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FilePath"), String.Empty);
            }
            set
            {
                SetValue("FilePath", value);
            }
        }


        /// <summary>
        /// File size.
        /// </summary>
        [DatabaseField]
        public virtual long FileSize
        {
            get
            {
                return ValidationHelper.GetLong(GetValue("FileSize"), 0);
            }
            set
            {
                SetValue("FileSize", value);
            }
        }


        /// <summary>
        /// File image width.
        /// </summary>
        [DatabaseField]
        public virtual int FileImageWidth
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileImageWidth"), 0);
            }
            set
            {
                SetValue("FileImageWidth", value, 0);
            }
        }


        /// <summary>
        /// File image height.
        /// </summary>
        [DatabaseField]
        public virtual int FileImageHeight
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileImageHeight"), 0);
            }
            set
            {
                SetValue("FileImageHeight", value, 0);
            }
        }


        /// <summary>
        /// File GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid FileGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("FileGUID"), Guid.Empty);
            }
            set
            {
                SetValue("FileGUID", value);
            }
        }


        /// <summary>
        /// File library ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileLibraryID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileLibraryID"), 0);
            }
            set
            {
                SetValue("FileLibraryID", value);
            }
        }


        /// <summary>
        /// File site ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileSiteID"), 0);
            }
            set
            {
                SetValue("FileSiteID", value);
            }
        }


        /// <summary>
        /// File created by user ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileCreatedByUserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileCreatedByUserID"), 0);
            }
            set
            {
                SetValue("FileCreatedByUserID", value, 0);
            }
        }


        /// <summary>
        /// File created when.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FileCreatedWhen
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("FileCreatedWhen"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileCreatedWhen", value);
            }
        }


        /// <summary>
        /// File modified by user ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileModifiedByUserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileModifiedByUserID"), 0);
            }
            set
            {
                SetValue("FileModifiedByUserID", value, 0);
            }
        }


        /// <summary>
        /// File modified when.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FileModifiedWhen
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("FileModifiedWhen"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileModifiedWhen", value);
            }
        }


        /// <summary>
        /// File custom data.
        /// </summary>
        [DatabaseField]
        public virtual string FileCustomData
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FileCustomData"), String.Empty);
            }
            set
            {
                SetValue("FileCustomData", value, String.Empty);
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
        protected Media_FileCloneInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="Media_FileCloneInfo"/> class.
        /// </summary>
        public Media_FileCloneInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="Media_FileCloneInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public Media_FileCloneInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}