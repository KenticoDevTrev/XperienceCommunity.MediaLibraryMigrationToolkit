using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using MediaLibraryMigrationToolkit;

[assembly: RegisterObjectType(typeof(FileLocationConfigurationInfo), FileLocationConfigurationInfo.OBJECT_TYPE)]

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Data container class for <see cref="FileLocationConfigurationInfo"/>.
    /// </summary>
    [Serializable]
    public partial class FileLocationConfigurationInfo : AbstractInfo<FileLocationConfigurationInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "medialibrarymigrationtoolkit.filelocationconfiguration";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FileLocationConfigurationInfoProvider), OBJECT_TYPE, "MediaLibraryMigrationToolkit.FileLocationConfiguration", "FileLocationConfigurationID", "FileLocationConfigurationLastModified", "FileLocationConfigurationGuid", null, null, null, null, null, null)
        {
            ModuleName = "MediaLibraryMigrationToolkit",
            TouchCacheDependencies = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, "MediaLibraryMigrationToolkit")
                }
            },
            ContainsMacros = false,
            LogEvents = false,
            LogIntegration = false,
        };


        /// <summary>
        /// File location configuration ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileLocationConfigurationID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileLocationConfigurationID"), 0);
            }
            set
            {
                SetValue("FileLocationConfigurationID", value);
            }
        }


        /// <summary>
        /// File location configuration guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FileLocationConfigurationGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("FileLocationConfigurationGuid"), Guid.Empty);
            }
            set
            {
                SetValue("FileLocationConfigurationGuid", value);
            }
        }


        /// <summary>
        /// File location configuration last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FileLocationConfigurationLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("FileLocationConfigurationLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileLocationConfigurationLastModified", value);
            }
        }


        /// <summary>
        /// Full Table Name of where to look for media file urls for conversion.
        /// </summary>
        [DatabaseField]
        public virtual string TableName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TableName"), String.Empty);
            }
            set
            {
                SetValue("TableName", value);
            }
        }


        /// <summary>
        /// Usually "dbo" but the schema for the table..
        /// </summary>
        [DatabaseField]
        public virtual string TableDBSchema
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TableDBSchema"), "dbo");
            }
            set
            {
                SetValue("TableDBSchema", value);
            }
        }


        /// <summary>
        /// Table identifier column.
        /// </summary>
        [DatabaseField]
        public virtual string TableIdentifierColumn
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TableIdentifierColumn"), String.Empty);
            }
            set
            {
                SetValue("TableIdentifierColumn", value);
            }
        }


        /// <summary>
        /// Column Name that will be scanned for media file URLs (both to detect files and for relative to permanent url swapping).
        /// </summary>
        [DatabaseField]
        public virtual string TableColumnName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("TableColumnName"), String.Empty);
            }
            set
            {
                SetValue("TableColumnName", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            FileLocationConfigurationInfoProvider.DeleteFileLocationConfigurationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            FileLocationConfigurationInfoProvider.SetFileLocationConfigurationInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected FileLocationConfigurationInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="FileLocationConfigurationInfo"/> class.
        /// </summary>
        public FileLocationConfigurationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FileLocationConfigurationInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FileLocationConfigurationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}