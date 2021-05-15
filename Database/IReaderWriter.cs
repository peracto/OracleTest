using System;
using System.IO;
using System.Threading.Tasks;

namespace Bourne.BatchLoader.Database
{
    internal interface IReaderWriter : IDisposable
    {
        Task<bool> ReadAsync();
        void Write(TextWriter writer);
        void WriteHeaders(TextWriter writer);

        int RowsWritten { get; }
    }

}
