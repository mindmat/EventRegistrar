using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure;
using EventRegistrator.Functions.Infrastructure.Bus;
using EventRegistrator.Functions.Infrastructure.DataAccess;
using EventRegistrator.Functions.Infrastructure.DomainEvents;
using EventRegistrator.Functions.Registrables;
using EventRegistrator.Functions.RegistrationForms;
using EventRegistrator.Functions.Registrations;
using EventRegistrator.Functions.Seats;
using EventRegistrator.Functions.WaitingList;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Response = EventRegistrator.Functions.Registrations.Response;

namespace EventRegistrator.Functions.Mailing
{
    public static class ComposeAndSendMailCommandHandler
    {
        public const string ComposeAndSendMailCommandsQueueName = "ComposeAndSendMailCommands";
        public const string FallbackLanguage = Language.English;
        private const string PrefixLeader = "LEADER";
        private const string PrefixFollower = "FOLLOWER";

        [FunctionName("ComposeAndSendMailCommandHandler")]
        public static async Task Run([ServiceBusTrigger(ComposeAndSendMailCommandsQueueName, AccessRights.Listen, Connection = "ServiceBusEndpoint")]ComposeAndSendMailCommand command, TraceWriter log)
        {
            log.Info($"ComposeAndSendMailCommand: RegistrationId {command.RegistrationId}");

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

                var registrables = await context.Registrables.Where(rbl => rbl.EventId == registration.RegistrationForm.EventId).ToListAsync();
                MailType? mailType = null;
                var mainRegistrationRole = Role.Leader;
                Guid? registrationId_Partner = null;
                var registrablesToCheckWaitingList = new List<Registrable>();
                if (registration.State == RegistrationState.Cancelled)
                {
                    mailType = MailType.RegistrationCancelled;
                }
                else if (registration.State == RegistrationState.Paid)
                {
                    var partnerSeat = await context.Seats.FirstOrDefaultAsync(seat => (seat.RegistrationId == command.RegistrationId ||
                                                                                       seat.RegistrationId_Follower == command.RegistrationId) &&
                                                                                      !seat.IsCancelled &&
                                                                                      seat.PartnerEmail != null);
                    log.Info($"paid, partner seat id {partnerSeat?.Id}");
                    if (partnerSeat == null)
                    {
                        // single registration
                        mailType = MailType.SingleRegistrationFullyPaid;
                    }
                    else
                    {
                        // partner registration
                        registrationId_Partner = partnerSeat.RegistrationId == command.RegistrationId ? partnerSeat.RegistrationId_Follower : partnerSeat.RegistrationId;
                        var partnerRegistrationPaid = await context.Registrations.AnyAsync(reg => reg.Id == registrationId_Partner && reg.State == RegistrationState.Paid);
                        mailType = partnerRegistrationPaid ? MailType.PartnerRegistrationFullyPaid : MailType.PartnerRegistrationFirstPaid;
                    }
                }
                else
                {
                    var seats = await context.Seats.Where(seat => seat.RegistrationId == command.RegistrationId ||
                                                                  seat.RegistrationId_Follower == command.RegistrationId)
                                                   .Include(seat => seat.Registrable)
                                                   .ToListAsync();
                    foreach (var seat in seats.OrderBy(seat => seat.Registrable.ShowInMailListOrder ?? int.MaxValue))
                    {
                        var registrable = registrables.FirstOrDefault(rbl => rbl.Id == seat.RegistrableId);
                        if (registrable == null)
                        {
                            throw new Exception($"No registrable found with Id {seat.RegistrableId}");
                        }
                        if (registrable.MaximumSingleSeats.HasValue || !registrable.MaximumDoubleSeats.HasValue && registrable.IsCore)
                        {
                            mailType = seat.IsWaitingList ? MailType.SingleRegistrationOnWaitingList :
                                                            MailType.SingleRegistrationAccepted;
                            break;
                        }
                        if (registrable.MaximumDoubleSeats.HasValue)
                        {
                            if (seat.RegistrationId_Follower == registration.RegistrationForm.Id)
                            {
                                mainRegistrationRole = Role.Follower;
                            }
                            if (seat.PartnerEmail == null)
                            {
                                // single registration for double registrable
                                mailType = seat.IsWaitingList ? MailType.SingleRegistrationOnWaitingList :
                                                                MailType.SingleRegistrationAccepted;
                                // maybe now sb can get off the waiting list
                                // HACK: this should not be here (SRP)
                                registrablesToCheckWaitingList.Add(registrable);
                            }
                            else if (seat.RegistrationId.HasValue && seat.RegistrationId_Follower.HasValue)
                            {
                                // partner registration for double registrable, both partner registered
                                mailType = seat.IsWaitingList ? MailType.DoubleRegistrationMatchedOnWaitingList :
                                                                MailType.DoubleRegistrationMatchedAndAccepted;
                                registrationId_Partner = seat.RegistrationId == registration.Id ? seat.RegistrationId_Follower :
                                                                                                  seat.RegistrationId;
                            }
                            else
                            {
                                // partner registration for double registrable, both partner registered
                                mailType = seat.IsWaitingList ? MailType.DoubleRegistrationFirstPartnerOnWaitingList :
                                                                MailType.DoubleRegistrationFirstPartnerAccepted;
                            }

                            break;
                        }
                    }
                    if (!seats.Any(seat => seat.Registrable.IsCore))
                    {
                        // no core seats -> sold out
                        mailType = MailType.SoldOut;
                    }
                }

                if (mailType == null)
                {
                    log.Info("no action needed");
                    return;
                }

                // Avoid duplicate mails
                if (!command.AllowDuplicate)
                {
                    var duplicate = await context.Mails.FirstOrDefaultAsync(ml => ml.Type == mailType.Value && ml.Registrations.Any(map => map.RegistrationId == command.RegistrationId));
                    if (duplicate != null)
                    {
                        log.Info($"Duplicate Mail found, Id {duplicate.Id}, Mail type {mailType}");
                        return;
                    }
                }

                var templates = await context.MailTemplates.Where(mtp => mtp.EventId == registration.RegistrationForm.EventId &&
                                                                         mtp.Type == mailType.Value)
                                                           .ToListAsync();
                if (!templates.Any())
                {
                    throw new ArgumentException($"No template in event {registration.RegistrationForm.EventId} with type {mailType}");
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
                Registration partnerRegistration = null;
                if (templateFiller.Prefixes.Any())
                {
                    log.Info($"Prefixes {string.Join(",", templateFiller.Prefixes)}");
                    log.Info($"mainRegistrationRole {mainRegistrationRole}");

                    var partnerResponses = await GetResponses(registrationId_Partner, context.Responses);
                    partnerRegistration = await context.Registrations.FirstOrDefaultAsync(reg => reg.Id == registrationId_Partner);
                    if (mainRegistrationRole == Role.Leader)
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
                    var registrationForPrefix = registration;
                    if (parts.prefix == PrefixLeader)
                    {
                        responsesForPrefix = leaderResponses;
                        registrationForPrefix = leaderRegistration;
                    }
                    else if (parts.prefix == PrefixFollower)
                    {
                        responsesForPrefix = followerResponses;
                        registrationForPrefix = followerRegistration;
                    }
                    if (parts.key == "SEATLIST")
                    {
                        templateFiller[key] = await GetSeatList(context,
                                                                registrationForPrefix,
                                                                responsesForPrefix,
                                                                language,
                                                                log);
                    }
                    else if (parts.key == "PRICE")
                    {
                        templateFiller[key] = (registrationForPrefix?.Price ?? 0m).ToString("F2"); // HACK: format hardcoded
                    }
                    else
                    {
                        templateFiller[key] = responsesForPrefix.Lookup(parts.key);
                    }
                }

                log.Info("Parameters: " + string.Join(Environment.NewLine, templateFiller.Parameters.Select(par => $"{par.Key}: {par.Value}")));
                var content = templateFiller.Fill();

                var mappings = new List<Registration> { registration };
                if (registrationId_Partner.HasValue && partnerRegistration != null)
                {
                    mappings.Add(partnerRegistration);
                }

                var mail = new Mail
                {
                    Id = Guid.NewGuid(),
                    Type = mailType,
                    SenderMail = template.SenderMail,
                    SenderName = template.SenderName,
                    Subject = template.Subject,
                    Recipients = string.Join(";", mappings.Select(reg => reg.RespondentEmail)),
                    Withhold = command.Withhold,
                    Created = DateTime.UtcNow
                };

                if (template.ContentType == ContentType.Html)
                {
                    mail.ContentHtml = content;
                }
                else
                {
                    mail.ContentPlainText = content;
                }

                context.Mails.Add(mail);
                context.MailToRegistrations.AddRange(mappings.Select(reg => new MailToRegistration { Id = Guid.NewGuid(), MailId = mail.Id, RegistrationId = reg.Id }));

                var sendMailCommand = new SendMailCommand
                {
                    MailId = mail.Id,
                    ContentHtml = mail.ContentHtml,
                    ContentPlainText = mail.ContentPlainText,
                    Subject = mail.Subject,
                    Sender = new EmailAddress { Email = mail.SenderMail, Name = mail.SenderName },
                    To = mappings.Select(reg => new EmailAddress { Email = reg.RespondentEmail, Name = reg.RespondentFirstName }).ToList()
                };

                await context.SaveChangesAsync();
                if (!command.Withhold)
                {
                    await ServiceBusClient.SendEvent(sendMailCommand, SendMailCommandHandler.SendMailQueueName);
                }
                foreach (var registrable in registrablesToCheckWaitingList)
                {
                    await ServiceBusClient.SendEvent(new TryPromoteFromWaitingListCommand { EventId = registrable.EventId, RegistrableId = registrable.Id }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
                }
            }
            await logTask;
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

        private static async Task<string> GetSeatList(EventRegistratorDbContext context, Registration registration, IDictionary<string, string> responses, string language, TraceWriter log)
        {
            var seats = await context.Seats
                                     .Where(seat => (seat.RegistrationId == registration.Id || seat.RegistrationId_Follower == registration.Id)
                                                    && seat.Registrable.ShowInMailListOrder.HasValue)
                                     .OrderBy(seat => seat.Registrable.ShowInMailListOrder.Value)
                                     .Include(seat => seat.Registrable)
                                     .ToListAsync();
            log.Info($"Seat count: {seats.Count}");
            var seatList = string.Join("<br />", seats.Select(seat => GetSeatText(seat, responses, language)));

            if (registration.SoldOutMessage != null)
            {
                seatList += $"<br /><br />{registration.SoldOutMessage.Replace(Environment.NewLine, "<br />")}";
            }
            log.Info($"seat list {seatList}");
            return seatList;
        }

        private static string GetSeatText(Seat seat, IDictionary<string, string> responses, string language)
        {
            if (seat?.Registrable == null)
            {
                return "?";
            }
            var text = $"- {seat.Registrable.Name}";
            if (seat.Registrable.MaximumDoubleSeats.HasValue)
            {
                // enrich info, e.g. "Lindy Hop Intermediate, Role: {role}, Partner: {email}"
                // HACK: hardcoded
                var role = responses.Lookup("ROLE", "?");
                var partner = responses.Lookup("PARTNER", "?");
                if (language == Language.Deutsch)
                {
                    text += $", Rolle: {role}" + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
                }
                else
                {
                    text += $", Role: {role}" + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
                }
            }

            if (seat.IsCancelled)
            {
                text += language == Language.Deutsch ? " (storniert)" : " (cancelled)";
            }
            return text;
        }
    }
}