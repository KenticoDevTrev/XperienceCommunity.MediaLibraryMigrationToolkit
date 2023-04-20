using System;
using System.Collections.Generic;

namespace XperienceCommunity.MediaLibraryMigrationToolkit.Helpers
{
    public class MediaTrackingDictionaries
    {
        public MediaTrackingDictionaries(Dictionary<string, Guid> pathToMediaGuid, Dictionary<string, Guid> encodedPathToMediaGuid, Dictionary<Guid, string> fileGuidToNewUrl, List<string> allMediaPrefixes, Dictionary<Guid, Guid> attachmentGuidToMediaGuid, Dictionary<string, int> tableSchemaColumnKeyToConfigurationID, Dictionary<Guid, int> fileGuidToFileTrackingID)
        {
            PathToMediaGuid = pathToMediaGuid;
            EncodedPathToMediaGuid = encodedPathToMediaGuid;
            FileGuidToNewUrl = fileGuidToNewUrl;
            AllMediaPrefixes = allMediaPrefixes;
            AttachmentGuidToMediaGuid = attachmentGuidToMediaGuid;
            TableSchemaColumnKeyToConfigurationID = tableSchemaColumnKeyToConfigurationID;
            FileGuidToFileTrackingID = fileGuidToFileTrackingID;
        }

        public List<string> AllMediaPrefixes = new List<string>();
        public Dictionary<string, Guid> PathToMediaGuid { get; set; } = new Dictionary<string, Guid>();
        public Dictionary<string, Guid> EncodedPathToMediaGuid { get; set; } = new Dictionary<string, Guid>();
        public Dictionary<Guid, string> FileGuidToNewUrl { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, Guid> AttachmentGuidToMediaGuid { get; set; } = new Dictionary<Guid, Guid>();
        public Dictionary<string, int> TableSchemaColumnKeyToConfigurationID { get; set; } = new Dictionary<string, int>();
        public Dictionary<Guid, int> FileGuidToFileTrackingID { get; set; } = new Dictionary<Guid, int>();
    }
}
