using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Infrastructure.DomainEvents;
using EventRegistrator.Functions.Seats;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EventRegistrator.Functions.Mailing
{
    public static class SendMail
    {
        public const string SendMailCommandsQueueName = "SendMailCommands";
        public const string FallbackLanguage = "en";

        [FunctionName("SendMail")]
        public static async Task Run([ServiceBusTrigger(SendMailCommandsQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]SendMailCommand command, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: {command}");

            var logTask = DomainEventPersistor.Log(command);

            using (var context = new EventRegistratorDbContext())
            {
                var registration = context.Registrations
                                          .Where(reg => reg.Id == command.RegistrationId)
                                          .Include(reg => reg.RegistrationForm)
                                          .FirstOrDefault();
                if (registration == null)
                {
                    throw new ArgumentException($"No registration with id {command.RegistrationId}");
                }
                var templates = await context.MailTemplates.Where(mtp => mtp.EventId == registration.RegistrationForm.EventId &&
                                                                         mtp.Type == command.Type)
                                                           .ToListAsync();
                if (!templates.Any())
                {
                    throw new ArgumentException($"No template in event {registration.RegistrationForm.EventId} with type {command.Type}");
                }
                var language = "de"; // ToDo: read from registration
                var template = templates.FirstOrDefault(mtp => mtp.Language == language) ??
                               templates.FirstOrDefault(mtp => mtp.Language == FallbackLanguage) ??
                               templates.First();
                var templateFiller = new TemplateFiller(template.Template);

                var responses = await context.Responses
                                             .Where(rsp => rsp.RegistrationId == command.RegistrationId && rsp.Question.TemplateKey != null)
                                             .Select(rsp => new
                                             {
                                                 TemplateKey = rsp.Question.TemplateKey.ToUpper(),
                                                 rsp.ResponseString
                                             })
                                             .ToListAsync();

                foreach (var key in templateFiller.Parameters.Keys.ToList())
                {
                    if (key == "SEATLIST")
                    {
                        templateFiller[key] = await GetSeatList(context,
                                                                command,
                                                                responses.FirstOrDefault(rsp => rsp.TemplateKey == "ROLE")?.ResponseString,    //HACK: hardcoded
                                                                responses.FirstOrDefault(rsp => rsp.TemplateKey == "PARTNER")?.ResponseString, //HACK: hardcoded
                                                                language);
                    }
                    else
                    {
                        templateFiller[key] = responses.FirstOrDefault(rsp => rsp.TemplateKey == key)?.ResponseString;
                    }
                }

                var msg = new SendGridMessage
                {
                    From = new EmailAddress(template.SenderMail, template.SenderName),
                    Subject = template.Subject,
                };
                var content = templateFiller.Fill();
                if (template.ContentType == ContentType.Html)
                {
                    msg.HtmlContent = content;
                }
                else
                {
                    msg.PlainTextContent = content;
                }

                msg.AddTo(new EmailAddress(registration.RespondentEmail, registration.RespondentFirstName));
                if (command.RegistrationId_Partner.HasValue)
                {
                    var followerRegistration = await context.Registrations.FirstOrDefaultAsync(reg => reg.Id == command.RegistrationId_Partner);
                    if (followerRegistration != null)
                    {
                        msg.AddTo(new EmailAddress(followerRegistration.RespondentEmail, followerRegistration.RespondentFirstName));
                    }
                }
                var apiKey = Environment.GetEnvironmentVariable("SendGrid_ApiKey");
                var client = new SendGridClient(apiKey);
                var response = await client.SendEmailAsync(msg);
                await logTask;
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Accepted)
                {
                    log.Warning($"SendMail status {response.StatusCode}, Body {await response.Body.ReadAsStringAsync()}");
                }
            }
        }

        private static async Task<string> GetSeatList(EventRegistratorDbContext context, SendMailCommand command, string role, string partner, string language)
        {
            var seats = await context.Seats
                                     .Where(seat => (seat.RegistrationId == command.RegistrationId || seat.RegistrationId_Follower == command.RegistrationId)
                                                    && seat.Registrable.ShowInMailListOrder.HasValue)
                                     .OrderBy(seat => seat.Registrable.ShowInMailListOrder.Value)
                                     .Include(seat => seat.Registrable)
                                     .ToListAsync();
            return string.Join("<br />", seats.Select(seat => GetSeatText(seat, role, partner, language)));
        }

        private static string GetSeatText(Seat seat, string role, string partner, string language)
        {
            if (seat?.Registrable == null)
            {
                return "?";
            }
            if (seat.Registrable.MaximumDoubleSeats.HasValue)
            {
                // enrich info, e.g. "Lindy Hop Intermediate, Role: {role}, Partner: {email}"
                // HACK: hardcoded
                if (language == "de")
                {
                    return $"- {seat.Registrable.Name}, Rolle: {role}, Partner: {partner}";
                }
                return $"- {seat.Registrable.Name}, Role: {role}, Partner: {partner}";
            }
            return $"- {seat.Registrable.Name}";
        }
    }
}