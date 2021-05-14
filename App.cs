using OracleTest.Database.Oracle;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bourne.Common.Pipeline;
using OracleTest.Database;
using OracleTest.IO;
using OracleTest.Model;
using OracleTest.Tasks;
using Snowflake.Data.Client;

namespace OracleTest
{
    static class App
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
            Debug.Assert(tablesQueue != null, nameof(tablesQueue) + " != null");
            Debug.Assert(compressQueue != null, nameof(compressQueue) + " != null");
            Debug.Assert(putQueue != null, nameof(putQueue) + " != null");
            Debug.Assert(resolveQueue != null, nameof(resolveQueue) + " != null");

            var sfCred = SnowflakeCredential.Create(snowflakeConnection);

            var tableTasks = tablesQueue.CreatePipelineTasks(
                count: 4,
                pipeFactory: () => ExportPipelineTask.Create(
                    // ReSharper disable once AccessToDisposedClosure
                    connection: con,
                    // ReSharper disable once AccessToDisposedClosure
                    callback: file => compressQueue.Enqueue(file),
                    feedback: (q, f, a, b, s) =>
                    {
                        if (s) Console.WriteLine(@"{0,-40}:Read {1,16} reader:{2,16}", q.DataSource.FullName, a, b);
                    }
                )
            );

            var compressTasks = compressQueue.CreatePipelineTasks(
                count: 1,
                pipeFactory: () => ProcessPipelineTask.Create(
                    sfCred,
                    // ReSharper disable once AccessToDisposedClosure
                    callback: batch => putQueue.Enqueue(batch)
                )
            );

            var putTasks = putQueue.CreatePipelineTasks(
                count: 4,
                pipeFactory: () => ReflectPipelineTask<DataSourceFile>.Create(
                    // ReSharper disable once AccessToDisposedClosure
                    callback: batch => resolveQueue.Enqueue(batch)
                )
            );

            var resolveTasks = resolveQueue.CreatePipelineTasks(
                count: 1,
                pipeFactory: () => ResolvePipelineTask.Create(
                    callback: batch => Console.WriteLine($@"**************** Done File {batch}")
                )
            );

            var controller = new DataSourceController();

            Primer.Prime(controller, tablesQueue);
            await tableTasks.WaitAll();
            await compressTasks.WaitAll();
            await putTasks.WaitAll();
            await resolveTasks.WaitAll();

            Console.WriteLine($@"start:{now} end:{DateTime.Now} diff:{DateTime.Now - now}");
        }

        private static async Task<SnowflakeDbConnection> CreateSnowflakeConnection(string cs)
        {
            SnowflakeDbConnection.HttpClientHandlerDelegate = client =>
            {
                // Verify no certificates have been revoked
                client.CheckCertificateRevocationList = false;
                return client;
            };

            var snowflakeConnection = new SnowflakeDbConnection()
            {
                ConnectionString = cs
            };

            await snowflakeConnection.OpenAsync();
            return snowflakeConnection;
        }
    }
}
