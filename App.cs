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
using System.Linq;

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
            using var databaseConnection = OracleDatabaseSource.Create(os);

            using var tablesQueue = new PipelineQueue<DataSourceSlice>(token);
            using var processQueue = new PipelineQueue<DataSourceFile>(token);
            using var uploadQueue = new PipelineQueue<OutputFile>(token);
            using var resolveQueue = new PipelineQueue<DataSource>(token);

            var sources = Primer.GetSources().ToDictionary(k => k.LifetimeKey);

            var lifetimeController = new LifetimeReference<string>(
                v => resolveQueue.Enqueue(sources[v])
            );

            var sfCred = new SnowflakeCredentialManager(snowflakeConnection);

            var tableTasks = tablesQueue.CreatePipelineTasks(
                count: 4,
                pipeFactory: () => new ExportPipelineTask(
                    // ReSharper disable once AccessToDisposedClosure
                    connection: databaseConnection,
                    // ReSharper disable once AccessToDisposedClosure
                    callback: file => {
                        lifetimeController.AddRef(file.LifetimeKey);
                        processQueue.Enqueue(file);
                    },
                    trigger: slice => lifetimeController.Release(slice.LifetimeKey),
                    feedback: (q, f, a, b, s) =>
                    {
                        if (s) Console.WriteLine(@"{0,-40}:Read {1,16} reader:{2,16}", q.DataSource.FullName, a, b);
                    }
                )
            );

            var compressTasks = processQueue.CreatePipelineTasks(
                count: 1,
                pipeFactory: () => new ProcessPipelineTask(
                    sfCred,
                    // ReSharper disable once AccessToDisposedClosure
                    callback: batch => uploadQueue.Enqueue(batch)
                )
            );

            var uploadTasks = uploadQueue.CreatePipelineTasks(
                count: 4,
                pipeFactory: () => new UploadPipelineTask(
                    // ReSharper disable once AccessToDisposedClosure
                    callback: batch => lifetimeController.Release(batch.LifetimeKey)
                )
            );

            var resolveTasks = resolveQueue.CreatePipelineTasks(
                count: 1,
                pipeFactory: () => new ResolvePipelineTask(
                    callback: batch => Console.WriteLine($@"**************** Done File {batch}")
                )
            );


            foreach (var item in Primer.Prime(sources.Values))
            {
                lifetimeController.AddRef(item.LifetimeKey);
                tablesQueue.Enqueue(item);
            }

            await tableTasks.WaitAll();
            await compressTasks.WaitAll();
            await uploadTasks.WaitAll();
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
