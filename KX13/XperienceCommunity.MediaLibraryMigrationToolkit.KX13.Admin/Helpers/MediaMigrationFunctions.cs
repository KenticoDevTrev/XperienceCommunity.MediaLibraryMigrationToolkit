using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using MediaLibraryMigrationToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

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

            var query = Service.Resolve<IFileLocationConfigurationInfoProvider>().Get();
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
        /// Converts the results to a DataSet, you can use the following code to export it to excel:
        ///  var export = new DataExportHelper(resultDS)
        ///{
        ///    FileName = "AttachmentAndMediaUsage.csv"
        ///};
        ///export.ExportData(DataExportFormatEnum.CSV, Page.Response);
        /// </summary>
        /// <param name="findingResults"></param>
        /// <returns></returns>
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
            // Delete any items no longer applicable
            ConnectionHelper.ExecuteNonQuery($"delete from [MediaLibraryMigrationToolkit_Media_File] where FileGUID not in (select M.FileGuid from Media_File M)", new QueryDataParameters(), QueryTypeEnum.SQLQuery);

            string sql = @"
insert into [MediaLibraryMigrationToolkit_Media_File]
select
GETDATE() as [Media_FileLastModified],
FileID as [OldFileID]
           ,null as [NewFileID]
		   ,0 As [UsageChecked]
           ,0 as [FoundUsage]
           ,0 as [KeepFile]
           ,0 as [Processed]
           ,[FileName]
           ,[FileTitle]
           ,[FileDescription]
           ,[FileExtension]
           ,[FileMimeType]
		   ,[FilePath]
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
           ,NULL as [Error]
		   from Media_File MF
		   where  MF.FileGUID not in (Select MLMT.FileGuid from MediaLibraryMigrationToolkit_Media_File MLMT)";
            ConnectionHelper.ExecuteNonQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery);

            // Now run update command
            string updateSql = @"update [MediaLibraryMigrationToolkit_Media_File] set
      [Media_FileLastModified] = CMF.FileModifiedWhen
      ,[FileName] = CMF.FileName
      ,[FileTitle] = CMF.FileTitle
      ,[FileDescription] = CMF.FileDescription
      ,[FileExtension] = CMF.FileExtension
      ,[FileMimeType] = CMF.FileMimeType
      ,[FilePath] = CMF.FilePath
      ,[FileSize] = CMF.FileSize
      ,[FileImageWidth] = CMF.FileImageWidth
      ,[FileImageHeight] = CMF.FileImageHeight
      ,[FileModifiedByUserID] = CMF.FileModifiedByUserID
      ,[FileModifiedWhen] = CMF.FileModifiedWhen
      ,[FileCustomData] = CMF.FileCustomData
  FROM [MediaLibraryMigrationToolkit_Media_File] CMF
  inner join Media_File MF on MF.FileGUID = CMF.FileGUID
  where MF.FileModifiedWhen > CMF.Media_FileLastModified";
            ConnectionHelper.ExecuteNonQuery(updateSql, new QueryDataParameters(), QueryTypeEnum.SQLQuery);


        }

        public static int PurgeAttachments(MediaFindingResult mediaFindingResult, AttachmentPurgeSettings attachmentPurgeSettings)
        {
            var _attachmentInfoProvider = Service.Resolve<IAttachmentInfoProvider>();
            var _attachmentHistoryInfoProvider = Service.Resolve<IAttachmentHistoryInfoProvider>();
            List<Guid> referencedAttachments = new List<Guid>();
            List<string> excludedPageTypes = new List<string>();
            string whereCondition = "1=1";
            string whereConditionVersion = "1=1";
            int itemsDelete = 0;

            switch (attachmentPurgeSettings.PurgeType)
            {
                case AttachmentPurgeType.AllAttachments:
                    //
                    break;
                case AttachmentPurgeType.OnlyNonReferencedAttachmentsAndSpecifiedPageTypes:
                    referencedAttachments.AddRange(mediaFindingResult.AttachmentOccurrances.Where(y => y.Value > 0).Select(x => x.Key));
                    break;
            }
            if (attachmentPurgeSettings.PageTypesToSkip.Any())
            {
                excludedPageTypes.AddRange(attachmentPurgeSettings.PageTypesToSkip);
            }

            referencedAttachments.AddRange(attachmentPurgeSettings.SpecificAttachmentGuidsToSkip);

            // Build SQL
            if (referencedAttachments.Any())
            {
                whereCondition = SqlHelper.AddWhereCondition(whereCondition, $"AttachmentGUID not in ('{string.Join("','", referencedAttachments.Select(x => x.ToString()))}')");
                whereConditionVersion = SqlHelper.AddWhereCondition(whereCondition, $"AttachmentGUID not in ('{string.Join("','", referencedAttachments.Select(x => x.ToString()))}')");
            }
            if (excludedPageTypes.Any())
            {
                whereCondition = SqlHelper.AddWhereCondition(whereCondition, $"DocumentID not in (select DocumentID from View_CMS_Tree_Joined where Classname in ('{string.Join("','", excludedPageTypes.Select(x => SqlHelper.EscapeQuotes(x)))}'))");
                whereConditionVersion = SqlHelper.AddWhereCondition(whereCondition, $@"
DocumentID not in 
    (
        select DocumentID from CMS_VersionHistory
            inner join CMS_Class on ClassID = VersionClassID
            where ClassName in ('{string.Join("','", referencedAttachments.Select(x => x.ToString()))}')
        
        UNION ALL 
        
        select DocumentID from View_CMS_Tree_Joined where Classname in ('{string.Join("','", excludedPageTypes.Select(x => SqlHelper.EscapeQuotes(x)))}')
    )");
            }

            // Delete, will use the API so it kills the proper file as well, even though slower.
            var attachmentIDs = ConnectionHelper.ExecuteQuery($"select AttachmentID from CMS_Attachment where {whereCondition}", new QueryDataParameters(), QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x["AttachmentID"]).ToList();
            var attachmentVersionHistoryIDs = ConnectionHelper.ExecuteQuery($"select AttachmentHistoryID from CMS_AttachmentHistory where {whereConditionVersion}", new QueryDataParameters(), QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x["AttachmentID"]).ToList();
            var attachmentVersionHistories = _attachmentHistoryInfoProvider.Get().WhereIn(nameof(AttachmentHistoryInfo.AttachmentHistoryID), attachmentVersionHistoryIDs).GetEnumerableTypedResult();
            foreach (var attachmentVersionHistory in attachmentVersionHistories)
            {
                itemsDelete++;
                _attachmentHistoryInfoProvider.Delete(attachmentVersionHistory);
            }

            var attachments = _attachmentInfoProvider.Get().WhereIn(nameof(AttachmentInfo.AttachmentID), attachmentIDs).GetEnumerableTypedResult();
            foreach (var attachment in attachments)
            {
                itemsDelete++;
                _attachmentInfoProvider.Delete(attachment);
            }

            return itemsDelete;
        }

        public static int PurgeMediaFiles(MediaFindingResult mediaFindingResult, MediaPurgeSettings mediaPurgeSettings)
        {
            List<Guid> referencedMediaFiles = new List<Guid>();
            var _mediaFileInfoProvider = Service.Resolve<IMediaFileInfoProvider>();

            int itemsDelete = 0;

            if (mediaPurgeSettings.KeepMediaFilesReferenced)
            {
                referencedMediaFiles.AddRange(mediaFindingResult.MediaOccurrances.Where(x => x.Value > 0).Select(x => x.Key));
            }

            if (mediaPurgeSettings.KeepMediaFilesMarkedAsKeepFile)
            {
                referencedMediaFiles.AddRange(ConnectionHelper.ExecuteQuery(@"select [FileGUID] from [MediaLibraryMigrationToolkit_Media_File] where KeepFile = 1", new QueryDataParameters(), QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (Guid)x["FileGUID"]));
            }

            referencedMediaFiles.AddRange(mediaPurgeSettings.SpecificMediaFileGuidsToSkip);

            // get media IDs from the Guids to exclude
            var allMediaGuidToID = _mediaFileInfoProvider.Get().Columns(nameof(MediaFileInfo.FileID), nameof(MediaFileInfo.FileGUID))
                .TypedResult
                .GroupBy(x => x.FileGUID)
                .ToDictionary(key => key.Key, value => value.First().FileID);

            var mediaIDsSpecificallyExcluded = referencedMediaFiles.Where(x => allMediaGuidToID.ContainsKey(x)).Select(x => allMediaGuidToID[x]).Distinct();

            string whereCondition = "1=1";
            List<string> libraryPathExclusions = new List<string>();

            if (mediaPurgeSettings.MediaLibrariesWithOptionalPathsToSkip.Keys.Any())
            {
                foreach (string library in mediaPurgeSettings.MediaLibrariesWithOptionalPathsToSkip.Keys)
                {
                    if (mediaPurgeSettings.MediaLibrariesWithOptionalPathsToSkip[library].Any())
                    {
                        libraryPathExclusions.Add($"(select MF.FileID from Media_File MF inner join Media_Library ML on ML.LibraryID = MF.FileLibraryID where LibraryName = '{SqlHelper.EscapeQuotes(library)}' and ({string.Join(" OR ", mediaPurgeSettings.MediaLibrariesWithOptionalPathsToSkip[library].Select(x => $"FilePath like '" + SqlHelper.EscapeQuotes(x) + "%'"))}))");
                    }
                    else
                    {
                        libraryPathExclusions.Add($"(select MF.FileID from Media_File MF inner join Media_Library ML on ML.LibraryID = MF.FileLibraryID where LibraryName = '{SqlHelper.EscapeQuotes(library)}')");
                    }
                }

                // Add exclusion by the library and paths
                whereCondition = SqlHelper.AddWhereCondition(whereCondition, $"FileID not in ({libraryPathExclusions.Join(" UNION ALL ")})");
            }

            // Delete, will use the API so it kills the proper file as well, even though slower.
            var mediaIDs = ConnectionHelper.ExecuteQuery($"select FileID from Media_File where {whereCondition}", new QueryDataParameters(), QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x["FileID"]).ToList();

            // Remove any media IDs specifieid
            mediaIDs = mediaIDs.Except(mediaIDsSpecificallyExcluded).ToList();
            var medias = _mediaFileInfoProvider.Get().WhereEquals(nameof(MediaFileInfo.FileID), mediaIDs).GetEnumerableTypedResult();
            foreach (var media in medias)
            {
                itemsDelete++;
                _mediaFileInfoProvider.Delete(media);
            }

            // update the Media file clone table and remove ones cloned.
            ConnectionHelper.ExecuteNonQuery($"delete from [MediaLibraryMigrationToolkit_Media_File] where FileGUID not in (select M.FileGuid from Media_File M)", new QueryDataParameters(), QueryTypeEnum.SQLQuery);

            return itemsDelete;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void MigrateUnprocessedFilesToAzure(string tempFileFolder, int maxSiblingFiles = 100)
        {
            MigrateFilesToAzure(tempFileFolder, maxSiblingFiles, true, Array.Empty<Guid>());
        }

        public static void MigrateFilesToAzure(string tempFileFolder, int maxSiblingFiles = 100, IEnumerable<Guid> fileGuids = null)
        {
            MigrateFilesToAzure(tempFileFolder, maxSiblingFiles, false, fileGuids != null ? fileGuids : Array.Empty<Guid>());
        }

        private static void MigrateFilesToAzure(string tempFileFolder, int maxSiblingFiles = 100, bool allUnprocessed = false, IEnumerable<Guid> fileGuids = null)
        {
            UpdateMediaCloneTable();
            var dictionary = GetMediaTrackingDictionaries(true);
            var movePath = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").Trim('/') + tempFileFolder.Trim('~').TrimEnd('/');
            if (!Directory.Exists(movePath))
            {
                CMS.IO.DirectoryHelper.EnsureDiskPath(movePath.Replace("/", "\\"), AppDomain.CurrentDomain.BaseDirectory);
                CMS.IO.DirectoryHelper.CreateDirectory(movePath.Replace("/", "\\"));
            }
            if (!Directory.Exists(movePath))
            {
                throw new Exception("Could not create temp directory, ensure your app pool has proper permissions.");
            }

            var query = Service.Resolve<IMedia_FileCloneInfoProvider>().Get();
            if (allUnprocessed)
            {
                query.WhereEquals(nameof(Media_FileCloneInfo.Processed), false);
            }
            if (fileGuids.Any())
            {
                query.WhereIn(nameof(Media_FileCloneInfo.FileGUID), fileGuids.ToArray());
            }
            var fileClones = query.TypedResult;
            foreach (var fileClone in fileClones)
            {
                if (dictionary.MediaGuidToRelativeFilePath.ContainsKey(fileClone.FileGUID))
                {
                    MigrateFileToAzureInternal(fileClone, dictionary.MediaGuidToRelativeFilePath[fileClone.FileGUID], movePath, maxSiblingFiles);
                }

            }

        }

        private static bool MigrateFileToAzureInternal(Media_FileCloneInfo fileCloneData, string fileRelativePath, string movePath, int maxSiblingFiles)
        {
            var _mediaFileInfoProvider = Service.Resolve<IMediaFileInfoProvider>();
            var _media_FileCloneInfoProvider = Service.Resolve<IMedia_FileCloneInfoProvider>();
            // move file
            string path = fileRelativePath.Trim('~').TrimEnd('/');
            // Relative Path
            if (path.StartsWith("/"))
            {
                path = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").Trim('/') + path;
            }

            if (File.Exists(path))
            {
                // Copy file so it won't be lost on deleting
                var oldFile = new FileInfo(path);
                // Delete if a file exists there.
                if (File.Exists(movePath + "/" + oldFile.Name))
                {
                    File.Delete(movePath + "/" + oldFile.Name);
                }
                oldFile.CopyTo(movePath + "/" + oldFile.Name);
                var file = new FileInfo(movePath + "/" + oldFile.Name);
                var oldFileFullPath = oldFile.FullName;

                // Get the new path, if max siblings is reached then starts adding to _# sub folders
                var newPathSplit = fileCloneData.FilePath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var newPath = (newPathSplit.Length > 0 ? newPathSplit.Take(newPathSplit.Length - 1).Select(x => x.ToLower().Replace(" ", "-")).Join("/") : "").Trim('/');
                var fileName = newPathSplit[newPathSplit.Length - 1].ToLower().Replace(" ", "-");



                var basePath = newPath;
                int subFolderCount = 0;
                int siblingCount = 0;
                do
                {
                    var siblingSql = $@"
SELECT count(*) as siblings
  FROM [Media_File] MF
  inner join MediaLibraryMigrationToolkit_Media_File CMF on MF.FileGUID = CMF.FileGUID
  where Processed = 1 and CMF.FileLibraryID = {fileCloneData.FileLibraryID} and REPLACE(REPLACE(CMF.FilePath, '/'+CMF.FileName+CMF.FileExtension, ''), CMF.FileName+CMF.FileExtension, '') = '{newPath}'
  ";

                    siblingCount = (int)ConnectionHelper.ExecuteQuery(siblingSql, new QueryDataParameters(), QueryTypeEnum.SQLQuery).Tables[0].Rows[0]["siblings"];
                    if (siblingCount >= maxSiblingFiles)
                    {
                        subFolderCount++;
                        newPath = $"{basePath}/_{subFolderCount}";
                    }
                } while (siblingCount >= maxSiblingFiles);

                // Upload again, so it will end up in azure
                var newFile = new MediaFileInfo(file.FullName, fileCloneData.FileLibraryID);
                var fileNameArray = fileName.Split('.');
                var filePart = fileNameArray.Take(fileNameArray.Length - 1).Join(".");
                var fileExtension = fileNameArray.Length > 1 ? "." + fileNameArray[fileNameArray.Length - 1] : "";
                // Update path and everything to be lowercase and no spaces
                newFile.FileName = filePart;
                newFile.FilePath = (!string.IsNullOrWhiteSpace(newPath) ? newPath + "/" + filePart : filePart) + fileExtension;

                newFile.FileExtension = "." + fileNameArray[fileNameArray.Length - 1];
                newFile.FileGUID = fileCloneData.FileGUID;
                newFile.SetValue("FileImageWidth", fileCloneData.GetValue("FileImageWidth"));
                newFile.SetValue("FileImageHeight", fileCloneData.GetValue("FileImageHeight"));
                newFile.SetValue("FileTitle", fileCloneData.GetValue("FileTitle"));
                newFile.SetValue("FileDescription", fileCloneData.GetValue("FileDescription"));
                newFile.SetValue("FileCreatedByUserID", fileCloneData.GetValue("FileCreatedByUserID"));
                newFile.SetValue("FileCreatedWhen", fileCloneData.GetValue("FileCreatedWhen"));
                newFile.SetValue("FileModifiedByUserID", fileCloneData.GetValue("FileModifiedByUserID"));
                newFile.SetValue("FileModifiedWhen", fileCloneData.GetValue("FileModifiedWhen"));
                newFile.SetValue("FileCustomData", fileCloneData.GetValue("FileCustomData"));

                // Delete current media
#pragma warning disable CS0618 // Type or member is obsolete
                MediaFileInfoProvider.DeleteMediaFileInfo(fileCloneData.OldFileID);
#pragma warning restore CS0618 // Type or member is obsolete
                try
                {
                    // insert the new one
                    _mediaFileInfoProvider.Set(newFile);
                }
                catch (Exception ex)
                {
                    // restore the media file data and then throw normal exception.
                    try { 
                        file.CopyTo(oldFileFullPath);
                    }
                    catch (IOException)
                    {
                        // already there
                    }
                    string restoreSql = $@"INSERT INTO [dbo].[Media_File]
           ([FileName]
           ,[FileTitle]
           ,[FileDescription]
           ,[FileExtension]
           ,[FileMimeType]
           ,[FilePath]
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
           ,[FileCustomData])
     VALUES
           (@FileName
           ,@FileTitle
           ,@FileDescription
           ,@FileExtension
           ,@FileMimeType
           ,@FilePath
           ,@FileSize
           ,@FileImageWidth
           ,@FileImageHeight
           ,@FileGUID
           ,@FileLibraryID
           ,@FileSiteID
           ,@FileCreatedByUserID
           ,@FileCreatedWhen
           ,@FileModifiedByUserID
           ,@FileModifiedWhen
           ,@FileCustomData)";
                    var queryParams = new QueryDataParameters()
                    {
                        {"@FileName", fileCloneData.GetValue("FileName") },
                        {"@FileTitle", fileCloneData.GetValue("FileTitle") },
                        {"@FileDescription", fileCloneData.GetValue("FileDescription") },
                        {"@FileExtension", fileCloneData.GetValue("FileExtension") },
                        {"@FileMimeType", fileCloneData.GetValue("FileMimeType") },
                        {"@FilePath", fileCloneData.GetValue("FilePath") },
                        {"@FileSize", fileCloneData.GetValue("FileSize") },
                        {"@FileImageWidth", fileCloneData.GetValue("FileImageWidth") },
                        {"@FileImageHeight", fileCloneData.GetValue("FileImageHeight") },
                        {"@FileGUID", fileCloneData.GetValue("FileGUID") },
                        {"@FileLibraryID", fileCloneData.GetValue("FileLibraryID") },
                        {"@FileSiteID", fileCloneData.GetValue("FileSiteID") },
                        {"@FileCreatedByUserID", fileCloneData.GetValue("FileCreatedByUserID") },
                        {"@FileCreatedWhen", fileCloneData.GetValue("FileCreatedWhen") },
                        {"@FileModifiedByUserID", fileCloneData.GetValue("FileModifiedByUserID") },
                        {"@FileModifiedWhen", fileCloneData.GetValue("FileModifiedWhen") },
                        {"@FileCustomData", fileCloneData.GetValue("FileCustomData") }

                    };
                    ConnectionHelper.ExecuteNonQuery(restoreSql, queryParams, QueryTypeEnum.SQLQuery);

                    fileCloneData.Error = $"{ex.GetType()}: {ex.Message}";
                    _media_FileCloneInfoProvider.Set(fileCloneData);
                    return false;
                }

                // Update tracking table
                fileCloneData.SetValue(nameof(Media_FileCloneInfo.Error), null);
                fileCloneData.FileName = newFile.FileName;
                fileCloneData.FilePath = newFile.FilePath;
                fileCloneData.FileExtension = newFile.FileExtension;
                fileCloneData.FileTitle = newFile.FileTitle;
                fileCloneData.Processed = true;
                _media_FileCloneInfoProvider.Set(fileCloneData);

                // Ran into this error...so give it 5 attempts before just continuing.
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        file.Delete();
                        i = 5;
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }

                return true;
            }
            else
            {
                fileCloneData.Error = $"Media file at path [{path}] not found, could not clone media file.";
                _media_FileCloneInfoProvider.Set(fileCloneData);
                return false;
            }
        }

        public static MediaTrackingDictionaries GetMediaTrackingDictionaries(bool lowercaseUrls)
        {
            var _fileLocationConfigurationInfoProvider = Service.Resolve<IFileLocationConfigurationInfoProvider>();
            var _attachmentInfoProvider = Service.Resolve<IAttachmentInfoProvider>();
            string sql = @"

SELECT Prefix, Prefix+FilePath as FileFullPath, FileGUID, FileName from (
select 
case when NULLIF(COALESCE(GlobalSKFolder.KeyValue, SiteSKFolder.KeyValue), '') is null 
	then
		'/'+SiteName+'/media' 
	else
		'/'+COALESCE(GlobalSKFolder.KeyValue, SiteSKFolder.KeyValue) +
		case when COALESCE(GlobalSKSiteFolder.KeyValue, SiteSKSiteFolder.KeyValue) <> 'False' then '/'+Sitename else '' end	
	end as Prefix,
'/'+Media_Library.LibraryFolder+'/'+FilePath as FilePath,
FileGUID, FileName+FileExtension as FileName
FROM [Media_File] 
inner join Media_Library on LibraryID = FileLibraryID 
left join CMS_Site S on LibrarySiteID = S.SiteID 
left join CMS_SettingsKey SiteSKFolder on SiteSKFolder.KeyName = 'CMSMediaLibrariesFolder' and SiteSKFolder.SiteID = S.SiteID
left join CMS_SettingsKey GlobalSKFolder on GlobalSKFolder.KeyName = 'CMSMediaLibrariesFolder' and GlobalSKFolder.SiteID is null
left join CMS_SettingsKey SiteSKSiteFolder on SiteSKSiteFolder.KeyName = 'CMSUseMediaLibrariesSiteFolder' and SiteSKSiteFolder.SiteID = S.SiteID
left join CMS_SettingsKey GlobalSKSiteFolder on GlobalSKSiteFolder.KeyName = 'CMSUseMediaLibrariesSiteFolder' and GlobalSKSiteFolder.SiteID is null
) combined
";
            var rows = ConnectionHelper.ExecuteQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery, false).Tables[0].Rows.Cast<DataRow>();

            var fullPathDictionary = new Dictionary<string, Guid>();
            var fileGuidToRelativeFilePath = new Dictionary<Guid, string>();
            var encodedPathDictionary = new Dictionary<string, Guid>();
            var fileGuidToNewUrl = new Dictionary<Guid, string>();
            var allMediaPrefixes = new Dictionary<string, bool>();
            foreach (var row in rows)
            {
                string path = ValidationHelper.GetString(row["FileFullPath"], "").ToLower();

                // Encoding is...hard.  Spaces to %20, then UrlEncode to handle foreign characters, then undo the encoding on the / and revert %2520 back to %20
                string encodedPath = HttpUtility.UrlEncode(path.Replace(" ", "%20")).Replace("%2520", "%20").Replace("%2f", "/").ToLower();
                string prefix = ValidationHelper.GetString(row["Prefix"], "").ToLower();
                Guid mediaFileGuid = ValidationHelper.GetGuid(row["FileGuid"], Guid.Empty);
                string newUrl = $"/getmedia/{mediaFileGuid}/{ValidationHelper.GetString(row["FileName"], string.Empty)}";
                if (lowercaseUrls)
                {
                    newUrl = newUrl.ToLower();
                }
                if (mediaFileGuid != Guid.Empty && !string.IsNullOrWhiteSpace(path))
                {
                    if (!allMediaPrefixes.ContainsKey(prefix))
                    {
                        allMediaPrefixes.Add(prefix, true);
                    }
                    if (!fullPathDictionary.ContainsKey(path))
                    {
                        fullPathDictionary.Add(path, mediaFileGuid);
                    }
                    if (!fileGuidToRelativeFilePath.ContainsKey(mediaFileGuid))
                    {
                        fileGuidToRelativeFilePath.Add(mediaFileGuid, path);
                    }
                    if (!encodedPathDictionary.ContainsKey(encodedPath))
                    {
                        encodedPathDictionary.Add(encodedPath, mediaFileGuid);
                    }
                    if (!fileGuidToNewUrl.ContainsKey(mediaFileGuid))
                    {
                        fileGuidToNewUrl.Add(mediaFileGuid, newUrl);
                    }
                }
            }

            // Get configurations next
            var allConfigurations = _fileLocationConfigurationInfoProvider.Get().TypedResult.GroupBy(x => $"{x.TableName.Replace("[", "")}|{x.TableDBSchema.Replace("[", "")}|{x.TableColumnName.Replace("[", "")}".ToLower())
                .ToDictionary(key => key.Key, value => value.First().FileLocationConfigurationID);

            var allAttachmentGuids = _attachmentInfoProvider.Get()
                .Columns(nameof(AttachmentInfo.AttachmentGUID))
                .TypedResult.GroupBy(x => x.AttachmentGUID)
                .ToDictionary(key => key.Key, value => true);

            // Now get attachment by the other /getattachment/node/alias/path/attachment.name
            var attachmentAltSql = @"select REPLACE('/getattachment'+NodeAliasPath+'/'+AttachmentName, '//', '/') as LookupPath, AttachmentGUID
from CMS_Attachment
inner join CMS_Document on DocumentID = AttachmentDocumentID
inner join CMS_Tree on NodeID = DocumentNodeID";
            var attachmentRows = ConnectionHelper.ExecuteQuery(attachmentAltSql, new QueryDataParameters(), QueryTypeEnum.SQLQuery, false).Tables[0].Rows.Cast<DataRow>();
            Dictionary<string, Guid> attachmentNodePathToGuid = new Dictionary<string, Guid>();
            Dictionary<string, Guid> attachmentNodePathEncodedToGuid = new Dictionary<string, Guid>();
            foreach (var row in attachmentRows)
            {
                string path = ValidationHelper.GetString(row["LookupPath"], "").ToLower();

                // Encoding is...hard.  Spaces to %20, then UrlEncode to handle foreign characters, then undo the encoding on the / and revert %2520 back to %20
                string encodedPath = HttpUtility.UrlEncode(path.Replace(" ", "%20")).Replace("%2520", "%20").Replace("%2f", "/").ToLower();
                Guid attachmentGuid = ValidationHelper.GetGuid(row["AttachmentGUID"], Guid.Empty);
                if (!attachmentNodePathToGuid.ContainsKey(path))
                {
                    attachmentNodePathToGuid.Add(path, attachmentGuid);
                }
                if (!attachmentNodePathEncodedToGuid.ContainsKey(encodedPath))
                {
                    attachmentNodePathEncodedToGuid.Add(encodedPath, attachmentGuid);
                }
            }


            return new MediaTrackingDictionaries(fullPathDictionary, fileGuidToRelativeFilePath, encodedPathDictionary, fileGuidToNewUrl, allMediaPrefixes.Keys.ToList(), allAttachmentGuids, attachmentNodePathToGuid, attachmentNodePathEncodedToGuid, allConfigurations);
        }

        /// <summary>
        /// Deletes any duplicate media files by Guid, this sometimes occurs if ran multiple times and errors occur between them.
        /// </summary>
        public static void ClearDuplicateMediaFiles()
        {
            var sql = @"delete from Media_File where FileID in (
select allItems.FileID from (
select FileID, ROW_NUMBER() over (partition by FileGuid order by FileID asc) as [count] from Media_File
) allItems where Count > 1)";
            ConnectionHelper.ExecuteNonQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery);
        }

    }




    public class AttachmentPurgeSettings
    {
        public AttachmentPurgeType PurgeType { get; set; } = AttachmentPurgeType.OnlyNonReferencedAttachmentsAndSpecifiedPageTypes;
        public IEnumerable<Guid> SpecificAttachmentGuidsToSkip { get; set; } = Array.Empty<Guid>();
        public IEnumerable<string> PageTypesToSkip { get; set; } = Array.Empty<string>();
    }

    public class MediaPurgeSettings
    {
        /// <summary>
        /// Will exclude any files from the MediaLibraryMigrationToolkit_Media_File where KeepFile is true.
        /// </summary>
        public bool KeepMediaFilesMarkedAsKeepFile { get; set; } = true;
        public bool KeepMediaFilesReferenced { get; set; } = true;

        public IEnumerable<Guid> SpecificMediaFileGuidsToSkip { get; set; } = Array.Empty<Guid>();

        /// <summary>
        /// What items to skip. 
        /// 
        /// If it's a Media library with no Paths, then the entire media library will be skipped.
        /// If it's a Media Library with paths, then only the paths specified within that library will be skipped.
        /// </summary>
        public Dictionary<string, List<string>> MediaLibrariesWithOptionalPathsToSkip { get; set; }

    }


    public enum AttachmentPurgeType
    {
        AllAttachments,
        OnlyNonReferencedAttachmentsAndSpecifiedPageTypes
    }
}