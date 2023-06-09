﻿using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using MediaLibraryMigrationToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public static class MediaMigrationFunctions
    {


        public static IEnumerable<MediaConversionObject> GetFileLocationConfigurations()
        {
            return GetFileLocationConfigurations(new int[] { });
        }

        /// <summary>
        /// Builds the File Location Configuration and all it's row IDs, this can take a while.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<MediaConversionObject> GetFileLocationConfigurations(IEnumerable<int> configurationIds)
        {
            var guidType = typeof(Guid);
            var keyToMediaConversionObject = new Dictionary<string, MediaConversionObject>();
            var _fileLocationConfigurationInfoProvider = Service.Resolve<IFileLocationConfigurationInfoProvider>();
            var query = _fileLocationConfigurationInfoProvider.Get();
            if (configurationIds.Any())
            {
                query.WhereIn(nameof(FileLocationConfigurationInfo.FileLocationConfigurationID), configurationIds.ToArray());
            }
            foreach (var configuration in query.TypedResult)
            {
                string tableName = SqlHelper.EscapeQuotes(configuration.TableName.Replace("[", "").Replace("]", "")).ToLower();
                string schema = SqlHelper.EscapeQuotes(configuration.TableDBSchema.Replace("[", "").Replace("]", "")).ToLower();
                string idColumn = SqlHelper.EscapeQuotes(configuration.TableIdentifierColumn.Replace("[", "").Replace("]", "")).ToLower();
                string column = SqlHelper.EscapeQuotes(configuration.TableColumnName.Replace("[", "").Replace("]", "")).ToLower();
                string key = $"{tableName}|{schema}".ToLower();
                if (!keyToMediaConversionObject.ContainsKey(key))
                {
                    keyToMediaConversionObject.Add(key, new MediaConversionObject(tableName, schema, idColumn));
                }
                var conversionObj = keyToMediaConversionObject[key];
                conversionObj.ColumnsToCheck.Add(column);
            }

            var mediaConversionObjects = keyToMediaConversionObject.Values.ToList();

            // Now fill all the items
            foreach (var mediaConversionObject in mediaConversionObjects)
            {
                string sql = mediaConversionObject.GetItemsSql();
                var items = ConnectionHelper.ExecuteQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery, false).Tables[0].Rows.Cast<DataRow>();
                foreach (DataRow dr in items)
                {
                    var newItem = new MediaConversionObjectItem()
                    {
                        ID = (int)dr[mediaConversionObject.RowIDColumn]
                    };
                    foreach (var column in mediaConversionObject.ColumnsToCheck)
                    {
                        if (dr[column] != null && dr[column] != DBNull.Value)
                        {
                            if (dr[column].GetType() == guidType)
                            {
                                newItem.ColumnToGuidValue.Add(column, (Guid)dr[column]);
                            }
                            else
                            {
                                newItem.ColumnToValue.Add(column, ValidationHelper.GetString(dr[column], string.Empty));
                            }
                        }
                        else
                        {
                            newItem.ColumnToValue.Add(column, null);
                        }
                    }
                    mediaConversionObject.Items.Add(newItem);
                }
            }
            return mediaConversionObjects;
        }

        /// <summary>
        /// Rebuilds the Attachment Tracking Table
        /// </summary>
        public static void UpdateAttachmentTracking()
        {
            string sql = @"
insert into MediaLibraryMigrationToolkit_AttachmentTracking
select
NEWID() as AttachmentTrackingGuid,
GETDATE() as AttachmentTrackingLastModified,
AttachmentID as AttachmentTrackingAttachmentID,
'/getattachment/'+cast(AttachmentGuid as nvarchar(50))+'/'+AttachmentName+AttachmentExtension as AttachmentPermanentUrl,
0 as AttachmentProcessed,
0 as AttachmentPreserved
from CMS_Attachment where AttachmentID not in (Select AttachmentTrackingAttachmentID from MediaLibraryMigrationToolkit_AttachmentTracking)

delete from MediaLibraryMigrationToolkit_AttachmentTracking where AttachmentTrackingAttachmentID not in (Select AttachmentID from CMS_Attachment)";
            ConnectionHelper.ExecuteNonQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery);
        }

        /// <summary>
        /// Updates the File Tracking Class with any new media files
        /// </summary>
        public static void UpdateMediaFileTracking()
        {
            string sql = @"

insert into MediaLibraryMigrationToolkit_FileTracking
select
NEWID() as FileTrackingGuid,
GETDATE() as FileTrackingLastModified,
FileID as FileTrackingMediaID,
FileTrackingOriginalUrl as FileTrackingOriginalUrl,
FilePermanentUrl as FileTrackingPermanentUrl,
0 as FileTrackingProcessed,
0 as FileTrackingPreserved
from (
SELECT FileID, Prefix+FilePath as FileTrackingOriginalUrl, FileGUID,  '/getmedia/'+cast(FileGuid as nvarchar(50))+'/'+FileName as FilePermanentUrl from (
select FileID, FileGuid, FileName+FileExtension as FileName,
case when NULLIF(COALESCE(GlobalSKFolder.KeyValue, SiteSKFolder.KeyValue), '') is null 
	then
		'/'+SiteName+'/media' 
	else
		'/'+COALESCE(GlobalSKFolder.KeyValue, SiteSKFolder.KeyValue) +
		case when COALESCE(GlobalSKSiteFolder.KeyValue, SiteSKSiteFolder.KeyValue) <> 'False' then '/'+Sitename else '' end	
	end as Prefix,
'/'+Media_Library.LibraryFolder+'/'+FilePath as FilePath
FROM [Media_File] 
inner join Media_Library on LibraryID = FileLibraryID 
left join CMS_Site S on LibrarySiteID = S.SiteID 
left join CMS_SettingsKey SiteSKFolder on SiteSKFolder.KeyName = 'CMSMediaLibrariesFolder' and SiteSKFolder.SiteID = S.SiteID
left join CMS_SettingsKey GlobalSKFolder on GlobalSKFolder.KeyName = 'CMSMediaLibrariesFolder' and GlobalSKFolder.SiteID is null
left join CMS_SettingsKey SiteSKSiteFolder on SiteSKSiteFolder.KeyName = 'CMSUseMediaLibrariesSiteFolder' and SiteSKSiteFolder.SiteID = S.SiteID
left join CMS_SettingsKey GlobalSKSiteFolder on GlobalSKSiteFolder.KeyName = 'CMSUseMediaLibrariesSiteFolder' and GlobalSKSiteFolder.SiteID is null
where FileID not in (select FileTrackingMediaID from MediaLibraryMigrationToolkit_FileTracking)
) combinedOne
) combined

delete from MediaLibraryMigrationToolkit_FileTracking where FileTrackingMediaID not in (Select FileID from Media_File)";
            ConnectionHelper.ExecuteNonQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery);
        }


        public static DataSet GetDataSetOfMediaResultsForDataExportHelper(MediaFindingResult findingResults)
        {
            // Create DataTabe of the results
            DataSet ds = new DataSet();
            DataTable dt = new DataTable("MediaFindingResults");
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("IsAttachment", typeof(bool)),
                new DataColumn("Occurrences", typeof(int)),
                new DataColumn("ID", typeof(int)),
                new DataColumn("GUID", typeof(string)),
                new DataColumn("Name", typeof(string)),
                new DataColumn("Path", typeof(string)),
                new DataColumn("CultureOrLibraryName", typeof(string)),
                new DataColumn("LastModified", typeof(DateTime))
            });

            foreach (var attachment in findingResults.AllAttachments.Values)
            {
                var dr = dt.NewRow();
                dr["IsAttachment"] = true;
                dr["Occurrences"] = attachment.TotalOccurrences;
                dr["ID"] = attachment.AttachmentID;
                dr["GUID"] = attachment.AttachmentGuid;
                dr["Name"] = attachment.AttachmentName;
                dr["Path"] = attachment.NodeAliasPath;
                dr["CultureOrLibraryName"] = attachment.DocumentCulture;
                dr["LastModified"] = attachment.LastModified;
                dt.Rows.Add(dr);
            }

            foreach (var mediaFile in findingResults.AllMediaFiles.Values)
            {
                var dr = dt.NewRow();
                dr["IsAttachment"] = false;
                dr["Occurrences"] = mediaFile.TotalOccurrences;
                dr["ID"] = mediaFile.MediaFileID;
                dr["GUID"] = mediaFile.MediaGuid;
                dr["Name"] = mediaFile.MediaName;
                dr["Path"] = mediaFile.MediaPath;
                dr["CultureOrLibraryName"] = mediaFile.LibraryName;
                dr["LastModified"] = mediaFile.LastModified;
                dt.Rows.Add(dr);
            }

            ds.Tables.Add(dt);

            return ds;
        }

        /// <summary>
        /// Adds any new Media Files to the Media File Clone table
        /// </summary>
        public static void UpdateMediaCloneTable()
        {
            string sql = @"
insert into [MediaLibraryMigrationToolkit_Media_File]
select
GETDATE() as [Media_FileLastModified],
FileID as [OldFileID]
           ,null as [NewFileID]
           ,[FileName]
           ,[FileTitle]
           ,[FileDescription]
           ,[FileExtension]
           ,[FileMimeType]
           ,[FileSize]
           ,[FileImageWidth]
           ,[FileImageHeight]
           ,[FileGUID]
           ,[FileLibraryID]
           ,[FileSiteID]
           ,[FileCreatedByUserID]
           ,[FileCreatedWhen]
           ,[FileModifiedByUserID]
           ,[FileModifiedWhen]
           ,[FileCustomData]
           ,[FilePath]
           ,0 as [FoundUsage]
           ,0 as [KeepFile]
           ,0 as [Processed]
		   ,0 As [UsageChecked]
		   from Media_File MF
		   where  MF.FileGUID not in (Select MLMT.FileGuid from MediaLibraryMigrationToolkit_Media_File MLMT)";
            ConnectionHelper.ExecuteNonQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery);

        }
    }
}
