using System;
using System.Threading;
using System.Threading.Tasks;
using Snowflake.Data.Client;
using Snowflake.FileStream;
using Snowflake.FileStream.Model;

namespace Bourne.BatchLoader.IO
{
    public class StorageManager
    {
        private readonly SnowflakeDbConnection _connection;

        public StorageManager(SnowflakeDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<IPutFile> Renew()
        {
            using var cmd = _connection.CreateCommand() as SnowflakeDbCommand;
            cmd.CommandText = @"put file://placeholder @haven_work.public.my_stage/bingo/stagey overwrite=true source_compression=gzip";
            cmd.CommandType = System.Data.CommandType.Text;
            var response = await cmd.CustomExecute<SnowflakePutResponse>(CancellationToken.None);
            return PutFiles.Create(response, DateTime.Now.AddSeconds(60 * 15));
        }
    }
}
