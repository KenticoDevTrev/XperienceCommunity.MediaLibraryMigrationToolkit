using System;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{

        public class MediaSummary
        {
            public int MediaFileID { get; set; }
            public Guid MediaGuid { get; set; }
            public string MediaName { get; set; }
            public string MediaPath { get; set; }
            public string SiteName { get; set; }
            public string LibraryName { get; set; }
            public DateTime LastModified { get; set; }
            public int TotalOccurrences { get; set; }
        }

}
