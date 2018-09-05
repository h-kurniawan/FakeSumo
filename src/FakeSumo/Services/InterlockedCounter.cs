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

        public InterlockedCounter()
        {
        }

        public long Count => Interlocked.CompareExchange(ref _count, 0, 0);

        public void Increase(long amount)
        {
            Interlocked.Add(ref _count, amount);
        }
    }
}
