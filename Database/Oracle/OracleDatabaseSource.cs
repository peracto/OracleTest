using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;
using System;

namespace OracleTest.Database.Oracle
{
    class OracleDatabaseSource : IDatabaseSource 
    {
        public static IDatabaseSource Create(string connectionString)
        {
            var con = new OracleConnection(connectionString);
            con.Open();
            return new OracleDatabaseSource(con);
        }

        private OracleConnection _connection;
        private OracleDatabaseSource(OracleConnection connection)
        {
            _connection = connection;
        }

        async Task<IReaderWriter> IDatabaseSource.CreateReader(string sql)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.FetchSize = 1024 * 1024 * 20; // 1MB
            var rdr = await cmd.ExecuteReaderAsync();
            return OracleDataWriter.Create((OracleDataReader) rdr);
        }

        public void Dispose()
        {
            if (_connection != null) _connection.Dispose();
        }
    }
}
