using CMS.DocumentEngine;
using CMS.MediaLibrary;
using System;
using System.Collections.Generic;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{


    public class MediaFindingResult
    {
        public Dictionary<Guid, MediaSummary> AllMediaFiles { get; set; }
        public Dictionary<Guid, AttachmentSummary> AllAttachments { get; set; }
        public Dictionary<Guid, int> MediaOccurrances { get; set; }
        public Dictionary<Guid, int> AttachmentOccurrances { get; set; }
        public Dictionary<Guid, List<MediaFileObjectReference>> MediaFileReferences { get; set; }
        public Dictionary<Guid, List<MediaFileObjectReference>> AttachmentFileReferences { get; set; }



    }
}
