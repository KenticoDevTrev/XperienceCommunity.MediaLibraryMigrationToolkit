using System;
using System.Collections.Generic;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public class MediaTrackingDictionaries
    {
        public MediaTrackingDictionaries(Dictionary<string, Guid> pathToMediaGuid, Dictionary<string, Guid> encodedPathToMediaGuid, Dictionary<Guid, string> fileGuidToNewUrl, List<string> allMediaPrefixes, Dictionary<Guid, bool> allAttachmentGuids, Dictionary<string, int> tableSchemaColumnKeyToConfigurationID)
        {
            PathToMediaGuid = pathToMediaGuid;
            EncodedPathToMediaGuid = encodedPathToMediaGuid;
            FileGuidToNewUrl = fileGuidToNewUrl;
            AllMediaPrefixes = allMediaPrefixes;
            AllAttachmentGuids = allAttachmentGuids;
            TableSchemaColumnKeyToConfigurationID = tableSchemaColumnKeyToConfigurationID;
        }

        public List<string> AllMediaPrefixes = new List<string>();
        public Dictionary<string, Guid> PathToMediaGuid { get; set; } = new Dictionary<string, Guid>();
        public Dictionary<string, Guid> EncodedPathToMediaGuid { get; set; } = new Dictionary<string, Guid>();
        public Dictionary<Guid, string> FileGuidToNewUrl { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, bool> AllAttachmentGuids { get; }
        public Dictionary<string, int> TableSchemaColumnKeyToConfigurationID { get; set; } = new Dictionary<string, int>();
    }
}
