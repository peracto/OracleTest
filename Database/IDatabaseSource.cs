using System;
using System.Threading.Tasks;

namespace OracleTest.Database
{
    internal interface IDatabaseSource : IDisposable
    {
        Task<IReaderWriter> CreateReader(string sql);
    }
}
