namespace Bourne.BatchLoader.Model
{
    internal class DataSourceSlice
    {
        public string SourceGroup { get; }
        public string Query { get; }
        public string LifetimeKey { get; }
        public DataSourceSlice(string sourceGroup, string query, string lifetimeKey)
        {
            SourceGroup = sourceGroup;
            Query = query;
            LifetimeKey = lifetimeKey;
        }
    }
}
