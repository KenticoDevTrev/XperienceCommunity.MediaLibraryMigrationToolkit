using CMS;
using CMS.DataEngine;
using CMS.EventLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            base.OnInit();
        }
    }
}
