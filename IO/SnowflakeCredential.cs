using System.Threading.Tasks;
using Snowflake.FileStream.Model;
using Snowflake.Data.Client;
using System.Threading;

namespace OracleTest
{
    public class SnowflakeCredential
    {
        public static SnowflakeCredential Create(SnowflakeDbConnection connection)
        {
            return new SnowflakeCredential(connection);
        }

        private SnowflakeDbConnection _connection;

        private SnowflakeCredential(SnowflakeDbConnection connection)
        {
            _connection = connection;
        }

        public Task<SnowflakePutResponse> DoIt()
        {
            using var cmd = _connection.CreateCommand() as SnowflakeDbCommand;
            cmd.CommandText = @"put file://placeholder @haven_work.public.my_stage/bingo/stagey overwrite=true source_compression=gzip";
            cmd.CommandType = System.Data.CommandType.Text;
            return cmd.CustomExecute<SnowflakePutResponse>(CancellationToken.None);
        }
    }
}
