using OracleTest.Database;

namespace OracleTest.Model
{
    internal class DataSourceFile  : PutFileItem
    {
        public readonly DataSource DataSource;
        public readonly int RowCount;
        public DataSourceFile(DataSource dataSource, string filename, int rowCount)
            :base(filename)
        {
            DataSource = dataSource;
            RowCount = rowCount;
        }
    }
}
