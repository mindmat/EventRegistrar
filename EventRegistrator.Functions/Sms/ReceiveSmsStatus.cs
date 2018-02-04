using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Sms
{
    public static class ReceiveSmsStatus
    {
        [FunctionName("ReceiveSmsStatus")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sms/status")]
            HttpRequestMessage req, 
            TraceWriter log)
        {
            var form = await req.Content.ReadAsFormDataAsync();
            var smsSid = form["SmsSid"];
            var messageStatus = form["MessageStatus"];

            using (var dbContext = new EventRegistratorDbContext())
            {
                var sms = await dbContext.Sms.FirstOrDefaultAsync(s => s.SmsSid == smsSid);
                if (sms != null)
                {
                    sms.SmsStatus = messageStatus;
                }
                await dbContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
