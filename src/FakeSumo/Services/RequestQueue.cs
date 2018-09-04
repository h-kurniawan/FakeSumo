using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;

namespace FakeSumo.Services
{
    public class RequestQueue : IRequestQueue
    {
        const int MaxRequestsPerSecond = 4;
        const int MaxRequestsPerMinute = 240;

        class InternalQueueItem
        {
            public InternalQueueItem(HttpRequest request, DateTimeOffset requestedAt)
            {
                Request = request;
                RequestedAt = requestedAt;
            }

            public HttpRequest Request { get; }
            public DateTimeOffset RequestedAt { get; }
        }

        private readonly ConcurrentQueue<InternalQueueItem> _queue;

        public int ItemCount => _queue.Count;

        public RequestQueue()
        {
            _queue = new ConcurrentQueue<InternalQueueItem>();
        }

        public EnqueueResponse Enqueue(HttpRequest request)
        {
            var requestedAt = DateTimeOffset.UtcNow;

            var response = CanEnqueue(requestedAt);
            if (response == EnqueueResponse.Added)
            {

                var queueItem = new InternalQueueItem(request, requestedAt);
                _queue.Enqueue(queueItem);
            }

            return response;
        }

        public HttpRequest Dequeu()
        {
            _queue.TryDequeue(out var queueItem);
            return queueItem.Request;
        }


        private EnqueueResponse CanEnqueue(DateTimeOffset requestedAt)
        {
            if (_queue.IsEmpty)
                return EnqueueResponse.Added;

            var itemCount = _queue.Count;
            _queue.TryPeek(out var firstItem);

            var requestedAtUnixTimeSeconds = requestedAt.ToUnixTimeSeconds();
            var firstItemUnixTimeSeconds = firstItem.RequestedAt.ToUnixTimeSeconds();
            var requestedAtUnixTimeMinutes = Math.Floor(requestedAtUnixTimeSeconds / 60D);
            var firstItemUnixTimeMinutes = Math.Floor(firstItemUnixTimeSeconds / 60D);

            if (itemCount >= MaxRequestsPerSecond &&
                requestedAtUnixTimeSeconds == firstItemUnixTimeSeconds)
                return EnqueueResponse.MaxRequestsPerSecondError;

            if (itemCount >= MaxRequestsPerMinute &&
                requestedAtUnixTimeMinutes == firstItemUnixTimeMinutes)
                return EnqueueResponse.MaxRequestsPerMinuteError;

            return EnqueueResponse.Added;
        }
    }
}
