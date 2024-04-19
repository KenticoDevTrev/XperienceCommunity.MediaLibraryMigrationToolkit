using CMS;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Modules;
using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using XperienceCommunity.MediaLibraryMigrationToolkit;

[assembly: RegisterModule(typeof(MediaLibraryMigrationToolkitInitializationModule))]
namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public class MediaLibraryMigrationToolkitInitializationModule : Module
    {
        public MediaLibraryMigrationToolkitInitializationModule() : base("MediaLibraryMigrationToolkitInitializationModule")
        {


        }

        protected override void OnInit()
        {
            // Ensure index exists
            string sql = @"
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_UniqueConfigurations' AND object_id = OBJECT_ID('MediaLibraryMigrationToolkit_FileLocationConfiguration'))
    BEGIN        
	CREATE UNIQUE NONCLUSTERED INDEX [IX_UniqueConfigurations] ON [dbo].[MediaLibraryMigrationToolkit_FileLocationConfiguration]
	(
		[TableName] ASC,
		[TableColumnName] ASC,
		[TableDBSchema] ASC,
		[TableIdentifierColumn] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END";
            try
            {
                ConnectionHelper.ExecuteNonQuery(sql, new QueryDataParameters(), QueryTypeEnum.SQLQuery, false);
            } catch(Exception ex)
            {
                EventLogProvider.LogException("MediaLibraryMigrationToolkitInitializationModule", "IndexError", ex);
            }

            ModulePackagingEvents.Instance.BuildNuSpecManifest.After += BuildNuSpecManifest_After;

            base.OnInit();
        }

        private void BuildNuSpecManifest_After(object sender, BuildNuSpecManifestEventArgs e)
        {
            if (e.ResourceName.Equals("MediaLibraryMigrationToolkit", System.StringComparison.InvariantCultureIgnoreCase))
            {
                // Change the name
                e.Manifest.Metadata.Title = "Kentico Media Library Migration Toolkit";
                e.Manifest.Metadata.ProjectUrl = "https://github.com/KenticoDevTrev/XperienceCommunity.MediaLibraryMigrationToolkit";
                e.Manifest.Metadata.IconUrl = "https://www.hbs.net/HBS/media/Favicon/favicon-96x96.png";
                e.Manifest.Metadata.Tags = "Kentico Media Azure Migration";
                e.Manifest.Metadata.Id = "XperienceCommunity.MediaLibraryMigrationToolkit";
                e.Manifest.Metadata.ReleaseNotes = "Fixed SQL Queries so the Error field is in the right spot for the insert.";

                // Add dependencies
                var dependencies = new List<ManifestDependency>();
                dependencies.Add(new ManifestDependency()
                {
                    Id = "Kentico.Libraries",
                    Version = "12.0.29"
                });
                e.Manifest.Metadata.DependencySets = new List<ManifestDependencySet>()
                {
                    new ManifestDependencySet()
                    {
                        Dependencies = dependencies
                    }
                };
            }
        }
    }
}
