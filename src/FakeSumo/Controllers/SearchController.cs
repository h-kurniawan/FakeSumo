using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeSumo.Models;
using FakeSumo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FakeSumo.Controllers
{
    [Produces("application/json")]
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

            var searchResponse = new SumoJobSearchResponse {
                Id = Guid.NewGuid().ToString(),
                Link = new SumoJobSearchResponse.HypermediaLink
                {
                    Rel = "self",
                    SearchLocation = searchLocation.AbsoluteUri
                }
            };

            return await ProcessRequest(
                RequestQueueItem.ApiEndpoint.SearchJobRequest, Accepted(searchLocation, searchResponse));
        }

        [HttpGet, Route("jobs/{searchJobId:guid}", Name = "getJobStatus")]
        public async Task<IActionResult> JobStatus(Guid searchJobId)
        {
            string jobState;
            var randomNo = _random.Next(1, 10);
            if (randomNo <= 4)
                jobState = SumoJobStatusResponse.States[0];
            else if (randomNo <= 7)
                jobState = SumoJobStatusResponse.States[1];
            else if (randomNo == 8)
                jobState = SumoJobStatusResponse.States[2];
            else if (randomNo == 9)
                jobState = SumoJobStatusResponse.States[3];
            else
                jobState = SumoJobStatusResponse.States[4];

            var jobStatusResponse = new SumoJobStatusResponse()
            {
                State = jobState,
                MessageCount = _random.Next(200, 1000)
            };

            return await ProcessRequest(
                RequestQueueItem.ApiEndpoint.GetJobStatus,
                Ok(jobStatusResponse));
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
                Ok(messageResponse));
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
            if (enqueuResponse != EnqueueResponse.Ok)
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
