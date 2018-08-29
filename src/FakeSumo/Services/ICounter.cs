using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeSumo.Services
{
    public interface ICounter
    {
        /// <summary>
        /// Increase the count be the amount.
        /// </summary>
        /// <param name="amount">The amount to increase the counter.</param>
        void Increase(long amount);

        /// <summary>
        /// Get the current count.
        /// </summary>
        /// <returns>The current count.</returns>
        long Count { get; }
    }
}
