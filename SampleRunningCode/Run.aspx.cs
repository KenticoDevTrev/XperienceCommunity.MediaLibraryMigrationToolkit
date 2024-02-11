using System;
using XperienceCommunity.MediaLibraryMigrationToolkit;

namespace CMSApp.CMSPages.MigrationTemp
{
    public partial class Run : System.Web.UI.Page
    {
        /// <summary>
        /// MODIFY THIS
        /// </summary>
        /// <returns></returns>

        private MediaLibraryMigrationSettings GetSettings()
        {
            return new MediaLibraryMigrationSettings()
            {
                AttachmentFailureForGuidColumn = AttachmentFailureMode.SetToMediaNotFoundGuid,
                AttachmentFailureForUrl = MediaMissingMode.SetToMediaNotFoundGuid,
                AttachmentMediaLibraryName = "Att",
                AttachmentTruncateFileNameLengthToPreservePath = 50,
                ConvertAttachments = true, // highly recommended, while attachments still work in KX13 and I believe in XbyK, best to move to only using media files unless versioning is needed.
                LowercaseNewUrls = true, // Should be true if going to azure
                MediaMissing = MediaMissingMode.SetToMediaNotFoundGuid,
                MediaNotFoundGuid = new Guid("98d3b757-a5ba-46e1-bc40-898ae45565f4"), // REPLACE
                UpdateMediaFileUrlsToPermanent = true
            };
        }

        /// <summary>
        /// MODIFY THIS
        /// </summary>
        /// <returns></returns>
        private AttachmentPurgeSettings GetAttachmentSettings() {
            return new AttachmentPurgeSettings()
            {
                PurgeType = AttachmentPurgeType.OnlyNonReferencedAttachmentsAndSpecifiedPageTypes,
                PageTypesToSkip = Array.Empty<string>(), // MODIFY TO SKIP PAGE TYPES IN ATTACHMENT PURGE
                SpecificAttachmentGuidsToSkip = Array.Empty<Guid>() // MODIFY TO KEEP SPECIFIC ATTACHMENTS
            }
        }



        protected void btnBuildList_Click(object sender, EventArgs e)
        {
            // Update Table
            MediaMigrationFunctions.UpdateMediaCloneTable();
        }

        protected void btnConvert_Click(object sender, EventArgs e)
        {
            var settings = GetSettings();
            var configurationLocations = MediaMigrationFunctions.GetFileLocationConfigurations();

            var converter = new MediaLibraryMigrationScanner(settings, configurationLocations);

            // Converts the URLs
            converter.RunReplacement();
            // Saves to database
            converter.SaveMediaConversionObjects();


            // Scans and finds all media usage, should run after conversions
            var results = converter.FindAllMediaItems();
            // Saves results to the MediaFile Results
            converter.SetFileFindingResults();
        }

        protected void btnCheckResults(object sender, EventArgs e)
        {
            var settings = GetSettings();
            var configurationLocations = MediaMigrationFunctions.GetFileLocationConfigurations();

            var converter = new MediaLibraryMigrationScanner(settings, configurationLocations);

            // Scans and finds all media usage, should run after conversions
            var results = converter.FindAllMediaItems();
            // Saves results to the MediaFile Results
            converter.SetFileFindingResults();
        }

        protected void btnMigrateToAzure_Click(object sender, EventArgs e)
        {
            MediaMigrationFunctions.MigrateUnprocessedFilesToAzure("/Temp/Azure");
            MediaMigrationFunctions.ClearDuplicateMediaFiles();
        }

        protected void btnRemoveAttachments_Click(object sender, EventArgs e)
        {
            var settings = GetSettings();
            var configurationLocations = MediaMigrationFunctions.GetFileLocationConfigurations();

            var converter = new MediaLibraryMigrationScanner(settings, configurationLocations);

            // Scans and finds all media usage, should run after conversions
            var results = converter.FindAllMediaItems();
            MediaMigrationFunctions.PurgeAttachments(results, );
        }
    }
}