using System;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Mailing;

namespace EventRegistrator.Functions.Sms
{
    public class TemplateParameterFinder
    {
        public string Fill(string template, Guid registrationId)
        {
            var filler = new TemplateFiller(template);
            var parameters = filler.Parameters;
            using (var dbContext = new EventRegistratorDbContext())
            {
                //dbContext.Registrations
            }
            return template;
        }
    }
}