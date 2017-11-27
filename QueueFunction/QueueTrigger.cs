using System;
using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace QueueFunction
{
    public static class QueueTrigger
    {
        [FunctionName("QueueTrigger")]
        public static void Run(
            [QueueTrigger("request")]string logicAppRequest, 
            TraceWriter log, 
            [Queue("response")] out string logicAppResponse)
        {
            log.Info($"got the request from Logic Apps: {logicAppRequest}");
            Thread.Sleep(TimeSpan.FromMinutes(3));
            logicAppResponse = "Work Finished";
        }
    }
}
