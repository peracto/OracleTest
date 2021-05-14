using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;

namespace OracleTest.Database.Oracle
{
    internal class OracleDatabaseSource : IDatabaseSource 
    {
        public static IDatabaseSource Create(string connectionString)
        {
            var con = new OracleConnection(connectionString);
            con.Open();
            return new OracleDatabaseSource(con);
        }

        private readonly OracleConnection _connection;
        private OracleDatabaseSource(OracleConnection connection)
        {
            _connection = connection;
        }

        async Task<IReaderWriter> IDatabaseSource.CreateReader(string sql)
        {
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.FetchSize = 1024 * 1024 * 20; // 1MB
            var rdr = await cmd.ExecuteReaderAsync();
            return OracleDataWriter.Create((OracleDataReader) rdr);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
