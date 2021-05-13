using OracleTest.Database.Oracle;
using Snowflake.Data.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OracleTest
{
    class App
    {
        public static async Task Execute(string cs, string os)
        {
            var now = DateTime.Now;

            var snowflakeConnection = await CreateSnowflakeConnection(cs);
            using var tokenSource = new CancellationTokenSource();
            
            var token = tokenSource.Token;

            using var con = OracleDatabaseSource.Create(os);

            using var tablesQueue = new PipelineQueue<DataSourceSlice>(token);
            using var compressQueue = new PipelineQueue<DataSourceFile>(token);
            using var putQueue = new PipelineQueue<IReflectPipelineTask<DataSourceFile>>(token);
            using var resolveQueue = new PipelineQueue<DataSourceFile>(token);

            var sfCred = SnowflakeCredential.Create(snowflakeConnection);

            var tableTasks = tablesQueue.CreatePipelineTasks(
                count: 4,
                task: () => ExportPipelineTask.Create(
                    connection: con,
                    callback: file => compressQueue.Enqueue(file),
                    feedback: (DataSourceSlice q, string f, int a, int b, bool s) =>
                    {
                        if (s) Console.WriteLine("{0,-40}:Read {1,16} reader:{2,16}", q.DataSource.FullName, a, b);
                    }
                )
            );

            var compressTasks = compressQueue.CreatePipelineTasks(
                count: 1,
                task: () => CompressPipelineTask.Create(
                    sfCred,
                    callback: batch => putQueue.Enqueue(batch)
                )
            );

            var putTasks = putQueue.CreatePipelineTasks(
                count: 4,
                task: () => ReflectPipelineTask<DataSourceFile>.Create(
                    callback: batch => resolveQueue.Enqueue(batch)
                )
            );

            var resolveTasks = resolveQueue.CreatePipelineTasks(
                count: 1,
                task: () => ResolvePipelineTask.Create(
                    callback: batch => Console.WriteLine($"**************** Done File {batch}")
                )
            );

            Primer.Prime(tablesQueue);
            await tableTasks.WaitAll();
            await compressTasks.WaitAll();
            await putTasks.WaitAll();
            await resolveTasks.WaitAll();

            Console.WriteLine($"start:{now} end:{DateTime.Now} diff:{DateTime.Now - now}");
        }

        private static async Task<SnowflakeDbConnection> CreateSnowflakeConnection(string cs)
        {
            SnowflakeDbConnection.HttpClientHandlerDelegate = client =>
            {
                // Verify no certificates have been revoked
                client.CheckCertificateRevocationList = false;
                return client;
            };

            var sfcon = new SnowflakeDbConnection()
            {
                ConnectionString = cs
            };

            await sfcon.OpenAsync();
            return sfcon;
        }
    }
}
