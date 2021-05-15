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
        private readonly Action<DataSourceSlice> _trigger;


        public ExportPipelineTask(
            IDatabaseSource connection,
            Action<DataSourceFile> callback,
            Action<DataSourceSlice> trigger,
            Action<DataSourceSlice, string, int, int, bool> feedback
            ) : base(callback)
        {
            _connection = connection;
            _feedback = feedback;
            _trigger = trigger;
        }
        public override async Task Execute(DataSourceSlice slice)
        {
            var lifetimeKey = slice.DataSource.FullName;
            var maxFileSize = slice.DataSource.MaxFileSize;

            var cycler = new FileCycler(
                i => $@"c:\temp\testfile_{slice.SourceGroup}_{i:D4}.csv"
            );

            using var rdr = await _connection.CreateReader($@"select * from ({slice.Query}) a where rownum < 10");

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
                }

                _feedback?.Invoke(slice, cycler.CurrentFileName, rows, rdr.RowsWritten, true);
                Console.WriteLine($@"Created {cycler.CurrentFileName}");
                Output(new DataSourceFile(slice.DataSource, cycler.CurrentFileName, rows));
            }
            _trigger(slice);
        }
    }
}
