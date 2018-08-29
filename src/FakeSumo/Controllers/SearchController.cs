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
        private const int MaxRequestCount = 2/*40*/;
        private const int TooManyRequestHttpCode = 429;

        private Random _random = new Random();

        private readonly ILogger _logger;
        private readonly ICounter _counter;


        public SearchController(ILogger<SearchController> logger, ICounter counter)
        {
            _logger = logger;
            _counter = counter;
        }

        [HttpPost]
        [Route("jobs", Name = "searchJobs")]
        public async Task<IActionResult> Jobs(string query, long from, long to)
        {
            var searchLocation =
                new Uri(new Uri($"{Url.RouteUrl("searchJobs", null, Request.Scheme, Request.Host.Value)}/"), Guid.NewGuid().ToString());

            return await ProcessRequest(Accepted(searchLocation));
        }

        [Route("jobs/{searchJobId:guid}", Name = "getJobStatus")]
        public async Task<IActionResult> JobStatus(Guid searchJobId)
        {
            var jobStatusResponse = new SumoJobStatusResponse()
            {
                State = SumoJobStatusResponse.States[_random.Next(SumoJobStatusResponse.States.Count())],
                MessageCount = _random.Next(200, 700)
            };

            return await ProcessRequest(Ok(JsonConvert.SerializeObject(jobStatusResponse)));
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
            return await ProcessRequest(Ok(JsonConvert.SerializeObject(messageResponse)));
        }

        private async Task<IActionResult> ProcessRequest(IActionResult result)
        {
            _counter.Increase(1);

            try
            {
                if (_counter.Count > MaxRequestCount)
                {
                    var response = StatusCode(TooManyRequestHttpCode, $"Too many requests. Rate limit of {MaxRequestCount} is reached.");
                    LogMessage(LogLevel.Error, response);
                    return response;
                }

                await Task.Delay(_random.Next(500, 2000));
                LogMessage(LogLevel.Information, result);

                return result;
            }
            finally
            {
                _counter.Increase(-1);
            }
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
                    RequestCount = _counter.Count
                });
            _logger.Log(logLevel, message);
        }
    }
}
