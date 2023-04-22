using CMS.Base.Internal;
using CMS.Base.UploadExtensions;
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public static class StringExtensionMethods
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }

    public class MediaLibraryMigrationScanner
    {
        private readonly MediaTrackingDictionaries _trackingDictionaries;
        private readonly Regex _mediaRegex;
        private readonly Regex _attachmentRegex;
        private readonly Regex _getMediaRegex;
        private readonly Regex _guidRegex;

        private readonly IAttachmentInfoProvider _attachmentInfoProvider;
        private readonly MediaLibraryInfo _attachmentMediaLibrary;
        private readonly IMediaFileInfoProvider _mediaFileInfoProvider;
        private readonly MediaLibraryMigrationSettings _settings;
        public IEnumerable<MediaConversionObject> MediaConversionObjects { get; }
        private readonly IAttachmentIDToGuidCloneInfoProvider _attachmentGuidInfoProvider;

        public MediaLibraryMigrationScanner(MediaLibraryMigrationSettings settings, IEnumerable<MediaConversionObject> mediaConversionObjects)
        {
            _settings = settings;
            
            _trackingDictionaries = GetMediaTrackingDictionaries();

            // Build media regex based on all possible prefixes
            string allPrefixMatches = $"({_trackingDictionaries.AllMediaPrefixes.Select(x => x + "/").Join(")|(")})";
            // This regex will match any of the media library relative path that exists
            // It creates a Group 1 of the full Relative Url (without any query string / hash)
            // You can then loop through other groups to see if any start with ? or #, if so then that group contains the stuff after the URL if it had a hash or ?
            _mediaRegex = new Regex($@"(~?({allPrefixMatches})([^\?\#\n\r\<\|\^\}}\""\']*\.[^\?\#\n\r\<\|\^\}}\""\' ]*))((\?([^\?\n\r\<\|\^\}}\""\' ]*))|(\#([^\n\r\<\|\^\}}\""\' ]*))){{0,1}}(?=(\<|\||\^|\}}|\""|\'| |$))", RegexOptions.IgnoreCase);
            _attachmentRegex = new Regex(@"(~?((\/getattachment\/))([0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12})([^\?\#\n\r\<\|\^\}\""\'\ ]*\.{0,1}[^\?\#\n\r\<\|\^\}\""\' ]*))((\?([^\?\n\r\<\|\^\}\""\' ]*))|(\#([^\n\r\<\|\^\}\""\' ]*))){0,1}(?=(\<|\||\^|\}|\""|\'| |$))", RegexOptions.IgnoreCase);
            _getMediaRegex = new Regex(@"(~?((\/getmedia\/))([0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12})([^\?\#\n\r\<\|\^\}\""\'\ ]*\.{0,1}[^\?\#\n\r\<\|\^\}\""\' ]*))((\?([^\?\n\r\<\|\^\}\""\' ]*))|(\#([^\n\r\<\|\^\}\""\' ]*))){0,1}(?=(\<|\||\^|\}|\""|\'| |$))", RegexOptions.IgnoreCase);
            _guidRegex = new Regex(@"([0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12})", RegexOptions.IgnoreCase);

            _attachmentInfoProvider = CMS.Core.Service.Resolve<IAttachmentInfoProvider>();
            if (_settings.ConvertAttachments)
            {
                _attachmentMediaLibrary = CMS.Core.Service.Resolve<IMediaLibraryInfoProvider>().Get().WhereEquals(nameof(MediaLibraryInfo.LibraryName), _settings.AttachmentMediaLibraryName).FirstOrDefault();
                if (_attachmentMediaLibrary == null)
                {
                    throw new NotSupportedException($"Must include a valid Attachment Media Library if you wish to convert attachments.  None found for {_settings.AttachmentMediaLibraryName}");
                }
            }
            _mediaFileInfoProvider = CMS.Core.Service.Resolve<IMediaFileInfoProvider>();
            MediaConversionObjects = mediaConversionObjects;
            _attachmentGuidInfoProvider = CMS.Core.Service.Resolve<IAttachmentIDToGuidCloneInfoProvider>();


        }

        public int SaveMediaConversionObjects()
        {
            int itemsUpdated = 0;
            foreach (var mediaConversionObject in MediaConversionObjects)
            {
                foreach (var mediaConversionItem in mediaConversionObject.Items)
                {
                    if (mediaConversionItem.UpdateIfTouchedItem(mediaConversionObject.Table, mediaConversionObject.Schema, mediaConversionObject.RowIDColumn))
                    {
                        itemsUpdated++;
                    }
                }
            }
            return itemsUpdated;
        }

        public void SetFileFindingResults()
        {
            Dictionary<int, Dictionary<int, int>> configurationIDToTrackingIDToCount = new Dictionary<int, Dictionary<int, int>>();
            foreach (var mediaConversionObject in MediaConversionObjects)
            {
                foreach (var mediaConversionItem in mediaConversionObject.Items)
                {
                    string configurationKey = $"{mediaConversionObject.Table}|{mediaConversionObject.Schema}|{mediaConversionItem.ColumnToValue}".ToLower();
                    if (_trackingDictionaries.TableSchemaColumnKeyToConfigurationID.ContainsKey(configurationKey))
                    {
                        int configurationID = _trackingDictionaries.TableSchemaColumnKeyToConfigurationID[configurationKey];

                        var trackingAndCount = mediaConversionItem.MediaFilesFound.Union(mediaConversionItem.MediaFilesUpdated)
                            .GroupBy(x => x)
                            .ToDictionary(key => _trackingDictionaries.FileGuidToFileTrackingID.ContainsKey(key.Key) ? _trackingDictionaries.FileGuidToFileTrackingID[key.Key] : 0, value => value.Count());
                        configurationIDToTrackingIDToCount.Add(configurationID, trackingAndCount);
                    } else
                    {
                        throw new Exception($"Could not find table schema column key with vale {configurationKey}, something went wrong.");
                    }
                    
                }
            }

            List<string> sqlDelete = new List<string>();
            List<string> sqlInsert = new List<string>();
            // Now generate the SQL statements
            foreach(int configurationId in configurationIDToTrackingIDToCount.Keys)
            {
                var trackingToCount = configurationIDToTrackingIDToCount[configurationId];
                sqlDelete.AddRange(trackingToCount.Select(x => $"delete from MediaLibraryMigrationToolkit_FileFindingResult where FileTracking FileFindingResultFileTrackingID = {x.Key} and FileFindingResultTableConfigurationID = {configurationId}"));
                sqlInsert.AddRange(trackingToCount.Select(x => $"INSERT INTO [dbo].[MediaLibraryMigrationToolkit_FileFindingResult] ([FileFindingResultGuid],[FileFindingResultLastModified],[FileFindingResultFileTrackingID],[FileFindingResultTableConfigurationID],[FileFindingResultCount]) VALUES (NEWID(),GETDATE(),{x.Key},{configurationId},{x.Value})"));
            }

            // Runs statements
            ConnectionHelper.ExecuteNonQuery(string.Join("\n\r", sqlDelete), new QueryDataParameters(), QueryTypeEnum.SQLQuery, false);
            ConnectionHelper.ExecuteNonQuery(string.Join("\n\r", sqlInsert), new QueryDataParameters(), QueryTypeEnum.SQLQuery, false);
        }

        /// <summary>
        /// Scans all columns given and tabulates what media files were reference or not.  Useful after the RunReplacement to see which media files are not being used.
        /// </summary>
        /// <param name="ConversionObjects"></param>
        /// <returns></returns>
        public Dictionary<Guid, Dictionary<string, MediaFileObjectReference>> FindAllMediaFileGuidsAndCounts()
        {
            Dictionary<Guid, Dictionary<string, MediaFileObjectReference>> mediaGuidToObjectKeyToReferences = new Dictionary<Guid, Dictionary<string, MediaFileObjectReference>>();

            foreach (var conversionObject in MediaConversionObjects)
            {
                foreach (var conversionItem in conversionObject.Items)
                {
                    foreach (var columnKey in conversionItem.ColumnToValue.Keys)
                    {
                        // Skip if no values
                        if (string.IsNullOrWhiteSpace(conversionItem.ColumnToValue[columnKey]))
                        {
                            continue;
                        }

                        string referenceKey = $"{conversionObject.Table}|{conversionItem.ID}|{columnKey}".ToLower();
                        string value = conversionItem.ColumnToValue[columnKey];
                        var matches = _getMediaRegex.Matches(value);
                        foreach (Match match in matches)
                        {
                            if (match.Groups.Count > 1)
                            {
                                string path = match.Groups[1].Value;
                                string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value ?? string.Empty;
                                var mediaGroups = match.Groups.Cast<Group>().Where(x => Guid.TryParse(x.Value, out var guid)).Select(x => Guid.Parse(x.Value));
                                if (mediaGroups.Any())
                                {
                                    Guid mediaGuid = mediaGroups.First();
                                    if (!mediaGuidToObjectKeyToReferences.ContainsKey(mediaGuid))
                                    {
                                        mediaGuidToObjectKeyToReferences.Add(mediaGuid, new Dictionary<string, MediaFileObjectReference>());
                                    }
                                    var dictionary = mediaGuidToObjectKeyToReferences[mediaGuid];
                                    if (!dictionary.ContainsKey(referenceKey))
                                    {
                                        dictionary.Add(referenceKey, new MediaFileObjectReference(conversionObject.Table, conversionItem.ID, columnKey));
                                    } else
                                    {
                                        dictionary[referenceKey].Occurrences++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return mediaGuidToObjectKeyToReferences;
        }


        /// <summary>
        /// Replaces all Non /getmedia URLs (and possibly /getattachment) with the proper /getmedia url, this does NOT run the database updates though.
        /// </summary>
        /// <param name="allRowValues"></param>
        /// <returns></returns>
        public void RunReplacement()
        {
            foreach (var conversionObject in MediaConversionObjects)
            {
                foreach (var conversionItem in conversionObject.Items)
                {
                    foreach (var columnKey in conversionItem.ColumnToValue.Keys)
                    {
                        // Skip if no values
                        if (string.IsNullOrWhiteSpace(conversionItem.ColumnToValue[columnKey]))
                        {
                            continue;
                        }

                        string value = conversionItem.ColumnToValue[columnKey];
                        int itemsTouched = 0;

                        // Fill normal Media Guids
                        var matches = _getMediaRegex.Matches(value);
                        foreach (Match match in matches)
                        {
                            if (match.Groups.Count > 1)
                            {
                                string path = match.Groups[1].Value;
                                string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value ?? string.Empty;
                                var mediaGroups = match.Groups.Cast<Group>().Where(x => Guid.TryParse(x.Value, out var guid)).Select(x => Guid.Parse(x.Value));
                                if (mediaGroups.Any())
                                {
                                    Guid mediaGuid = mediaGroups.First();
                                    conversionItem.MediaFilesFound.Add(mediaGuid);
                                }
                            }
                        }

                        // Find media items that are relative and convert to permanent
                        if (_settings.UpdateMediaFileUrlsToPermanent)
                        {
                            matches = _mediaRegex.Matches(value);
                            foreach (Match match in matches)
                            {
                                if (match.Groups.Count > 1)
                                {
                                    string path = match.Groups[1].Value;
                                    string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value ?? string.Empty;
                                    string pathLookup = "/" + path.Trim('~').Trim('/').ToLower();
                                    Guid? fileGuid = null;
                                    if (_trackingDictionaries.PathToMediaGuid.ContainsKey(pathLookup))
                                    {
                                        fileGuid = _trackingDictionaries.PathToMediaGuid[pathLookup];
                                    }
                                    else if (_trackingDictionaries.EncodedPathToMediaGuid.ContainsKey(pathLookup))
                                    {
                                        fileGuid = _trackingDictionaries.EncodedPathToMediaGuid[pathLookup];
                                    }
                                    if (fileGuid.HasValue)
                                    {
                                        // Only replace first in case other groups in mix.
                                        value = value.ReplaceFirst($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[fileGuid.Value] + extension);
                                        itemsTouched++;
                                        conversionItem.MediaFilesUpdated.Add(fileGuid.Value);
                                    }
                                    else
                                    {
                                        conversionItem.MediaNotFound.Add(path);
                                    }
                                }
                            }
                        }

                        // Convert attachments to media files
                        if (_settings.ConvertAttachments)
                        {
                            matches = _attachmentRegex.Matches(value);
                            foreach (Match match in matches)
                            {
                                if (match.Groups.Count > 1)
                                {
                                    string path = match.Groups[1].Value;
                                    string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value ?? string.Empty;
                                    var attachmentGroups = match.Groups.Cast<Group>().Where(x => Guid.TryParse(x.Value, out var guid)).Select(x => Guid.Parse(x.Value));
                                    if (attachmentGroups.Any())
                                    {
                                        Guid attachmentGuid = attachmentGroups.First();

                                        // Upload to media Library here and get new Guid
                                        if (!_trackingDictionaries.AttachmentGuidToMediaGuid.ContainsKey(attachmentGuid))
                                        {
                                            // Upload to media file HERE
                                            CloneAttachmentToMediaFile(attachmentGuid);
                                        }

                                        // Only replace first in case other groups in mix.
                                        value = value.ReplaceFirst($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_trackingDictionaries.AttachmentGuidToMediaGuid[attachmentGuid]] + extension);
                                        itemsTouched++;
                                        conversionItem.AttachmentsConverted.Add(attachmentGuid);
                                    }
                                    else
                                    {
                                        conversionItem.MediaNotFound.Add(path);
                                    }
                                }
                            }
                        }

                        // Store values
                        if (itemsTouched > 0)
                        {
                            conversionItem.ColumnToValue[columnKey] = value;
                        }
                        conversionItem.ColumnsTouched.Add(columnKey, itemsTouched);
                    }
                }
            }
        }

        private void CloneAttachmentToMediaFile(Guid attachmentGuid)
        {
            var attachment = _attachmentInfoProvider.Get().WhereEquals(nameof(AttachmentInfo.AttachmentGUID), attachmentGuid).TypedResult.FirstOrDefault(); ;

            var newMediaFile = new MediaFileInfo(ConstructHttpPostedFile(attachment.AttachmentBinary, attachment.AttachmentName, attachment.AttachmentMimeType).ToUploadedFile(), _attachmentMediaLibrary.LibraryID, attachment.AttachmentGUID.ToString().Substring(0, 2))
            {
                FileGUID = attachmentGuid,
                FileDescription = attachment.AttachmentDescription,
                FileExtension = attachment.AttachmentExtension,
                FileSize = attachment.AttachmentSize,
                FileMimeType = attachment.AttachmentMimeType,
                FileTitle = attachment.AttachmentTitle,
            };
            foreach (var columnName in attachment.AttachmentCustomData.ColumnNames)
            {
                newMediaFile.FileCustomData.SetValue(columnName, attachment.AttachmentCustomData[columnName]);
            }

            // Save the media file
            _mediaFileInfoProvider.Set(newMediaFile);

            // Add this attachment to media library item
            var attachmentIdToGuid = new AttachmentIDToGuidCloneInfo()
            {
                AttachmentIDToGuidCloneAttachmentID = attachment.AttachmentID,
                AttachmentIDToGuidCloneAttachmentGuid = newMediaFile.FileGUID
            };
            _attachmentGuidInfoProvider.Set(attachmentIdToGuid);

            string newUrl = $"/getmedia/{newMediaFile.FileGUID}/{newMediaFile.FileTitle}{newMediaFile.FileExtension}";
            if (_settings.LowercaseNewUrls)
            {
                newUrl = newUrl.ToLower();
            }
            // Add to the dictionaries
            _trackingDictionaries.AttachmentGuidToMediaGuid.Add(attachmentGuid, newMediaFile.FileGUID);
            _trackingDictionaries.FileGuidToNewUrl.Add(newMediaFile.FileGUID, newUrl);
        }

        public HttpPostedFile ConstructHttpPostedFile(byte[] data, string filename, string contentType)
        {
            // Get the System.Web assembly reference
            Assembly systemWebAssembly = typeof(HttpPostedFileBase).Assembly;
            // Get the types of the two internal types we need
            Type typeHttpRawUploadedContent = systemWebAssembly.GetType("System.Web.HttpRawUploadedContent");
            Type typeHttpInputStream = systemWebAssembly.GetType("System.Web.HttpInputStream");

            // Prepare the signatures of the constructors we want.
            Type[] uploadedParams = { typeof(int), typeof(int) };
            Type[] streamParams = { typeHttpRawUploadedContent, typeof(int), typeof(int) };
            Type[] parameters = { typeof(string), typeof(string), typeHttpInputStream };

            // Create an HttpRawUploadedContent instance
            object uploadedContent = typeHttpRawUploadedContent
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, uploadedParams, null)
              .Invoke(new object[] { data.Length, data.Length });

            // Call the AddBytes method
            typeHttpRawUploadedContent
              .GetMethod("AddBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, new object[] { data, 0, data.Length });

            // This is necessary if you will be using the returned content (ie to Save)
            typeHttpRawUploadedContent
              .GetMethod("DoneAddingBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, null);

            // Create an HttpInputStream instance
            object stream = (Stream)typeHttpInputStream
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, streamParams, null)
              .Invoke(new object[] { uploadedContent, 0, data.Length });

            // Create an HttpPostedFile instance
            HttpPostedFile postedFile = (HttpPostedFile)typeof(HttpPostedFile)
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null)
              .Invoke(new object[] { filename, contentType, stream });

            return postedFile;
        }

        private MediaTrackingDictionaries GetMediaTrackingDictionaries()
        {
            MediaMigrationFunctions.UpdateMediaFileTracking();
            MediaMigrationFunctions.UpdateAttachmentTracking();

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
FileGUID, FileName+'.'+FileExtension as FileName
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
                if (_settings.LowercaseNewUrls)
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
            var _fileLocationConfiguratorInfoProvider = CMS.Core.Service.Resolve<IFileLocationConfigurationInfoProvider>();
            var allConfigurations = _fileLocationConfiguratorInfoProvider.Get().TypedResult.GroupBy(x => $"{x.TableName.Replace("[","")}|{x.TableDBSchema.Replace("[", "")}|{x.TableColumnName.Replace("[", "")}".ToLower())
                .ToDictionary(key => key.Key, value => value.First().FileLocationConfigurationID);
            var attachmentGuidToMediaGuid = _attachmentGuidInfoProvider.Get()
                .Source(x => x.InnerJoin<AttachmentInfo>(nameof(AttachmentIDToGuidCloneInfo.AttachmentIDToGuidCloneAttachmentID), nameof(AttachmentInfo.AttachmentGUID)))
                .Columns(nameof(AttachmentInfo.AttachmentGUID), nameof(AttachmentIDToGuidCloneInfo.AttachmentIDToGuidCloneAttachmentGuid))
                .Result.Tables[0].Rows.Cast<DataRow>().GroupBy(x => (Guid)x[nameof(AttachmentInfo.AttachmentGUID)]).ToDictionary(key => key.Key, value => (Guid)value.First()[nameof(AttachmentIDToGuidCloneInfo.AttachmentIDToGuidCloneAttachmentGuid)]);

            var _fileTrackingInfoProvider = CMS.Core.Service.Resolve<IFileTrackingInfoProvider>();
            var mediaGuidToFileTrackingID = _fileTrackingInfoProvider.Get()
                .Source(x => x.InnerJoin<MediaFileInfo>(nameof(FileTrackingInfo.FileTrackingMediaID), nameof(MediaFileInfo.FileID)))
                .Columns(nameof(MediaFileInfo.FileGUID), nameof(FileTrackingInfo.FileTrackingID))
                .Result.Tables[0].Rows.Cast<DataRow>().GroupBy(x => (Guid)x[nameof(MediaFileInfo.FileGUID)]).ToDictionary(key => key.Key, value => (int)value.First()[nameof(FileTrackingInfo.FileTrackingID)]);

            return new MediaTrackingDictionaries(fullPathDictionary, encodedPathDictionary, fileGuidToNewUrl, allMediaPrefixes.Keys.ToList(), attachmentGuidToMediaGuid, allConfigurations, mediaGuidToFileTrackingID);
        }

    }
}
