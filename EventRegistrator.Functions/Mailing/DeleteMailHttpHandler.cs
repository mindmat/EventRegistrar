using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Mailing
{
   public static class DeleteMailHttpHandler
   {
      [FunctionName("DeleteMailHttpHandler")]
      public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mails/{maildIdString:guid}/delete")]
         HttpRequestMessage req,
         string mailIdString,
         TraceWriter log)
      {
         var mailId = Guid.Parse(mailIdString);

         using (var dbContext = new EventRegistratorDbContext())
         {
            var mailToDelete = await dbContext.Mails.FirstOrDefaultAsync(mail => mail.Id == mailId);
            if (mailToDelete != null)
            {
               dbContext.Mails.Remove(mailToDelete);
            }
            await dbContext.SaveChangesAsync();

            return req.CreateResponse(HttpStatusCode.OK);
         }
      }
   }
}
