using System.Data.Entity;
using System.Data.Entity.Core;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.RegistrationForms
{
    public static class GetExternalIdentifiers
    {
        [FunctionName("GetExternalIdentifiers")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrationform/{formId}/ExternalIdentifiers")]
                   HttpRequestMessage req,
                   string formId,
                   TraceWriter log)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var form = await dbContext.RegistrationForms.FirstOrDefaultAsync(frm => frm.ExternalIdentifier == formId);
                if (form == null)
                {
                    throw new ObjectNotFoundException($"No form with ExternalIdentifier {formId} found");
                }

                var ids = await dbContext.Registrations.Where(reg => reg.RegistrationFormId == form.Id).Select(reg => reg.ExternalIdentifier).ToListAsync();
                log.Info($"ExternalIdentifiers: {string.Join(", ", ids)}");
                return req.CreateResponse(HttpStatusCode.OK, ids);
            }
        }
    }
}