using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace OracleTest
{
    class ExportPipelineTask : PipelineTaskBase<DataSourceSlice, DataSourceFile>
    {
        private readonly IDatabaseSource Connection;
        private readonly Action<DataSourceSlice, string, int, int, bool> Feedback;

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
            Connection = connection;
            Feedback = feedback;
        }
        public override async Task Execute(DataSourceSlice slice)
        {
            var list = new List<string>();

            using var rdr = await Connection.CreateReader($"select a.* from ({slice.Query}) a where rownum <=10000000");

            using var cycler = new FileCycler(
                (i) => $@"c:\temp\testfile_{slice.SourceGroup}_{i:D4}.csv",
                (string s) => list.Add(s)
            );

            var maxFileSize = slice.DataSource.MaxFileSize;

            while (await rdr.ReadAsync())
            {
                var rows = 0;
                using (var stream = cycler.CreateStream())
                using (var writer = new StreamWriter(stream))
                {
                    Console.WriteLine($"Creating {cycler.CurrentFileName}");
                    rdr.WriteHeaders(writer);
                    do
                    {
                        rdr.Write(writer);
                        if (++rows % 200000 == 0) Feedback?.Invoke(slice, cycler.CurrentFileName, rows, rdr.RowsWritten, false);
                    }
                    while (stream.Position<= maxFileSize && rdr.Read());

                    Console.WriteLine($"Size: {stream.Position}");
                }
                Feedback?.Invoke(slice, cycler.CurrentFileName, rows, rdr.RowsWritten, true);
                Console.WriteLine($"Created {cycler.CurrentFileName}");
                Output(new DataSourceFile(slice.DataSource, cycler.CurrentFileName, rows));
            }
            cycler.Close();
        }
    }
}
