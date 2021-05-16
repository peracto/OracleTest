using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Bourne.BatchLoader.IO
{
    public class LifetimeReference<T>
    {
        private readonly ConcurrentDictionary<T, LifetimeToken> _dict = new ConcurrentDictionary<T, LifetimeToken>();
        private readonly Action<T> _trigger;

        public LifetimeReference(Action<T> trigger)
        {
            _trigger = trigger;
        }

        public void AddRef(T key)
        {
            var i = _dict
                .GetOrAdd(key, _ => new LifetimeToken())
                .AddRef();
            Console.WriteLine($"Addref {key}::{i}");
        }

        public void Release(T key)
        {
            if (!_dict.TryGetValue(key, out var value))
                throw new Exception($"Unexpected lifetime key {key}");

            var i = value.Release();
            Console.WriteLine($"Release {key}::{i}");

            if (i == -1) 
                throw new Exception($@"Unexpected lifetime state {i}");
            if (i != 0) 
                return;
            _trigger(key);
        }

        private class LifetimeToken
        {
            private int _lifetime = 0;
            public int AddRef() => Interlocked.Increment(ref _lifetime);
            public int Release() => Interlocked.Decrement(ref _lifetime);
        }
    }
}
