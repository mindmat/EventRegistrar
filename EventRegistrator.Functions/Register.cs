using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions
{
    public static class Register
    {
        [FunctionName("Register")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registration/{id}")]HttpRequestMessage req, string id, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            //log.Info($"id: {id}");
            //log.Info($"content: {await req.Content.ReadAsStringAsync()}");

            var registration = await req.Content.ReadAsAsync<Dictionary<string, string>>();

            //log.Info(string.Join(Environment.NewLine, registration.Select(itm => $"{itm.Key} = {itm.Value}")));

            // Fetching the name from the path parameter in the request URL
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}