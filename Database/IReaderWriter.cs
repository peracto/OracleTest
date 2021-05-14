using System;
using System.IO;
using System.Threading.Tasks;

namespace OracleTest.Database
{
    internal interface IReaderWriter : IDisposable
    {
        bool Read();
        Task<bool> ReadAsync();
        void Write(TextWriter writer);
        void WriteHeaders(TextWriter writer);

        int RowsWritten { get; }
    }

}
