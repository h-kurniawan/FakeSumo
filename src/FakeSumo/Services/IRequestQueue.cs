using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeSumo.Services
{
    public enum EnqueueResponse
    {
        Added,
        MaxRequestsPerSecondError,
        MaxRequestsPerMinuteError
    }

    public interface IRequestQueue
    {
        EnqueueResponse Enqueue(HttpRequest item);
        HttpRequest Dequeu();
        int ItemCount { get; }
    }
}
