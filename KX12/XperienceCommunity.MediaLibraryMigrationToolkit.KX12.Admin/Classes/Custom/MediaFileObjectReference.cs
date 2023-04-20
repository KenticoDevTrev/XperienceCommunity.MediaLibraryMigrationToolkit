namespace XperienceCommunity.MediaLibraryMigrationToolkit.Helpers
{
    public class MediaFileObjectReference
    {
        public MediaFileObjectReference(string objectType, int rowID, string column)
        {
            ObjectType = objectType;
            RowID = rowID;
            Column = column;
        }

        public string ObjectType { get; }
        public int RowID { get; }
        public string Column { get; }
        public int Occurrences { get; set; } = 1;
    }
}
