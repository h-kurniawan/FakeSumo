using System;
using Microsoft.AspNetCore.Http;

namespace FakeSumo.Models
{
    public class RequestQueueItem
    {
        public RequestQueueItem(HttpRequest request, ApiEndpoint endpoint)
        {
            Request = request;
            Endpoint = endpoint;
            RequestedAt = DateTimeOffset.UtcNow;
        }

        public enum ApiEndpoint
        {
            SearchJobRequest,
            GetJobStatus,
            GetMessage,
            DeleteJobRequest
        }

        public ApiEndpoint Endpoint { get; set; }
        public HttpRequest Request { get; set; }
        public DateTimeOffset RequestedAt { get;  }
    }
}
