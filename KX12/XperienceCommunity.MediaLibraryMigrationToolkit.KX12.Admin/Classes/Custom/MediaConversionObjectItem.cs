using CMS.DataEngine;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace XperienceCommunity.MediaLibraryMigrationToolkit
{
    public class MediaConversionObjectItem
    {
        public MediaConversionObjectItem()
        {

        }
        public int ID { get; set; }
        /// <summary>
        /// Contains unique identifiers found, both from columns of type UniqueIdentifier, as well as those found in the strings after normal scans.  Can be used to find what media / attachments are still referenced.
        /// </summary>
        public Dictionary<string, Guid> ColumnToGuidValue { get; set; } = new Dictionary<string, Guid>();
        public Dictionary<string, string> ColumnToValue { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> ColumnsTouched { get; set; } = new Dictionary<string, int>();
        public List<string> MediaNotFound { get; set; } = new List<string>();

        public List<Guid> MediaFilesFound { get; set; } = new List<Guid>();
        public List<Guid> MediaFilesUpdated { get; set; } = new List<Guid>();
        public List<Guid> AttachmentsConverted { get; set; } = new List<Guid>();
        public List<Guid> AttachmentConversionFailures { get; set; } = new List<Guid>();

        public bool UpdateIfTouchedItem(string table, string schema, string rowIDColumn)
        {
            var queryParams = new QueryDataParameters()
            {
                { "@ConversionObjectRowID", ID }
            };
            string query = $@"update [{schema}].[{table}] set ";
            List<string> querySets = new List<string>();
            List<string> valuesSet = new List<string>();
            foreach (string column in ColumnsTouched.Where(x => x.Value > 0).Select(x => x.Key))
            {
                // May need to "update" the guid column only in the case where it was an attachment and the attachment conversion failed, so it will now be null.
                if (ColumnToGuidValue.ContainsKey(column))
                {
                    // Possibly set to null
                    queryParams.Add($"@{column}", (ColumnToGuidValue[column] == Guid.Empty ? (Guid?)null : (Guid?)ColumnToGuidValue[column]));
                    querySets.Add($"[{column}] = @{column}");
                } else
                {
                    queryParams.Add($"@{column}", ColumnToValue[column]);
                    querySets.Add($"[{column}] = @{column}");
                    valuesSet.Add($"{column}: {ColumnToValue[column]}");
                }
            }
            if (querySets.Any())
            {
                query += querySets.Join(", ") + $" WHERE [{rowIDColumn}] = {ID}";
                try
                {
                    var results = ConnectionHelper.ExecuteNonQuery(query, queryParams, QueryTypeEnum.SQLQuery, true) > 0;
                    return results;

                } catch (SqlException ex)
                {
                    if (ex.Message.ToLower().IndexOf("string or binary data would be truncated") > -1)
                    {
                        throw new ConversionSaveException($"Some field, after modification, would be too long.  Please check [{schema}].[{table}], called with query '{query}' and data [{string.Join(", ", valuesSet)}]");
                    }
                    
                }
                return false;
            }
            else
            {
                return false;
            }
        }
    }
}
