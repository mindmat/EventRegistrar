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
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Seats;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Response = EventRegistrator.Functions.Registrations.Response;

namespace EventRegistrator.Functions.Mailing
{
    public static class SendMail
    {
        public const string SendMailCommandsQueueName = "SendMailCommands";
        public const string FallbackLanguage = Language.English;
        private const string PrefixLeader = "LEADER";
        private const string PrefixFollower = "FOLLOWER";

        [FunctionName("SendMail")]
        public static async Task Run([ServiceBusTrigger(SendMailCommandsQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]SendMailCommand command, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function processed message: RegistrationId {command.RegistrationId}, RegistrationId_Partner {command.RegistrationId_Partner}, MainRegistrationRole {command.MainRegistrationRole}, Type {command.Type}");

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
                var language = registration.Language ?? FallbackLanguage;
                var template = templates.FirstOrDefault(mtp => mtp.Language == language) ??
                               templates.FirstOrDefault(mtp => mtp.Language == FallbackLanguage) ??
                               templates.First();

                IDictionary<string, string> responses = await GetResponses(command.RegistrationId, context.Responses);
                IDictionary<string, string> leaderResponses = null;
                IDictionary<string, string> followerResponses = null;
                Registration leaderRegistration = null;
                Registration followerRegistration = null;
                var templateFiller = new TemplateFiller(template.Template);
                if (templateFiller.Prefixes.Any())
                {
                    log.Info($"Prefixes {string.Join(",", templateFiller.Prefixes)}");
                    log.Info($"command.MainRegistrationRole {command.MainRegistrationRole}");

                    var partnerResponses = await GetResponses(command.RegistrationId_Partner, context.Responses);
                    var partnerRegistration = await context.Registrations.FirstOrDefaultAsync(reg => reg.Id == command.RegistrationId_Partner);
                    if (command.MainRegistrationRole == Role.Leader)
                    {
                        leaderResponses = responses;
                        followerResponses = partnerResponses;

                        leaderRegistration = registration;
                        followerRegistration = partnerRegistration;
                    }
                    else
                    {
                        leaderResponses = partnerResponses;
                        followerResponses = responses;

                        leaderRegistration = partnerRegistration;
                        followerRegistration = registration;
                    }
                }

                foreach (var key in templateFiller.Parameters.Keys.ToList())
                {
                    var parts = GetPrefix(key);

                    var responsesForPrefix = responses;
                    var registrationIdForPrefix = command.RegistrationId;
                    var registrationForPrefix = registration;
                    if (parts.prefix == PrefixLeader)
                    {
                        responsesForPrefix = leaderResponses;
                        registrationForPrefix = leaderRegistration;
                        registrationIdForPrefix = command.MainRegistrationRole == Role.Leader
                            ? command.RegistrationId
                            : command.RegistrationId_Partner;
                    }
                    else if (parts.prefix == PrefixFollower)
                    {
                        responsesForPrefix = followerResponses;
                        registrationForPrefix = followerRegistration;
                        registrationIdForPrefix = command.MainRegistrationRole == Role.Follower
                            ? command.RegistrationId
                            : command.RegistrationId_Partner;
                    }
                    if (parts.key == "SEATLIST")
                    {
                        templateFiller[key] = await GetSeatList(context,
                                                                registrationIdForPrefix,
                                                                responsesForPrefix,
                                                                language,
                                                                log);
                    }
                    else if (parts.key == "PRICE")
                    {
                        templateFiller[key] = (registrationForPrefix?.Price ?? 0m).ToString("F2"); // HACK: hardcoded
                    }
                    else
                    {
                        templateFiller[key] = responsesForPrefix.Lookup(parts.key);
                    }
                }

                var msg = new SendGridMessage
                {
                    From = new EmailAddress(template.SenderMail, template.SenderName),
                    Subject = template.Subject,
                };
                log.Info(string.Join(Environment.NewLine, templateFiller.Parameters.Select(par => $"{par.Key}: {par.Value}")));
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
                await DomainEventPersistor.Log(new MailSent(msg), command.RegistrationId);
            }
        }

        private static async Task<Dictionary<string, string>> GetResponses(Guid? registrationId, IQueryable<Response> responses)
        {
            return (await responses
                          .Where(rsp => rsp.RegistrationId == registrationId && rsp.Question.TemplateKey != null)
                          .Select(rsp => new
                          {
                              TemplateKey = rsp.Question.TemplateKey.ToUpper(),
                              rsp.ResponseString
                          })
                          .ToListAsync()
                  )
                  .ToDictionary(tmp => tmp.TemplateKey.ToUpper(), tmp => tmp.ResponseString);
        }

        private static (string prefix, string key) GetPrefix(string key)
        {
            var parts = key?.Split('.');
            if (parts?.Length > 1)
            {
                return (parts[0], parts[1]);
            }
            return (null, key);
        }

        private static async Task<string> GetSeatList(EventRegistratorDbContext context, Guid? registrationId, IDictionary<string, string> responses, string language, TraceWriter log)
        {
            var seats = await context.Seats
                                     .Where(seat => (seat.RegistrationId == registrationId || seat.RegistrationId_Follower == registrationId)
                                                    && seat.Registrable.ShowInMailListOrder.HasValue)
                                     .OrderBy(seat => seat.Registrable.ShowInMailListOrder.Value)
                                     .Include(seat => seat.Registrable)
                                     .ToListAsync();
            log.Info($"Seat count: {seats.Count}");
            var seatList = string.Join("<br />", seats.Select(seat => GetSeatText(seat, responses, language)));
            log.Info($"seat list {seatList}");
            return seatList;
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
                var role = responses.Lookup("ROLE", "?");
                var partner = responses.Lookup("PARTNER", "?");
                if (language == Language.Deutsch)
                {
                    return $"- {seat.Registrable.Name}, Rolle: {role}" + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
                }
                return $"- {seat.Registrable.Name}, Role: {role}" + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
            }
            return $"- {seat.Registrable.Name}";
        }
    }
}