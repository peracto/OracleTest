namespace OracleTest
{
    class DataSourceSlice
    {
        public readonly string SourceGroup;
        public readonly string Query;
        public readonly DataSource DataSource;
        public DataSourceSlice(DataSource dataSource, string sourceGroup, string query)
        {
            DataSource = dataSource;
            SourceGroup = sourceGroup;
            Query = query;
        }
    }
}
