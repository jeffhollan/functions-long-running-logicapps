using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace HttpToDurable
{
    public static class HttpTrigger
    {
        [FunctionName("HttpTrigger_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            TraceWriter log)
        {
            // Function input comes from the request content.
            ProcessRequest requestData = await req.Content.ReadAsAsync<ProcessRequest>();
            string instanceId = await starter.StartNewAsync("HttpTrigger_Orchestrator", requestData);

            log.Info($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("HttpTrigger_Orchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("HttpTrigger_DoWork", context.GetInput<ProcessRequest>()));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("HttpTrigger_DoWork")]
        public static string DoWork([ActivityTrigger] ProcessRequest requestData, TraceWriter log)
        {
            log.Info($"Doing work on data {requestData.data}.");
            Thread.Sleep(new TimeSpan(0, 3, 0));
            return "some response data";
        }
    }

    public class ProcessRequest
    {
        public string data { get; set; }
    }
}