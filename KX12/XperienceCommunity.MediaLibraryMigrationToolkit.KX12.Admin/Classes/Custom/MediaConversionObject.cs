using System.Collections.Generic;
using System.Linq;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public class MediaConversionObject
    {
        public MediaConversionObject(string table, string schema, string rowIDColumn)
        {
            Table = table;
            Schema = schema;
            RowIDColumn = rowIDColumn;
            
        }

        public string Table { get; set; }
        public string Schema { get; set; }
        public string RowIDColumn { get; set; }
        public List<string> ColumnsToCheck { get; set; } = new List<string>();
        public List<MediaConversionObjectItem> Items { get; set; } = new List<MediaConversionObjectItem>();

        public string GetItemsSql()
        {
            return $"Select [{RowIDColumn}], {(ColumnsToCheck.Any() ? string.Join(", ", ColumnsToCheck.Select(x => $"[{x}]")) : "0 as NoColumns")} from [{Schema}].[{Table}] order by [{RowIDColumn}]";
        }
    }
}
