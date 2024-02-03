using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Modules;
using NuGet.Packaging.Core;
using NuGet.Packaging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
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
            }
            catch (Exception ex)
            {
                Service.Resolve<IEventLogService>().LogException("MediaLibraryMigrationToolkitInitializationModule", "IndexError", ex);
            }

            base.OnInit();

            ModulePackagingEvents.Instance.BuildNuSpecManifest.After += BuildNuSpecManifest_After;

        }

        private void BuildNuSpecManifest_After(object sender, BuildNuSpecManifestEventArgs e)
        {
            if (e.ResourceName.Equals("MediaLibraryMigrationToolkit", System.StringComparison.InvariantCultureIgnoreCase))
            {
                // Change the name
                e.Manifest.Metadata.Title = "Kentico Media Library Migration Toolkit";
                e.Manifest.Metadata.SetProjectUrl("https://github.com/KenticoDevTrev/XperienceCommunity.MediaLibraryMigrationToolkit");
                e.Manifest.Metadata.SetIconUrl("https://www.hbs.net/HBS/media/Favicon/favicon-96x96.png");
                e.Manifest.Metadata.Tags = "Kentico Media Azure Migration";
                e.Manifest.Metadata.Id = "XperienceCommunity.MediaLibraryMigrationToolkit";
                e.Manifest.Metadata.ReleaseNotes = "Initial Release";
                // Add nuget dependencies

                // Add dependencies
                List<PackageDependency> NetStandardDependencies = new List<PackageDependency>()
                {
                    new PackageDependency("Kentico.Xperience.Libraries", new VersionRange(new NuGetVersion("13.0.13")), new string[] { }, new string[] {"Build","Analyzers"}),
                };
                PackageDependencyGroup PackageGroup = new PackageDependencyGroup(new NuGet.Frameworks.NuGetFramework(".NETStandard2.0"), NetStandardDependencies);
                e.Manifest.Metadata.DependencyGroups = new PackageDependencyGroup[] { PackageGroup };
                // Add in Designer.cs and .cs files since really hard to include these in class library due to depenencies
                string BaseDir = HttpContext.Current.Server.MapPath("~").Trim('\\');
            }
        }
    }
}
