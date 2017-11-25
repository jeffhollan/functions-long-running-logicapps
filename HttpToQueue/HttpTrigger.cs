
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Http;
using System;

namespace HttpToQueueWebhook
{
    public static class HttpTrigger
    {
        [FunctionName("HttpTrigger")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequest req, 
            TraceWriter log,
            [Queue("process")]out ProcessRequest process)
        {
            log.Info("Webhook request from Logic Apps received.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string callbackUrl = data?.callbackUrl;

            //This will drop a message in a queue that QueueTrigger will pick up
            process = new ProcessRequest { callbackUrl = callbackUrl, data = "some data" };
            return new AcceptedResult();
        }

        public static HttpClient client = new HttpClient();

        /// <summary>
        /// Queue trigger function to pick up item and do long work. Will then invoke
        /// the callback URL to have logic app continue
        /// </summary>
        [FunctionName("QueueTrigger")]
        public static void Run([QueueTrigger("process")]ProcessRequest item, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {item.data}");
            Thread.Sleep(new TimeSpan(0, 3, 0));
            ProcessResponse result = new ProcessResponse { data = "some result data" };
            client.PostAsJsonAsync<ProcessResponse>(item.callbackUrl, result);
        }
    }

    public class ProcessRequest
    {
        public string callbackUrl { get; set; }
        public string data { get; set; }
    }

    public class ProcessResponse
    {
        public string data { get; set; }
    }

}
