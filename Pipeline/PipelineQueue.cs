using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Bourne.BatchLoader.Pipeline
{
    public class PipelineQueue<T> : IPipelineQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationToken _cancellationToken;
        private  int _subscribeCount;
        public PipelineQueue(CancellationToken token)
        {
            _cancellationToken = token;
            _semaphore = new SemaphoreSlim(0);
        }

        public void Enqueue(T e)
        {
            _queue.Enqueue(e);
            _semaphore.Release();
        }

        public void Enqueue(IEnumerable<T> e)
        {
            var i = 0;
            foreach (var q in e)
            {
                _queue.Enqueue(q);
                i++;
            }
            _semaphore.Release(i);
        }

        public void Complete()
        {
            Console.Out.WriteLineAsync($"Completing {_subscribeCount}");
            var i = _subscribeCount;
            if (i > 0)
                _semaphore.Release(i);
        }

        public async IAsyncEnumerable<T> GetConsumingEnumerable()
        {
            Interlocked.Increment(ref _subscribeCount);

            while (!_cancellationToken.IsCancellationRequested)
            {
                await _semaphore.WaitAsync(_cancellationToken);

                if (!_queue.TryDequeue(out var m))
                    break;

                yield return m;
            }

            Interlocked.Decrement(ref _subscribeCount);
        }        

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
