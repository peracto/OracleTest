using OracleTest.Database;

namespace OracleTest.Model
{
    internal class DataSourceSlice
    {
        public readonly string SourceGroup;
        public readonly string Query;
        public readonly string LifetimeKey;
        public readonly DataSource DataSource;
        public DataSourceSlice(DataSource dataSource, string sourceGroup, string query, string lifetimeKey)
        {
            DataSource = dataSource;
            SourceGroup = sourceGroup;
            Query = query;
            LifetimeKey = lifetimeKey;
        }
    }
}
