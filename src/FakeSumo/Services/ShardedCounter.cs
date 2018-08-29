using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FakeSumo.Services
{
    /// <summary>
    /// Implement the ICounterInterface using System.Threading.Interlocked
    /// functions.
    /// </summary>
    public class InterlockedCounter : ICounter
    {
        private long _count = 0;

        public long Count => Interlocked.CompareExchange(ref _count, 0, 0);

        public void Increase(long amount)
        {
            Interlocked.Add(ref _count, amount);
        }
    }

    /// <summary>
    /// Implement the ICounterInterface by creating thread-local counters
    /// fore each thread.
    /// </summary>
    public class ShardedCounter : ICounter
    {
        // Protects _deadShardSum and _shards.
        private readonly object _thisLock = new object();

        // The total sum from the shards from the threads which have terminated.
        private long _deadShardSum = 0;

        // The list of shards.
        private List<Shard> _shards = new List<Shard>();

        // The thread-local slot where shards are stored.
        private readonly LocalDataStoreSlot _slot = Thread.AllocateDataSlot();

        public long Count
        {
            get
            {
                // Sum over all the shards, and clean up dead shards at the
                // same time.
                long sum = _deadShardSum;
                List<Shard> livingShards_ = new List<Shard>();
                lock (_thisLock)
                {
                    foreach (Shard shard in _shards)
                    {
                        sum += shard.Count;
                        if (shard.Owner.IsAlive)
                        {
                            livingShards_.Add(shard);
                        }
                        else
                        {
                            _deadShardSum += shard.Count;
                        }
                    }
                    _shards = livingShards_;
                }
                return sum;
            }
        }

        public void Increase(long amount)
        {
            // Increase counter for this thread.
            Shard counter = Thread.GetData(_slot) as Shard;
            if (null == counter)
            {
                counter = new Shard()
                {
                    Owner = Thread.CurrentThread
                };
                Thread.SetData(_slot, counter);
                lock (_thisLock) _shards.Add(counter);
            }
            counter.Increase(amount);
        }

        private class Shard : InterlockedCounter
        {
            public Thread Owner { get; set; }
        }
    }
}
