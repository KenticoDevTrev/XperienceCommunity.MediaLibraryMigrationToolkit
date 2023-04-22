using CMS.DataEngine;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public class MediaConversionObjectItem
    {
        public MediaConversionObjectItem()
        {

        }
        public int ID { get; set; }
        public Dictionary<string, string> ColumnToValue { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Contains unique identifiers found, both from File reference fields, and also populated from normal scans
        /// </summary>
        public List<Guid> GuidValuesFound { get; set; } = new List<Guid>();
        public Dictionary<string, int> ColumnsTouched { get; set; } = new Dictionary<string, int>();
        public List<string> MediaNotFound { get; set; } = new List<string>();

        public List<Guid> MediaFilesFound { get; set; } = new List<Guid>();
        public List<Guid> MediaFilesUpdated { get; set; } = new List<Guid>();
        public List<Guid> AttachmentsConverted { get; set; } = new List<Guid>();

        public bool UpdateIfTouchedItem(string table, string schema, string rowIDColumn)
        {
            var queryParams = new QueryDataParameters()
            {
                { "@ConversionObjectRowID", ID }
            };
            string query = $@"update [{schema}].[{table}] set ";
            List<string> querySets = new List<string>();
            foreach (string column in ColumnsTouched.Where(x => x.Value > 0).Select(x => x.Key))
            {
                queryParams.Add($"@{column}", ColumnToValue[column]);
                querySets.Add($"[{column}] = @{column}");
            }
            if (querySets.Any())
            {
                query += querySets.Join(", ") + $" WHERE [{rowIDColumn}] = {ID}";
                return ConnectionHelper.ExecuteNonQuery(query, queryParams, QueryTypeEnum.SQLQuery, true) > 0;
            }
            else
            {
                return false;
            }
        }
    }
}
