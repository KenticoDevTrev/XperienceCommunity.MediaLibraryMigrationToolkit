using CMS.AmazonStorage;
using CMS.Base.UploadExtensions;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.SiteProvider;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using File = System.IO.File;

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

    public partial class MediaLibraryMigrationScanner
    {
        private readonly MediaTrackingDictionaries _trackingDictionaries;
        private readonly IMediaFileInfoProvider _mediaFileInfoProvider;
        private readonly IEventLogService _eventLogProvider;
        private readonly Regex _mediaRegex;
        private readonly Regex _attachmentByPathRegex;
        private readonly Regex _attachmentRegex;
        private readonly Regex _getMediaRegex;
        private readonly Regex _guidRegex;

        private readonly MediaLibraryInfo _attachmentMediaLibrary;
        private readonly MediaLibraryMigrationSettings _settings;
        public List<MediaConversionObject> MediaConversionObjects { get; }
        public List<string> SaveErrors { get; set; } = new List<string>();
        private readonly bool _multipleSitesExist;

        public MediaLibraryMigrationScanner(MediaLibraryMigrationSettings settings, IEnumerable<MediaConversionObject> mediaConversionObjects)
        {
            _settings = settings;

            _trackingDictionaries = MediaMigrationFunctions.GetMediaTrackingDictionaries(settings.LowercaseNewUrls);
            _mediaFileInfoProvider = Service.Resolve<IMediaFileInfoProvider>();
            _eventLogProvider = Service.Resolve<IEventLogService>();

            if (_settings.MediaMissing == MediaMissingMode.SetToMediaNotFoundGuid || _settings.AttachmentFailureForGuidColumn == AttachmentFailureMode.SetToMediaNotFoundGuid)
            {
                if (!_trackingDictionaries.FileGuidToNewUrl.ContainsKey(_settings.MediaNotFoundGuid))
                {
                    throw new NotSupportedException($"If you want to set Media to a 'Not Found' media item, the MediaNotFoundGuid {_settings.MediaNotFoundGuid} must point to a valid Media File");
                }
            }

            // Build media regex based on all possible prefixes
            string allPrefixMatches = $"({_trackingDictionaries.AllMediaPrefixes.Select(x => (x + "/").Replace("/", "\\/")).Join(")|(")})";
            // This regex will match any of the media library relative path that exists
            // It creates a Group 1 of the full Relative Url (without any query string / hash)
            // You can then loop through other groups to see if any start with ? or #, if so then that group contains the stuff after the URL if it had a hash or ?
            // Ending characters for matching are ^ (inline widgets), } " ' (widget / json) < space | and end of line
            char[] escapeEndingChars = "?#nr<|^}\"'\\".ToCharArray();
            char[] nonEscapeEndingChars = " ".ToCharArray();
            var allChars = escapeEndingChars.Select(x => "\\" + x).Union(nonEscapeEndingChars.Select(x => x.ToString()));
            var allCharsExceptHash = escapeEndingChars.Except("#".ToCharArray()).Select(x => "\\" + x).Union(nonEscapeEndingChars.Select(x => x.ToString()));
            var allCharsExceptQuestionHash = escapeEndingChars.Except("?#".ToCharArray()).Select(x => "\\" + x).Union(nonEscapeEndingChars.Select(x => x.ToString()));
            var queryToEnd = $@"((\?([^{string.Join("", allCharsExceptHash)}]*))|(\#([^{string.Join("", allCharsExceptQuestionHash)}]*))){{0,1}}(?=({string.Join("|", allCharsExceptQuestionHash)}|$))";
            var fileName = $@"[^{string.Join("", allChars)}]*\.{{0,1}}[^{string.Join("", allChars)}]*";

            _mediaRegex = new Regex($@"(~?({allPrefixMatches})([^{string.Join("", allChars.Except(new string[] { " " }))}]*\.[^{string.Join("", allChars)}]*)){queryToEnd}", RegexOptions.IgnoreCase);
            _attachmentByPathRegex = new Regex($@"(~?(\/getattachment\/)([^{string.Join("", allChars)}]*\.[^{string.Join("", allChars)}]*)){queryToEnd}", RegexOptions.IgnoreCase);
            _attachmentRegex = new Regex($@"(~?((\/getattachment\/))([0-9A-F]{{8}}[-]?(?:[0-9A-F]{{4}}[-]?){{3}}[0-9A-F]{{12}})(\/{fileName})){queryToEnd}", RegexOptions.IgnoreCase);
            _getMediaRegex = new Regex($@"(~?((\/getmedia\/))([0-9A-F]{8}[-]?(?:[0-9A-F]{{4}}[-]?){{3}}[0-9A-F]{{12}})(\/{fileName})){queryToEnd}", RegexOptions.IgnoreCase);
            _guidRegex = new Regex(@"([0-9A-F]{8}[-][0-9A-F]{4}[-][0-9A-F]{4}[-][0-9A-F]{4}[-][0-9A-F]{12})", RegexOptions.IgnoreCase);

            /*
            _mediaRegex = new Regex($@"(~?({allPrefixMatches})([^\?\#\n\r\<\|\^\}}\""\'\\ ]*\.[^\?\#\n\r\<\|\^\}}\""\'\\ ]*))((\?([^\?\n\r\<\|\^\}}\""\'\\ ]*))|(\#([^\n\r\<\|\^\}}\""\'\\ ]*))){{0,1}}(?=(\<|\||\^|\}}|\""|\'|\\| |$))", RegexOptions.IgnoreCase);
            _attachmentRegex = new Regex(@"(~?((\/getattachment\/))([0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12})([^\?\#\n\r\<\|\^\}\""\'\ ]*\.{0,1}[^\?\#\n\r\<\|\^\}\""\' ]*))((\?([^\?\n\r\<\|\^\}\""\' ]*))|(\#([^\n\r\<\|\^\}\""\' ]*))){0,1}(?=(\<|\||\^|\}|\""|\'| |$))", RegexOptions.IgnoreCase);
            _getMediaRegex = new Regex(@"(~?((\/getmedia\/))([0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12})([^\?\#\n\r\<\|\^\}\""\'\ ]*\.{0,1}[^\?\#\n\r\<\|\^\}\""\' ]*))((\?([^\?\n\r\<\|\^\}\""\' ]*))|(\#([^\n\r\<\|\^\}\""\' ]*))){0,1}(?=(\<|\||\^|\}|\""|\'| |$))", RegexOptions.IgnoreCase);
            _guidRegex = new Regex(@"([0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12})", RegexOptions.IgnoreCase);
            */
            if (_settings.ConvertAttachments)
            {
                _attachmentMediaLibrary = Service.Resolve<IMediaLibraryInfoProvider>().Get().WhereEquals(nameof(MediaLibraryInfo.LibraryName), _settings.AttachmentMediaLibraryName).FirstOrDefault();
                if (_attachmentMediaLibrary == null)
                {
                    throw new NotSupportedException($"Must include a valid Attachment Media Library if you wish to convert attachments.  None found for {_settings.AttachmentMediaLibraryName}");
                }
            }
            MediaConversionObjects = mediaConversionObjects.ToList();

            _multipleSitesExist = Service.Resolve<ISiteInfoProvider>().Get().Count > 1;

        }

        /// <summary>
        /// Updates all items found and converted
        /// </summary>
        /// <returns></returns>
        public int SaveMediaConversionObjects()
        {
            int itemsUpdated = 0;
            foreach (var mediaConversionObject in MediaConversionObjects)
            {
                foreach (var mediaConversionItem in mediaConversionObject.Items)
                {
                    try
                    {
                        if (mediaConversionItem.UpdateIfTouchedItem(mediaConversionObject.Table, mediaConversionObject.Schema, mediaConversionObject.RowIDColumn))
                        {
                            itemsUpdated++;
                        }
                    }
                    catch (ConversionSaveException ex)
                    {
                        SaveErrors.Add(ex.Message);
                    }
                }
            }
            return itemsUpdated;
        }

        /// <summary>
        /// Saves the Media Library items and how many times they were found in a given configuration to the database.
        /// </summary>
        public void SetFileFindingResults()
        {
            Dictionary<int, Dictionary<Guid, int>> configurationIDToFileGuidToCount = new Dictionary<int, Dictionary<Guid, int>>();
            foreach (var mediaConversionObject in MediaConversionObjects)
            {
                foreach (var mediaConversionItem in mediaConversionObject.Items)
                {
                    foreach (var columnKey in mediaConversionItem.ColumnToValue.Keys)
                    {
                        string configurationKey = $"{mediaConversionObject.Table}|{mediaConversionObject.Schema}|{columnKey}".ToLower();
                        if (_trackingDictionaries.TableSchemaColumnKeyToConfigurationID.ContainsKey(configurationKey))
                        {
                            int configurationID = _trackingDictionaries.TableSchemaColumnKeyToConfigurationID[configurationKey];

                            var trackingAndCount = mediaConversionItem.MediaFilesFound.Union(mediaConversionItem.MediaFilesUpdated)
                                .GroupBy(x => x)
                                .ToDictionary(key => key.Key, value => value.Count());
                            if (!configurationIDToFileGuidToCount.ContainsKey(configurationID))
                            {
                                configurationIDToFileGuidToCount.Add(configurationID, new Dictionary<Guid, int>());

                            }

                            var tracking = configurationIDToFileGuidToCount[configurationID];
                            foreach (var key in trackingAndCount.Keys)
                            {
                                if (!tracking.ContainsKey(key))
                                {
                                    tracking.Add(key, 0);
                                }
                                tracking[key] += trackingAndCount[key];
                            }
                        }
                        else
                        {
                            throw new Exception($"Could not find table schema column key with vale {configurationKey}, something went wrong.");
                        }
                    }

                }
            }

            List<string> sqlDelete = new List<string>();
            List<string> sqlInsert = new List<string>();
            // Now generate the SQL statements
            foreach (int configurationId in configurationIDToFileGuidToCount.Keys)
            {
                var trackingToCount = configurationIDToFileGuidToCount[configurationId];
                sqlDelete.AddRange(trackingToCount.Select(x => $"delete from MediaLibraryMigrationToolkit_FileFindingResult where FileFindingResultMediaGuid = '{x.Key}' and FileFindingResultTableConfigurationID = {configurationId}"));
                sqlInsert.AddRange(trackingToCount.Select(x => $"INSERT INTO [dbo].[MediaLibraryMigrationToolkit_FileFindingResult] ([FileFindingResultGuid],[FileFindingResultLastModified],[FileFindingResultMediaGuid],[FileFindingResultTableConfigurationID],[FileFindingResultCount]) VALUES (NEWID(),GETDATE(),'{x.Key}',{configurationId},{x.Value})"));
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
        public MediaFindingResult FindAllMediaItems()
        {
            // Get all attachments (that haven't already been converted) and Media Items by guid
            var attachmentByGuid = ConnectionHelper.ExecuteQuery(@"select 
AttachmentID,
AttachmentGuid,
AttachmentName,
NodeAliasPath,
SiteName,
DocumentCulture,
AttachmentLastModified
 from CMS_Attachment
inner join View_CMS_Tree_Joined on AttachmentDocumentID = DocumentID
inner join CMS_Site on SiteID = NodeSiteID
where AttachmentGUID not in (Select FileGuid from Media_File)
order by SiteName, NodeAliasPath, DocumentCulture, AttachmentName", null, QueryTypeEnum.SQLQuery)
                .Tables[0].Rows.Cast<DataRow>()
                .Select(x => new AttachmentSummary()
                {
                    AttachmentID = (int)x["AttachmentID"],
                    AttachmentGuid = (Guid)x["AttachmentGuid"],
                    AttachmentName = (string)x["AttachmentName"],
                    NodeAliasPath = (string)x["NodeAliasPath"],
                    SiteName = (string)x["SiteName"],
                    DocumentCulture = (string)x["DocumentCulture"],
                    LastModified = (DateTime)x["AttachmentLastModified"]
                })
                .GroupBy(x => x.AttachmentGuid)
                .ToDictionary(key => key.Key, value => value.First());

            var mediaByGuid = ConnectionHelper.ExecuteQuery(@"select 
FileID,
FileGUID,
FileName+FileExtension as FileName,
LibraryFolder+'/'+FilePath as FilePath,
SiteName,
LibraryName,
FileModifiedWhen
 from Media_File
inner join Media_Library on LibraryID = FileLibraryID
inner join CMS_Site on SiteID = LibrarySiteID
order by SiteName, LibraryName, FilePath, FileName", null, QueryTypeEnum.SQLQuery)
                .Tables[0].Rows.Cast<DataRow>()
                .Select(x => new MediaSummary()
                {
                    MediaFileID = (int)x["FileID"],
                    MediaGuid = (Guid)x["FileGUID"],
                    MediaName = (string)x["FileName"],
                    MediaPath = (string)x["FilePath"],
                    SiteName = (string)x["SiteName"],
                    LibraryName = (string)x["LibraryName"],
                    LastModified = (DateTime)x["FileModifiedWhen"],

                })
                .GroupBy(x => x.MediaGuid)
                .ToDictionary(key => key.Key, value => value.First());

            var mediaGuidToObjectKeyToReferences = mediaByGuid.ToDictionary(key => key.Key, value => new Dictionary<string, MediaFileObjectReference>());
            var attachmentGuidToObjectKeyToReferences = attachmentByGuid.ToDictionary(key => key.Key, value => new Dictionary<string, MediaFileObjectReference>());

            foreach (var conversionObject in MediaConversionObjects)
            {
                foreach (var conversionItem in conversionObject.Items)
                {
                    // Check for Guid columns first
                    foreach (var columnKey in conversionItem.ColumnToGuidValue.Keys)
                    {
                        string referenceKey = $"{conversionObject.Table}|{conversionItem.ID}|{columnKey}".ToLower();
                        var columnGuidValue = conversionItem.ColumnToGuidValue[columnKey];
                        if (attachmentByGuid.ContainsKey(columnGuidValue))
                        {
                            attachmentByGuid[columnGuidValue].TotalOccurrences++;
                            if (!attachmentGuidToObjectKeyToReferences[columnGuidValue].ContainsKey(referenceKey))
                            {
                                attachmentGuidToObjectKeyToReferences[columnGuidValue].Add(referenceKey, new MediaFileObjectReference(conversionObject.Table, conversionItem.ID, columnKey));
                            }
                            attachmentGuidToObjectKeyToReferences[columnGuidValue][referenceKey].Occurrences++;
                        }
                        if (mediaByGuid.ContainsKey(columnGuidValue))
                        {
                            mediaByGuid[columnGuidValue].TotalOccurrences++;
                            if (!mediaGuidToObjectKeyToReferences[columnGuidValue].ContainsKey(referenceKey))
                            {
                                mediaGuidToObjectKeyToReferences[columnGuidValue].Add(referenceKey, new MediaFileObjectReference(conversionObject.Table, conversionItem.ID, columnKey));
                            }
                            mediaGuidToObjectKeyToReferences[columnGuidValue][referenceKey].Occurrences++;
                        }
                    }

                    foreach (var columnKey in conversionItem.ColumnToValue.Keys)
                    {
                        string referenceKey = $"{conversionObject.Table}|{conversionItem.ID}|{columnKey}".ToLower();
                        // Skip if no values
                        if (string.IsNullOrWhiteSpace(conversionItem.ColumnToValue[columnKey]))
                        {
                            continue;
                        }

                        string value = conversionItem.ColumnToValue[columnKey];
                        var matches = _guidRegex.Matches(value);
                        foreach (Match match in matches)
                        {
                            if (match.Groups.Count > 1 && Guid.TryParse(match.Groups[1].Value, out var guidValue))
                            {
                                if (attachmentByGuid.ContainsKey(guidValue))
                                {
                                    attachmentByGuid[guidValue].TotalOccurrences++;
                                    if (!attachmentGuidToObjectKeyToReferences[guidValue].ContainsKey(referenceKey))
                                    {
                                        attachmentGuidToObjectKeyToReferences[guidValue].Add(referenceKey, new MediaFileObjectReference(conversionObject.Table, conversionItem.ID, columnKey));
                                    }
                                    attachmentGuidToObjectKeyToReferences[guidValue][referenceKey].Occurrences++;
                                }
                                if (mediaByGuid.ContainsKey(guidValue))
                                {
                                    mediaByGuid[guidValue].TotalOccurrences++;
                                    if (!mediaGuidToObjectKeyToReferences[guidValue].ContainsKey(referenceKey))
                                    {
                                        mediaGuidToObjectKeyToReferences[guidValue].Add(referenceKey, new MediaFileObjectReference(conversionObject.Table, conversionItem.ID, columnKey));
                                    }
                                    mediaGuidToObjectKeyToReferences[guidValue][referenceKey].Occurrences++;
                                }
                            }
                        }
                    }
                }
            }

            var results = new MediaFindingResult()
            {
                AllAttachments = attachmentByGuid,
                AttachmentOccurrances = attachmentByGuid.ToDictionary(key => key.Key, value => value.Value.TotalOccurrences),
                AttachmentFileReferences = attachmentGuidToObjectKeyToReferences.ToDictionary(key => key.Key, value => value.Value.Select(x => x.Value).ToList()),
                AllMediaFiles = mediaByGuid,
                MediaOccurrances = mediaByGuid.ToDictionary(key => key.Key, value => value.Value.TotalOccurrences),
                MediaFileReferences = mediaGuidToObjectKeyToReferences.ToDictionary(key => key.Key, value => value.Value.Select(x => x.Value).ToList())
            };

            return results;
        }

        /// <summary>
        /// Replaces all Non /getmedia URLs (and possibly /getattachment) with the proper /getmedia url, this does NOT run the database updates though.
        /// </summary>
        /// <returns></returns>
        public void RunReplacement()
        {
            for (int co = 0; co < MediaConversionObjects.Count(); co++)
            //foreach (var conversionObject in MediaConversionObjects)
            {
                var conversionObject = MediaConversionObjects[co];
                for (int coi = 0; coi < conversionObject.Items.Count; coi++)
                //foreach (var conversionItem in conversionObject.Items)
                {
                    var conversionItem = conversionObject.Items[coi];
                    var guidColumnKeys = conversionItem.ColumnToGuidValue.Keys.ToArray();
                    foreach (var columnKey in guidColumnKeys)
                    {
                        // Handle GUID Columns specially
                        var columnGuidValue = conversionItem.ColumnToGuidValue[columnKey];
                        if (_settings.ConvertAttachments && _trackingDictionaries.AllAttachmentGuids.ContainsKey(columnGuidValue))
                        {
                            // Upload to media Library here and get new Guid
                            if (!_trackingDictionaries.FileGuidToNewUrl.ContainsKey(columnGuidValue))
                            {
                                // Upload to media file HERE
                                if (!CloneAttachmentToMediaFile(columnGuidValue))
                                {
                                    conversionItem.AttachmentConversionFailures.Add(columnGuidValue);
                                    switch (_settings.AttachmentFailureForGuidColumn)
                                    {
                                        case AttachmentFailureMode.Leave:

                                            break;
                                        case AttachmentFailureMode.SetNull:
                                            conversionItem.ColumnToGuidValue[columnKey] = Guid.Empty;
                                            conversionItem.ColumnsTouched.Add(columnKey, 1);
                                            break;
                                        case AttachmentFailureMode.SetToMediaNotFoundGuid:
                                            conversionItem.ColumnToGuidValue[columnKey] = _settings.MediaNotFoundGuid;
                                            conversionItem.ColumnsTouched.Add(columnKey, 1);
                                            break;
                                    }
                                    continue;
                                }
                            }

                            conversionItem.AttachmentsConverted.Add(columnGuidValue);
                        }
                    }

                    var columnKeys = conversionItem.ColumnToValue.Keys.ToArray();
                    foreach (var columnKey in columnKeys)
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
                                string path = match.Groups[1].Value.Trim();
                                string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value.Trim() ?? string.Empty;
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
                                    string path = match.Groups[1].Value.Trim();
                                    string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value.Trim() ?? string.Empty;
                                    string pathLookup = "/" + path.Trim('~').TrimStart('/').ToLower();
                                    Guid? fileGuid = null;
                                    if (_trackingDictionaries.PathToMediaGuid.ContainsKey(pathLookup))
                                    {
                                        fileGuid = _trackingDictionaries.PathToMediaGuid[pathLookup];
                                    }
                                    else if (_trackingDictionaries.EncodedPathToMediaGuid.ContainsKey(pathLookup))
                                    {
                                        fileGuid = _trackingDictionaries.EncodedPathToMediaGuid[pathLookup];
                                    }
                                    else
                                    {
                                        // Last attempt, see if there is either that begin with the path.
                                        var possibleKey = _trackingDictionaries.PathToMediaGuid.Keys.Where(x => pathLookup.IndexOf(x, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                                        var possibleKey2 = _trackingDictionaries.EncodedPathToMediaGuid.Keys.Where(x => pathLookup.IndexOf(x, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                                        if (possibleKey != null)
                                        {
                                            fileGuid = _trackingDictionaries.PathToMediaGuid[possibleKey];
                                            path = path.Substring(0, path.Length - (pathLookup.Length - possibleKey.Length)); // Update path and trim off any 'extra' that it may have found
                                        }
                                        else if (possibleKey2 != null)
                                        {
                                            fileGuid = _trackingDictionaries.EncodedPathToMediaGuid[possibleKey2];
                                            path = path.Substring(0, path.Length - (pathLookup.Length - possibleKey2.Length)); // Update path and trim off any 'extra' that it may have found
                                        }
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
                                        switch (_settings.MediaMissing)
                                        {
                                            case MediaMissingMode.Leave:
                                                break;
                                            case MediaMissingMode.SetToMediaNotFoundGuid:
                                                value = value.Replace($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_settings.MediaNotFoundGuid] + extension);
                                                itemsTouched++;
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        // Convert attachments to media files
                        if (_settings.ConvertAttachments)
                        {
                            // First look for Guids and convert to getmedia
                            matches = _attachmentRegex.Matches(value);
                            foreach (Match match in matches)
                            {
                                if (match.Groups.Count > 1)
                                {
                                    string path = match.Groups[1].Value.Trim();
                                    string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value.Trim() ?? string.Empty;
                                    var attachmentGroups = match.Groups.Cast<Group>().Where(x => Guid.TryParse(x.Value, out var guid)).Select(x => Guid.Parse(x.Value));
                                    if (attachmentGroups.Any())
                                    {
                                        Guid attachmentGuid = attachmentGroups.First();

                                        if (_trackingDictionaries.AllAttachmentGuids.ContainsKey(attachmentGuid))
                                        {

                                            // Upload to media Library here and get new Guid
                                            if (!_trackingDictionaries.FileGuidToNewUrl.ContainsKey(attachmentGuid))
                                            {
                                                // Upload to media file HERE
                                                if (!CloneAttachmentToMediaFile(attachmentGuid))
                                                {
                                                    conversionItem.AttachmentConversionFailures.Add(attachmentGuid);
                                                    switch (_settings.AttachmentFailureForUrl)
                                                    {
                                                        case MediaMissingMode.Leave:
                                                            break;
                                                        case MediaMissingMode.SetToMediaNotFoundGuid:
                                                            value = value.Replace($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_settings.MediaNotFoundGuid] + extension);
                                                            itemsTouched++;
                                                            break;
                                                    }
                                                    continue;
                                                }
                                            }

                                            // Only replace first in case other groups in mix.
                                            value = value.ReplaceFirst($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[attachmentGuid] + extension);
                                            itemsTouched++;
                                            conversionItem.AttachmentsConverted.Add(attachmentGuid);
                                        }
                                        else
                                        {
                                            conversionItem.MediaNotFound.Add(path);
                                            switch (_settings.MediaMissing)
                                            {
                                                case MediaMissingMode.Leave:
                                                    break;
                                                case MediaMissingMode.SetToMediaNotFoundGuid:
                                                    value = value.Replace($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_settings.MediaNotFoundGuid] + extension);
                                                    itemsTouched++;
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        conversionItem.MediaNotFound.Add(path);
                                        switch (_settings.AttachmentFailureForUrl)
                                        {
                                            case MediaMissingMode.Leave:
                                                break;
                                            case MediaMissingMode.SetToMediaNotFoundGuid:
                                                value = value.Replace($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_settings.MediaNotFoundGuid] + extension);
                                                itemsTouched++;
                                                break;
                                        }
                                    }
                                }
                            }

                            // Now try matching on the old /getattachment/node/alias/path/attachment.name
                            matches = _attachmentByPathRegex.Matches(value);
                            foreach (Match match in matches)
                            {
                                if (match.Groups.Count > 1)
                                {
                                    string path = match.Groups[1].Value.Trim();
                                    string extension = match.Groups.Cast<Group>().FirstOrDefault(x => x.Value.StartsWith("?") || x.Value.StartsWith("#"))?.Value.Trim() ?? string.Empty;
                                    string pathLookup = "/" + path.Trim('~').TrimStart('/').Replace(".aspx", "").ToLower();
                                    Guid? fileGuid = null;
                                    if (_trackingDictionaries.AttachmentNodePathToGuid.ContainsKey(pathLookup))
                                    {
                                        fileGuid = _trackingDictionaries.AttachmentNodePathToGuid[pathLookup];
                                    }
                                    else if (_trackingDictionaries.AttachmentNodePathEncodedToGuid.ContainsKey(pathLookup))
                                    {
                                        fileGuid = _trackingDictionaries.AttachmentNodePathEncodedToGuid[pathLookup];
                                    }
                                    else
                                    {
                                        // Last attempt, see if there is either that begin with the path.
                                        var possibleKey = _trackingDictionaries.AttachmentNodePathToGuid.Keys.Where(x => pathLookup.IndexOf(x, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                                        var possibleKey2 = _trackingDictionaries.AttachmentNodePathEncodedToGuid.Keys.Where(x => pathLookup.IndexOf(x, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                                        if (possibleKey != null)
                                        {
                                            fileGuid = _trackingDictionaries.AttachmentNodePathToGuid[possibleKey];
                                            path = path.Substring(0, path.Length - (pathLookup.Length - possibleKey.Length)); // Update path and trim off any 'extra' that it may have found
                                        }
                                        else if (possibleKey2 != null)
                                        {
                                            fileGuid = _trackingDictionaries.AttachmentNodePathEncodedToGuid[possibleKey2];
                                            path = path.Substring(0, path.Length - (pathLookup.Length - possibleKey2.Length)); // Update path and trim off any 'extra' that it may have found
                                        }
                                    }

                                    if (fileGuid.HasValue)
                                    {
                                        var attachmentGuid = fileGuid.Value;
                                        if (_trackingDictionaries.AllAttachmentGuids.ContainsKey(attachmentGuid))
                                        {
                                            // Upload to media Library here and get new Guid
                                            if (!_trackingDictionaries.FileGuidToNewUrl.ContainsKey(attachmentGuid))
                                            {
                                                // Upload to media file HERE
                                                if (!CloneAttachmentToMediaFile(attachmentGuid))
                                                {
                                                    conversionItem.AttachmentConversionFailures.Add(attachmentGuid);
                                                    switch (_settings.AttachmentFailureForUrl)
                                                    {
                                                        case MediaMissingMode.Leave:
                                                            break;
                                                        case MediaMissingMode.SetToMediaNotFoundGuid:
                                                            value = value.Replace($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_settings.MediaNotFoundGuid] + extension);
                                                            itemsTouched++;
                                                            break;
                                                    }
                                                    continue;
                                                }
                                            }

                                            // Only replace first in case other groups in mix.
                                            value = value.ReplaceFirst($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[attachmentGuid] + extension);
                                            itemsTouched++;
                                            conversionItem.AttachmentsConverted.Add(attachmentGuid);
                                        }
                                        else
                                        {
                                            conversionItem.MediaNotFound.Add(path);
                                            switch (_settings.AttachmentFailureForUrl)
                                            {
                                                case MediaMissingMode.Leave:
                                                    break;
                                                case MediaMissingMode.SetToMediaNotFoundGuid:
                                                    value = value.Replace($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_settings.MediaNotFoundGuid] + extension);
                                                    itemsTouched++;
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        conversionItem.MediaNotFound.Add(path);
                                        switch (_settings.AttachmentFailureForUrl)
                                        {
                                            case MediaMissingMode.Leave:
                                                break;
                                            case MediaMissingMode.SetToMediaNotFoundGuid:
                                                value = value.Replace($"{path}{extension}", _trackingDictionaries.FileGuidToNewUrl[_settings.MediaNotFoundGuid] + extension);
                                                itemsTouched++;
                                                break;
                                        }
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

        /// <summary>
        /// Clones the attachment, returns false if it could not create due to missing binary data.
        /// </summary>
        /// <param name="attachmentGuid"></param>
        /// <returns></returns>
        private bool CloneAttachmentToMediaFile(Guid attachmentGuid)
        {
            // Using a query becuase the attachment binary isn't setting, even when constructing using the dr...
            var dr = ConnectionHelper.ExecuteQuery(@"
select COALESCE(SKS.KeyValue, SKG.KeyValue) as AttachmentFolder,
SiteName,
NodeAliasPath,
CMS_Attachment.* from CMS_Attachment
inner join View_CMS_Tree_Joined on DocumentID = AttachmentDocumentID
inner join CMS_Site on SiteID = NodeSiteID
left join CMS_SettingsKey SKG on SKG.KeyName = 'CMSFilesFolder' and SKG.SiteID is null
left join CMS_SettingsKey SKS on SKS.KeyName = 'CMSFilesFolder' and SKS.SiteID = NodeSiteID
where AttachmentGUID = @AttachmentGuid", new QueryDataParameters() { { "@AttachmentGuid", attachmentGuid } }, QueryTypeEnum.SQLQuery).Tables[0].Rows[0];
            var attachment = new AttachmentInfo(dr);
            MediaFileInfo newMediaFile = new MediaFileInfo();
            string siteName = ValidationHelper.GetString(dr["SiteName"], "");
            string nodeAliasPath = ValidationHelper.GetString(dr["NodeAliasPath"], "");
            string fileTitle = !string.IsNullOrWhiteSpace(attachment.AttachmentTitle) ? attachment.AttachmentTitle : attachment.AttachmentName;
            string newPath = $"{(_multipleSitesExist ? siteName : "")}{nodeAliasPath}".Trim('/');

            string attachmentName = URLHelper.GetSafeFileName((_settings.LowercaseNewUrls ? attachment.AttachmentName.ToLower() : attachment.AttachmentName), siteName);

            // Make sure the path won't exceed 236, it's 236 instead of 256 because when it eventually uploads to azure it puts it in the /App_Data/AzureTemp/[normalpath], so adds 19 characters (rounded to 20)
            var maxPathLength = 256 - "/App_Data/AzureTemp/".Length - 1; // Also some cases we ran into a path of 257 still being calculated even with the 20 characters, so one more to make sure it's 256 max.
            var baseMediaPath = $"{AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").Trim('/')}/{siteName}/media/{_attachmentMediaLibrary.LibraryFolder}";
            // First see if the path allows for the node alias path + at least 40 (guid + extension)
            if ($"{baseMediaPath}/{newPath}".Length + (attachmentName.Length > _settings.AttachmentTruncateFileNameLengthToPreservePath ? _settings.AttachmentTruncateFileNameLengthToPreservePath : attachmentName.Length) >= maxPathLength)
            {
                // Path is too long, so use trimmed version
                newPath = $"{(_multipleSitesExist ? siteName + "/" : "")}path-too-long/{attachmentGuid.ToString().Substring(0, 2)}";
            }

            // Next check the file name length limit
            if ($"{baseMediaPath}/{newPath}/{attachmentName}".Length >= maxPathLength)
            {
                var overage = $"{baseMediaPath}/{newPath}/{attachmentName}".Length - maxPathLength;
                var parts = attachmentName.Split('.');
                var fileName = string.Join(".", parts.Take(parts.Length - 1));
                var extension = parts.Last();
                // shorten name
                attachmentName = $"{fileName.Substring(0, fileName.Length - overage)}.{extension}";
            }

            if (_settings.LowercaseNewUrls)
            {
                newPath = newPath.ToLower();
            }

            if (attachment.AttachmentBinary != null)
            {
                newMediaFile = new MediaFileInfo(ConstructHttpPostedFile(attachment.AttachmentBinary, attachmentName, attachment.AttachmentMimeType).ToUploadedFile(), _attachmentMediaLibrary.LibraryID, newPath)
                {
                    FileGUID = attachmentGuid,
                    FileExtension = attachment.AttachmentExtension,
                    FileSize = attachment.AttachmentSize,
                    FileMimeType = attachment.AttachmentMimeType,
                    FileTitle = fileTitle
                };
            }
            else
            {
                // it's in the file system, get it there
                string path = ValidationHelper.GetString(dr["AttachmentFolder"], "").Trim('~').TrimEnd('/');
                // Relative Path
                if (path.Trim('~').StartsWith("/") || string.IsNullOrWhiteSpace(path.Trim('~')))
                {
                    path = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/").Trim('/') + path;
                }
                string attachmentGuidStr = attachment.AttachmentGUID.ToString().ToLower();
                string attachmentPath = $"{attachmentGuidStr.Substring(0, 2)}/{attachmentGuid}{attachment.AttachmentExtension}".ToLower();
                var possiblePaths = new string[]
                {
                    $"{path}/{siteName.ToLower()}/files/{attachmentPath}".Replace("/","\\"),
                    $"{path}/{siteName.ToLower()}/{attachmentPath}".Replace("/","\\"),
                    $"{path}/files/{attachmentPath}".Replace("/","\\"),
                    $"{path}/{attachmentPath}".Replace("/","\\"),
                };
                string foundPath = string.Empty;
                foreach (var possiblePath in possiblePaths)
                {
                    if (string.IsNullOrWhiteSpace(foundPath) && File.Exists(possiblePath))
                    {
                        foundPath = possiblePath;
                    }
                }
                // can't find
                if (string.IsNullOrWhiteSpace(foundPath))
                {
                    return false;
                }

                // Found
                newMediaFile = new MediaFileInfo(foundPath, _attachmentMediaLibrary.LibraryID, newPath)
                {
                    FileGUID = attachmentGuid,
                    FileExtension = attachment.AttachmentExtension,
                    FileSize = attachment.AttachmentSize,
                    FileMimeType = attachment.AttachmentMimeType,
                };
                newMediaFile.FileName = attachmentName;

            }

            if (newMediaFile.FileName.EndsWith(attachment.AttachmentExtension, StringComparison.OrdinalIgnoreCase))
            {
                newMediaFile.FileName = newMediaFile.FileName.Substring(0, newMediaFile.FileName.Length - attachment.AttachmentExtension.Length);
                newMediaFile.FileExtension = attachment.AttachmentExtension;
            }

            if (!string.IsNullOrWhiteSpace(attachment.AttachmentTitle))
            {
                newMediaFile.FileTitle = attachment.AttachmentTitle;
            }
            if (!string.IsNullOrWhiteSpace(attachment.AttachmentDescription))
            {
                newMediaFile.FileDescription = attachment.AttachmentDescription;
            }
            if (attachment.AttachmentImageWidth > 0)
            {
                newMediaFile.FileImageWidth = attachment.AttachmentImageWidth;
            }
            if (attachment.AttachmentImageHeight > 0)
            {
                newMediaFile.FileImageHeight = attachment.AttachmentImageHeight;
            }

            foreach (var columnName in attachment.AttachmentCustomData.ColumnNames)
            {
                newMediaFile.FileCustomData.SetValue(columnName, attachment.AttachmentCustomData[columnName]);
            }

            try
            {
                // Save the media file
                _mediaFileInfoProvider.Set(newMediaFile);
            }
            catch (PathTooLongException)
            {
                _eventLogProvider.LogEvent(EventTypeEnum.Error, "MediaLibraryMigrationScanner", "AttachmentPathTooLong", eventDescription: $"Could not make a path short enough for the system's max path length, tried {newPath}");
                // Try shorter path?
                return false;
            }

            string newUrl = $"/getmedia/{newMediaFile.FileGUID}/{newMediaFile.FileName}{newMediaFile.FileExtension}";
            if (_settings.LowercaseNewUrls)
            {
                newUrl = newUrl.ToLower();
            }
            // Add to the dictionaries
            if (!_trackingDictionaries.FileGuidToNewUrl.ContainsKey(newMediaFile.FileGUID))
            {
                _trackingDictionaries.FileGuidToNewUrl.Add(newMediaFile.FileGUID, newUrl);
            }

            return true;
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



    }



}