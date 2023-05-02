using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using MediaLibraryMigrationToolkit;

[assembly: RegisterObjectType(typeof(FileFindingResultInfo), FileFindingResultInfo.OBJECT_TYPE)]

namespace MediaLibraryMigrationToolkit
{
    /// <summary>
    /// Data container class for <see cref="FileFindingResultInfo"/>.
    /// </summary>
    [Serializable]
    public partial class FileFindingResultInfo : AbstractInfo<FileFindingResultInfo, IFileFindingResultInfoProvider>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "medialibrarymigrationtoolkit.filefindingresult";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(FileFindingResultInfoProvider), OBJECT_TYPE, "MediaLibraryMigrationToolkit.FileFindingResult", "FileFindingResultID", "FileFindingResultLastModified", "FileFindingResultGuid", null, null, null, null, null, null)
        {
            ModuleName = "MediaLibraryMigrationToolkit",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("FileFindingResultTableConfigurationID", "medialibrarymigrationtoolkit.filelocationconfiguration", ObjectDependencyEnum.Required),
            },
            LogEvents = false,
            LogIntegration = false,
        };


        /// <summary>
        /// File finding result ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileFindingResultID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileFindingResultID"), 0);
            }
            set
            {
                SetValue("FileFindingResultID", value);
            }
        }


        /// <summary>
        /// File finding result guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FileFindingResultGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("FileFindingResultGuid"), Guid.Empty);
            }
            set
            {
                SetValue("FileFindingResultGuid", value);
            }
        }


        /// <summary>
        /// File finding result last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime FileFindingResultLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("FileFindingResultLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("FileFindingResultLastModified", value);
            }
        }


        /// <summary>
        /// File finding result media guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid FileFindingResultMediaGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("FileFindingResultMediaGuid"), Guid.Empty);
            }
            set
            {
                SetValue("FileFindingResultMediaGuid", value);
            }
        }


        /// <summary>
        /// File finding result table configuration ID.
        /// </summary>
        [DatabaseField]
        public virtual int FileFindingResultTableConfigurationID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileFindingResultTableConfigurationID"), 0);
            }
            set
            {
                SetValue("FileFindingResultTableConfigurationID", value);
            }
        }


        /// <summary>
        /// How many items were found and updated.
        /// </summary>
        [DatabaseField]
        public virtual int FileFindingResultCount
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("FileFindingResultCount"), 0);
            }
            set
            {
                SetValue("FileFindingResultCount", value);
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
        protected FileFindingResultInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="FileFindingResultInfo"/> class.
        /// </summary>
        public FileFindingResultInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="FileFindingResultInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public FileFindingResultInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}