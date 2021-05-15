using OracleTest.Database;

namespace OracleTest.Model
{
    internal class DataSourceFile : PutFileItem
    {
        public readonly DataSource DataSource;
        public int RowCount { get; }

        public string LifetimeKey { get; }


        public DataSourceFile(DataSource dataSource, string filename, int rowCount)
            : base(filename)
        {
            DataSource = dataSource;
            RowCount = rowCount;
            LifetimeKey = dataSource.FullName;
        }
    }
}
