using System;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{

    public class AttachmentSummary
    {
        public int AttachmentID { get; set; }
        public Guid AttachmentGuid { get; set; }
        public string AttachmentName { get; set; }
        public string NodeAliasPath { get; set; }
        public string SiteName { get; set; }
        public string DocumentCulture { get; set; }
        public DateTime LastModified { get; set; }
        public int TotalOccurrences { get; set; }
    }


}
