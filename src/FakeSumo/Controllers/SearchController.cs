using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FakeSumo.Models;
using FakeSumo.Services;

namespace FakeSumo.Controllers
{
    [Route("api/v1/[controller]")]
    public class SearchController : Controller
    {
        private const int TooManyRequestHttpCode = 429;

        private Random _random = new Random();

        private readonly ILogger _logger;
        private readonly IRequestQueue _requestQueue;

        public SearchController(ILogger<SearchController> logger, IRequestQueue requestQueue)
        {
            _logger = logger;
            _requestQueue = requestQueue;
        }

        [HttpPost]
        [Route("jobs", Name = "searchJob")]
        public async Task<IActionResult> Jobs(string query, long from, long to)
        {
            var searchLocation =
                new Uri(new Uri($"{Url.RouteUrl("searchJob", null, Request.Scheme, Request.Host.Value)}/"), Guid.NewGuid().ToString());

            return await ProcessRequest(RequestQueueItem.ApiEndpoint.SearchJobRequest, Accepted(searchLocation));
        }

        [HttpGet, Route("jobs/{searchJobId:guid}", Name = "getJobStatus")]
        public async Task<IActionResult> JobStatus(Guid searchJobId)
        {
            var jobStatusResponse = new SumoJobStatusResponse()
            {
                State = SumoJobStatusResponse.States[_random.Next(SumoJobStatusResponse.States.Count())],
                MessageCount = _random.Next(200, 1000)
            };

            return await ProcessRequest(
                RequestQueueItem.ApiEndpoint.GetJobStatus,
                Ok(JsonConvert.SerializeObject(jobStatusResponse)));
        }

        [Route("jobs/{searchJobId:guid}/messages", Name = "getMessages")]
        public async Task<IActionResult> Messages(Guid searchJobId, int offset, int limit)
        {
            var messages = new List<SumoMessage>();
            for (int i = 0; i < limit; i++)
            {
                messages.Add(new SumoMessage
                {
                    Map = new SumoMessageMap
                    {
                        RawMessage = "{\"timestamp\":\"2016-03-15T09:08:18.448+0000\",\"machine_name\":\"ip-10-224-251-101\",\"component\":\"checkmate-sandbox3\",\"component_version\":\"release.2016.1.0 \",\"trace_id\":\"" + Guid.NewGuid() + "\",\"span_id\":\"fa8989d6d65b0b71\"}"
                    }
                });
            }

            var messageResponse = new SumoMessageResponse
            {
                Messages = messages
            };
            return await ProcessRequest(
                RequestQueueItem.ApiEndpoint.GetMessage, 
                Ok(JsonConvert.SerializeObject(messageResponse)));
        }

        [HttpDelete, Route("jobs/{searchJobId:guid}", Name = "deleteSearchJob")]
        public async Task<IActionResult> Delete(Guid searchJobId, int offset, int limit)
        {
            return await ProcessRequest(RequestQueueItem.ApiEndpoint.DeleteJobRequest, Ok());
        }

        private async Task<IActionResult> ProcessRequest(RequestQueueItem.ApiEndpoint endpoint, IActionResult result)
        {
            var queueItem = new RequestQueueItem(Request, endpoint);
            var enqueuResponse = _requestQueue.Enqueue(queueItem);
            if (enqueuResponse != EnqueueResponse.Added)
            {
                var response = 
                    StatusCode(TooManyRequestHttpCode, $"Too many requests. {enqueuResponse} occured.");
                LogMessage(LogLevel.Error, response);
                return response;
            }

            await Task.Delay(_random.Next(500, 2000));
            LogMessage(LogLevel.Information, result);
            _requestQueue.Dequeu();

            return result;
        }

        private void LogMessage(LogLevel logLevel, IActionResult actionResult)
        {
            int statusCode;
            string content = string.Empty;

            if (actionResult is ObjectResult objectResult)
            {
                statusCode = objectResult.StatusCode.Value;
                if (objectResult is AcceptedResult acceptedResult)
                    content = acceptedResult.Location;
                else
                    content = objectResult.Value?.ToString();
            }
            else
                statusCode = (actionResult as StatusCodeResult).StatusCode;

            var message = JsonConvert.SerializeObject(
                new
                {
                    URI = Request.Path.Value,
                    StatusCode = statusCode,
                    Content = content,
                    RequestCount = _requestQueue.ItemCount
                });
            _logger.Log(logLevel, message);
        }
    }
}
