using System.Threading.Tasks;
using System;

namespace OracleTest
{
    interface IDatabaseSource : IDisposable
    {
        Task<IReaderWriter> CreateReader(string sql);
    }
}
