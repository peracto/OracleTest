using System;
using System.Threading.Tasks;

namespace Bourne.BatchLoader.Database
{
    internal interface IDatabaseSource : IDisposable
    {
        Task<IReaderWriter> CreateReader(string sql);
    }
}
