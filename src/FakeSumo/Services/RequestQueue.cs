using System;
using System.Collections.Concurrent;
using FakeSumo.Models;

namespace FakeSumo.Services
{
    public class RequestQueue : IRequestQueue
    {
        const int MaxRequestsPerSecond = 4;
        const int MaxRequestsPerMinute = 240;
        const int MaxSearchJobRequest = 200;

        private readonly ConcurrentQueue<RequestQueueItem> _apiQueue;
        private ICounter _searchJobCounter;

        public int ItemCount => _apiQueue.Count;

        public RequestQueue(ICounter searchJobCounter)
        {
            _apiQueue = new ConcurrentQueue<RequestQueueItem>();
            _searchJobCounter = searchJobCounter;
        }

        public EnqueueResponse Enqueue(RequestQueueItem queueItem)
        {
            var response = CanEnqueue(queueItem);
            if (response == EnqueueResponse.Added)
            {
                _apiQueue.Enqueue(queueItem);
                if (queueItem.Endpoint == RequestQueueItem.ApiEndpoint.SearchJobRequest)
                    _searchJobCounter.Increase(1);
            }

            return response;
        }

        public RequestQueueItem Dequeu()
        {
            _apiQueue.TryDequeue(out var queueItem);

            if (queueItem.Endpoint == RequestQueueItem.ApiEndpoint.DeleteJobRequest
                && _searchJobCounter.Count > 0)
                _searchJobCounter.Increase(-1);

            return queueItem;
        }

        private EnqueueResponse CanEnqueue(RequestQueueItem queueItem)
        {
            if (_apiQueue.IsEmpty && _searchJobCounter.Count == 0)
                return EnqueueResponse.Added;

            var itemCount = _apiQueue.Count;
            _apiQueue.TryPeek(out var firstItem);

            var requestedAtUnixTimeSeconds = queueItem.RequestedAt.ToUnixTimeSeconds();
            var firstItemUnixTimeSeconds = firstItem.RequestedAt.ToUnixTimeSeconds();
            var requestedAtUnixTimeMinutes = Math.Floor(requestedAtUnixTimeSeconds / 60D);
            var firstItemUnixTimeMinutes = Math.Floor(firstItemUnixTimeSeconds / 60D);

            if (itemCount >= MaxRequestsPerSecond &&
                requestedAtUnixTimeSeconds == firstItemUnixTimeSeconds)
                return EnqueueResponse.MaxRequestsPerSecondError;

            if (itemCount >= MaxRequestsPerMinute &&
                requestedAtUnixTimeMinutes == firstItemUnixTimeMinutes)
                return EnqueueResponse.MaxRequestsPerMinuteError;

            if (queueItem.Endpoint == RequestQueueItem.ApiEndpoint.SearchJobRequest &&
                _searchJobCounter.Count >= MaxSearchJobRequest)
                return EnqueueResponse.MaxSearchJobRequestError;

            return EnqueueResponse.Added;
        }
    }
}
