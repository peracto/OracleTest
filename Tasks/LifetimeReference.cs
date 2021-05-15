using System;
using System.Collections.Concurrent;
using System.Threading;

namespace OracleTest.Tasks
{
    public class LifetimeReference<T>
    {
        private ConcurrentDictionary<T, LifetimeCounter> _dict = new ConcurrentDictionary<T, LifetimeCounter>();
        private Action<T> _trigger;


        public LifetimeReference(Action<T> trigger)
        {
            _trigger = trigger;
        }

        public void AddRef(T key)
        {
            _dict
                .GetOrAdd(key, _ => new LifetimeCounter())
                .AddRef();
        }

        public void Release(T key)
        {
            if (!_dict.TryGetValue(key, out var value))
                throw new Exception($"Unexpected lifetime key {key}");

            var i = value.Release();

            if (i == -1) 
                throw new Exception($@"Unexpected lifetime state {i}");
            if (i != 0) 
                return;
            _trigger(key);
        }

        private class LifetimeCounter
        {
            private int _lifetime = 0;
            public void AddRef() => Interlocked.Increment(ref _lifetime);
            public int Release() => Interlocked.Decrement(ref _lifetime);
        }
    }
}
