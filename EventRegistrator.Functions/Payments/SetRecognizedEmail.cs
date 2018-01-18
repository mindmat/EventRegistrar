using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Payments
{
    public static class SetRecognizedEmail
    {
        [FunctionName("SetRecognizedEmail")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/{paymentIdString}/RecognizedEmail")]
            HttpRequestMessage req,
            string paymentIdString,
            TraceWriter log)
        {
            var email = await req.Content.ReadAsAsync<string>();
            if (!Guid.TryParse(paymentIdString, out var paymentId))
            {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, $"{paymentIdString} is not a guid");
            }

            log.Info($"Set Mail on id {paymentId} to {email}");

            using (var dbContext = new EventRegistratorDbContext())
            {
                var payment = await dbContext.ReceivedPayments
                                             .Where(rpy => rpy.Id == paymentId)
                                             .Include(rpy => rpy.PaymentFile)
                                             .FirstOrDefaultAsync();

                if (payment == null)
                {
                    return req.CreateErrorResponse(HttpStatusCode.NotFound, $"No payment found with id {paymentIdString}");
                }
                if (payment.RecognizedEmail != null)
                {
                    return req.CreateErrorResponse(HttpStatusCode.NotFound, $"Payment {paymentIdString} already is recognized");
                }

                payment.RecognizedEmail = email;

                await dbContext.SaveChangesAsync();

                var eventId = payment.PaymentFile?.EventId;
                if (eventId.HasValue)
                {
                    await ServiceBusClient.SendEvent(new ProcessPaymentFilesCommand {EventId = eventId.Value },ProcessPaymentFiles.ProcessPaymentFilesQueueName);
                }
            }
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
