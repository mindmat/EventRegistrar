using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;
using Microsoft.Azure.WebJobs.Host;

namespace EventRegistrator.Functions.Sms
{
    public class TemplateParameterFinder
    {
        public async Task<string> Fill(string template, Guid registrationId)
        {
            var filler = new TemplateFiller(template);
            var parameters = filler.Parameters;
            using (var dbContext = new EventRegistratorDbContext())
            {
                var registration = await dbContext.Registrations.FirstOrDefaultAsync(reg => reg.Id == registrationId);
                foreach (var key in parameters.Keys.ToList())
                {
                    if (key == "FIRSTNAME")
                    {
                        filler[key] = registration.RespondentFirstName;
                    }
                    else if (key == "LASTNAME")
                    {
                        filler[key] = registration.RespondentLastName;
                    }
                    else if (key == "EMAIL")
                    {
                        filler[key] = registration.RespondentEmail;
                    }
                }
            }
            return filler.Fill();
        }
    }
}