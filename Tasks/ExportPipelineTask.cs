using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bourne.Common.Pipeline;
using OracleTest.Database;
using OracleTest.IO;
using OracleTest.Model;

namespace OracleTest.Tasks
{
    internal class ExportPipelineTask : PipelineTaskBase<DataSourceSlice, DataSourceFile>
    {
        private readonly IDatabaseSource _connection;
        private readonly Action<DataSourceSlice, string, int, int, bool> _feedback;

        public static IPipelineTask<DataSourceSlice, DataSourceFile> Create(
            IDatabaseSource connection,
            Action<DataSourceFile> callback,
            Action<DataSourceSlice, string, int, int, bool> feedback
            )
        {
            return new ExportPipelineTask(connection, callback, feedback);
        }

        private ExportPipelineTask(
            IDatabaseSource connection,
            Action<DataSourceFile> callback,
            Action<DataSourceSlice, string, int, int, bool> feedback
            ) : base(callback)
        {
            _connection = connection;
            _feedback = feedback;
        }
        public override async Task Execute(DataSourceSlice slice)
        {
            slice.DataSource.AddRef();

            try
            {
                var cycler = new FileCycler(
                    i => $@"c:\temp\testfile_{slice.SourceGroup}_{i:D4}.csv"
                );

                using var rdr = await _connection.CreateReader(slice.Query);

                var maxFileSize = slice.DataSource.MaxFileSize;

                while (await rdr.ReadAsync())
                {
                    var rows = 0;
                    await using (var stream = cycler.CreateStream())
                    await using (var writer = new StreamWriter(stream))
                    {
                        Console.WriteLine($@"Creating {cycler.CurrentFileName}");
                        rdr.WriteHeaders(writer);
                        do
                        {
                            rdr.Write(writer);
                            if (++rows % 200000 == 0)
                                _feedback?.Invoke(slice, cycler.CurrentFileName, rows, rdr.RowsWritten, false);
                        } while (stream.Position <= maxFileSize && await rdr.ReadAsync());

                        Console.WriteLine($@"Size: {stream.Position}");
                    }

                    _feedback?.Invoke(slice, cycler.CurrentFileName, rows, rdr.RowsWritten, true);
                    Console.WriteLine($@"Created {cycler.CurrentFileName}");
                    slice.DataSource.AddRef();
                    Output(new DataSourceFile(slice.DataSource, cycler.CurrentFileName, rows));
                }
            }
            finally
            {
                slice.DataSource.ReleaseRef();
            }
        }
    }
}
