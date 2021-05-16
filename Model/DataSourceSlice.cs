namespace Bourne.BatchLoader.Model
{
    internal class DataSourceSlice
    {
        public string SourceGroup { get; }
        public string Query { get; }
        public string LifetimeKey { get; }
        public int BatchFileSize { get; }

        public DataSourceSlice(string sourceGroup, string query, string lifetimeKey, int batchFileSize)
        {
            SourceGroup = sourceGroup;
            Query = query;
            LifetimeKey = lifetimeKey;
            BatchFileSize = batchFileSize;
        }
    }
}
