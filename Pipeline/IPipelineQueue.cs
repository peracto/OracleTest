using System;
using System.Collections.Generic;

namespace Bourne.BatchLoader.Pipeline
{
    public interface IPipelineQueue<T> : IDisposable
    {
        void Enqueue(T e);
        void Enqueue(IEnumerable<T> e);
        IAsyncEnumerable<T> GetConsumingEnumerable();        
        void Complete();
    }
}