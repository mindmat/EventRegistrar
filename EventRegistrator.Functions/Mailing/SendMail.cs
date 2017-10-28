using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Infrastructure.DomainEvents;
using EventRegistrator.Functions.Registrables;
using EventRegistrator.Functions.RegistrationForms;
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
        private const string PrefixLeader = "LEADER";
        private const string PrefixFollower = "FOLLOWER";

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

                IDictionary<string, string> responses = await context.Responses
                                                              .Where(rsp => rsp.RegistrationId == command.RegistrationId && rsp.Question.TemplateKey != null)
                                                              .ToDictionaryAsync(rsp => rsp.Question.TemplateKey.ToUpper(), rsp => rsp.ResponseString);
                IDictionary<string, string> leaderResponses = null;
                IDictionary<string, string> followerResponses = null;

                var templateFiller = new TemplateFiller(template.Template);
                if (templateFiller.Prefixes.Any())
                {
                    var partnerResponses = await context.Responses
                                                 .Where(rsp => rsp.RegistrationId == command.RegistrationId_Partner && rsp.Question.TemplateKey != null)
                                                 .ToDictionaryAsync(rsp => rsp.Question.TemplateKey.ToUpper(), rsp => rsp.ResponseString);
                    if (command.MainRegistrationRole == Role.Leader)
                    {
                        leaderResponses = responses;
                        followerResponses = partnerResponses;
                    }
                    if (templateFiller.Prefixes.Contains(PrefixLeader))
                    {
                        leaderResponses = partnerResponses;
                        followerResponses = responses;
                    }
                }

                foreach (var key in templateFiller.Parameters.Keys.ToList())
                {
                    var prefix = GetPrefix(key);
                    var responsesForPrefix = responses;
                    var registrationIdForPrefix = command.RegistrationId;
                    if (prefix == PrefixLeader)
                    {
                        responsesForPrefix = leaderResponses;
                        registrationIdForPrefix = command.MainRegistrationRole == Role.Leader
                            ? command.RegistrationId
                            : command.RegistrationId_Partner;
                    }
                    else if (prefix == PrefixFollower)
                    {
                        responsesForPrefix = followerResponses;
                        registrationIdForPrefix = command.MainRegistrationRole == Role.Follower
                            ? command.RegistrationId
                            : command.RegistrationId_Partner;
                    }
                    if (key == "SEATLIST")
                    {
                        templateFiller[key] = await GetSeatList(context,
                                                                registrationIdForPrefix,
                                                                responsesForPrefix,
                                                                language);
                    }
                    else
                    {
                        templateFiller[key] = responsesForPrefix.Lookup(key);
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
                // add partner mail
                if (command.RegistrationId_Partner.HasValue)
                {
                    var partnerRegistration = await context.Registrations.FirstOrDefaultAsync(reg => reg.Id == command.RegistrationId_Partner);
                    if (partnerRegistration != null)
                    {
                        msg.AddTo(new EmailAddress(partnerRegistration.RespondentEmail, partnerRegistration.RespondentFirstName));
                    }
                }

                // send mail
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

        private static string GetPrefix(string key)
        {
            var parts = key?.Split('.');
            if (parts?.Length > 1)
            {
                return parts?[0];
            }
            return null;
        }

        private static async Task<string> GetSeatList(EventRegistratorDbContext context, Guid? registrationId, IDictionary<string, string> responses, string language)
        {
            var seats = await context.Seats
                                     .Where(seat => (seat.RegistrationId == registrationId || seat.RegistrationId_Follower == registrationId)
                                                    && seat.Registrable.ShowInMailListOrder.HasValue)
                                     .OrderBy(seat => seat.Registrable.ShowInMailListOrder.Value)
                                     .Include(seat => seat.Registrable)
                                     .ToListAsync();
            return string.Join("<br />", seats.Select(seat => GetSeatText(seat, responses, language)));
        }

        private static string GetSeatText(Seat seat, IDictionary<string, string> responses, string language)
        {
            if (seat?.Registrable == null)
            {
                return "?";
            }
            if (seat.Registrable.MaximumDoubleSeats.HasValue)
            {
                // enrich info, e.g. "Lindy Hop Intermediate, Role: {role}, Partner: {email}"
                // HACK: hardcoded
                var role = responses.Lookup("ROLE");
                var partner = responses.Lookup("PARTNER");
                if (language == Language.Deutsch)
                {
                    return $"- {seat.Registrable.Name}, Rolle: {role}, Partner: {partner}";
                }
                return $"- {seat.Registrable.Name}, Role: {role}, Partner: {partner}";
            }
            return $"- {seat.Registrable.Name}";
        }
    }
}