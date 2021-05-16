using System;
using System.IO;
using System.Threading.Tasks;
using Bourne.BatchLoader.Database;
using Bourne.BatchLoader.Model;
using Bourne.BatchLoader.Pipeline;

namespace Bourne.BatchLoader.Tasks
{
    internal class ExportPipelineTask : PipelineTaskBase<DataSourceSlice, DataSourceSlice>
    {
        private readonly IDatabaseSource _connection;
        private readonly Action<ExportFeedback> _feedback;
        private readonly Action<ExportFeedback> _emitFile;

        public ExportPipelineTask(
            IDatabaseSource connection,
            Action<ExportFeedback> emitFile,
            Action<ExportFeedback> feedback
        )
        {
            _connection = connection;
            _feedback = feedback;
            _emitFile = emitFile;
        }

        public override async Task<DataSourceSlice> Execute(DataSourceSlice slice)
        {
            var batchFileSize = slice.BatchFileSize;

            var cycler = new FileCycler(i => $@"c:\temp\IMPORT_{slice.SourceGroup}_{i:D4}.csv");
            using var rdr = await _connection.CreateReader($@"select * from ({slice.Query}) a where rownum <= 10000000");

            while (await rdr.ReadAsync())
            {
                var rows = 0;
                
                _feedback?.Invoke(new ExportFeedback(
                    slice, 
                    cycler.CurrentFileName, 
                    rdr.RowsWritten, rows,
                    ExportFeedbackState.Start)
                );
                
                await using (var stream = cycler.CreateStream())
                await using (var writer = new StreamWriter(stream))
                {
                    rdr.WriteHeaders(writer);
                    do
                    {
                        rdr.Write(writer);
                        if (++rows % 1000000 != 0) continue;
                        _feedback?.Invoke(
                            new ExportFeedback(
                                slice,
                                cycler.CurrentFileName,
                                rdr.RowsWritten,
                                rows,
                                ExportFeedbackState.Active
                            )
                        );
                    } while (stream.Position <= batchFileSize && await rdr.ReadAsync());
                }

                var feedback= new ExportFeedback(
                    slice,
                    cycler.CurrentFileName,
                    rdr.RowsWritten,
                    rows,
                    ExportFeedbackState.Complete
                );
                _emitFile?.Invoke(feedback);
                _feedback?.Invoke(feedback);
            }
            return slice;
        }

        public enum ExportFeedbackState
        {
            Start,
            Active,
            Complete
        }

        public class ExportFeedback
        {
            public int TotalRowCount { get; }
            public int RowCount { get; }
            public string Filename { get; }
            public DataSourceSlice DataSourceSlice { get; }
            
            public ExportFeedbackState State { get; }

            internal ExportFeedback(DataSourceSlice slice, string filename, int totalRowCount, int rowCount, ExportFeedbackState state)
            {
                DataSourceSlice = slice;
                Filename = filename;
                TotalRowCount = totalRowCount;
                RowCount = rowCount;
                State = state;
            }
        }

        private class FileCycler
        {
            private int _index;
            private readonly Func<int, string> _template;

            public FileCycler(Func<int, string> template)
            {
                _template = template;
            }

            public Stream CreateStream()
            {
                CurrentFileName = _template(_index++);
                return File.Create(CurrentFileName);
            }

            public string CurrentFileName { get; private set; }
        }
    }
}