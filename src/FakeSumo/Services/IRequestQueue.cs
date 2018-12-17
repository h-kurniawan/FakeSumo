using FakeSumo.Models;

namespace FakeSumo.Services
{
    public enum EnqueueResponse
    {
        Ok,
        MaxRequestsPerSecondError,
        MaxRequestsPerMinuteError,
        MaxSearchJobRequestError,
    }

    public interface IRequestQueue
    {
        EnqueueResponse Enqueue(RequestQueueItem item);
        RequestQueueItem Dequeu();
        int ItemCount { get; }
    }
}
