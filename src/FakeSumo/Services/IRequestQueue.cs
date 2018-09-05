using FakeSumo.Models;

namespace FakeSumo.Services
{
    public enum EnqueueResponse
    {
        Added,
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
