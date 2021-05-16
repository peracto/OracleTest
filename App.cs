using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bourne.BatchLoader.Database;
using Bourne.BatchLoader.Database.Oracle;
using Bourne.BatchLoader.IO;
using Bourne.BatchLoader.Model;
using Bourne.BatchLoader.Pipeline;
using Bourne.BatchLoader.Tasks;
using Snowflake.Data.Client;

namespace Bourne.BatchLoader
{
    internal static class App
    {
        public static async Task Execute(string targetConnectionString, string sourceConnectionString)
        {
            var now = DateTime.Now;
            using var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            await using var snowflakeConnection = await CreateSnowflakeConnection(targetConnectionString);
            using var databaseConnection = OracleDatabaseSource.Create(sourceConnectionString);
            using var tablesQueue = new PipelineQueue<DataSourceSlice>("tables",token);
            using var processQueue = new PipelineQueue<DataSourceFile>("process",token);
            using var uploadQueue = new PipelineQueue<UploadItem>("upload", token);
            using var resolveQueue = new PipelineQueue<DataSource>("resolve", token);

            var sources = Primer.GetSources().ToDictionary(k => k.LifetimeKey);

            await DoIt(
                new StorageManager(snowflakeConnection),
                databaseConnection,
                sources,
                tablesQueue,
                processQueue,
                uploadQueue,
                resolveQueue,
                new LifetimeReference<string>(
                    // ReSharper disable once AccessToDisposedClosure
                    v => resolveQueue.Enqueue(sources[v])
                ),
                token
            );
            Console.WriteLine($@"start:{now} end:{DateTime.Now} diff:{DateTime.Now - now}");
        }

        private static async Task DoIt(
            StorageManager credentialManager,
            IDatabaseSource databaseConnection,
            IDictionary<string, DataSource> sources,
            PipelineQueue<DataSourceSlice> tablesQueue,
            PipelineQueue<DataSourceFile> processQueue,
            PipelineQueue<UploadItem> uploadQueue,
            PipelineQueue<DataSource> resolveQueue,
            LifetimeReference<string> lifetimeController,
            CancellationToken token
        )
        {
            var tableTasks = tablesQueue.CreatePump(
                pipeFactory: () => new ExportPipelineTask(
                    connection: databaseConnection,
                    emitFile: state=>
                    {
                        lifetimeController.AddRef(state.DataSourceSlice.LifetimeKey);
                        processQueue.Enqueue(
                            new DataSourceFile(
                                state.Filename,
                                state.DataSourceSlice.LifetimeKey,
                                state.RowCount)
                        );
                    },
                    feedback: state =>
                    {
                        if (state.State != ExportPipelineTask.ExportFeedbackState.Complete) return;
                        Console.Out.WriteLine(
                            $@"{state.Filename:-40}:State:{state.State}:Rows:{state.RowCount:D16}:TotalRows:{state.TotalRowCount:D16}"
                        );
                    }
                ),
                threadCount: 4,
                action: slice => lifetimeController.Release(slice.LifetimeKey)
            );

            var processTasks = processQueue.CreatePump(
                pipeFactory: () => new ProcessPipelineTask(credentialManager),
                threadCount: 1,
                action: batch => uploadQueue.Enqueue(batch)
            );

            var uploadTasks = uploadQueue.CreatePump(
                pipeFactory: () => new UploadPipelineTask(),
                threadCount: 4,
                action: batch => lifetimeController.Release(batch.LifetimeKey)
            );

            var resolveTasks = resolveQueue.CreatePump(
                pipeFactory: () => new ResolvePipelineTask(),
                threadCount: 1,
                action: batch => Console.WriteLine($@"**************** Done File {batch}")
            );

            await Task.Run(
                () =>
                {
                    foreach (var item in Primer.Prime(sources.Values))
                    {
                        lifetimeController.AddRef(item.LifetimeKey);
                        tablesQueue.Enqueue(item);
                    }

                    tablesQueue.Complete();
                },
                token
            );

            await Task.WhenAll(tableTasks);
            processQueue.Complete();
            await Task.WhenAll(processTasks);
            uploadQueue.Complete();
            await Task.WhenAll(uploadTasks);
            resolveQueue.Complete();
            await Task.WhenAll(resolveTasks);
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
