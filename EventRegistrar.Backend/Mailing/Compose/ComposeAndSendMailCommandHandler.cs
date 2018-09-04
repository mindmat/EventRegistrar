using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Send;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class ComposeAndSendMailCommandHandler : IRequestHandler<ComposeAndSendMailCommand>
    {
        public const string FallbackLanguage = Language.Deutsch;
        private const string DateFormat = "dd.MM.yy";
        private const string PrefixFollower = "FOLLOWER";
        private const string PrefixLeader = "LEADER";

        private readonly ILogger _logger;
        private readonly IRepository<Mail> _mails;
        private readonly IRepository<MailToRegistration> _mailsToRegistrations;
        private readonly IQueryable<Registration> _registrations;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IQueryable<MailTemplate> _templates;

        public ComposeAndSendMailCommandHandler(IQueryable<MailTemplate> templates,
                                                IQueryable<Registration> registrations,
                                                IRepository<Mail> mails,
                                                IRepository<MailToRegistration> mailsToRegistrations,
                                                ILogger logger,
                                                ServiceBusClient serviceBusClient)
        {
            _templates = templates;
            _registrations = registrations;
            _mails = mails;
            _mailsToRegistrations = mailsToRegistrations;
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Unit> Handle(ComposeAndSendMailCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                   .Include(reg => reg.Seats_AsLeader).ThenInclude(seat => seat.Registrable)
                                                   .Include(reg => reg.Seats_AsFollower).ThenInclude(seat => seat.Registrable)
                                                   .FirstAsync(cancellationToken);
            var templates = await _templates.Where(mtp => mtp.EventId == registration.EventId &&
                                                          mtp.Type == command.MailType)
                                            .ToListAsync(cancellationToken);
            if (!templates.Any())
            {
                throw new ArgumentException($"No template in event {registration.EventId} with type {command.MailType}");
            }
            var language = registration.Language ?? FallbackLanguage;
            var template = templates.FirstOrDefault(mtp => mtp.Language == language) ??
                           templates.FirstOrDefault(mtp => mtp.Language == FallbackLanguage) ??
                           templates.First();

            var mainRegistrationRole = registration.Seats_AsFollower.Any() ? Role.Follower : Role.Leader;
            //IDictionary<string, string> responses = await GetResponses(command.RegistrationId, context.Responses);
            //IDictionary<string, string> leaderResponses = null;
            //IDictionary<string, string> followerResponses = null;
            Registration leaderRegistration = null;
            Registration followerRegistration = null;
            var templateFiller = new TemplateFiller(template.Template);
            Registration partnerRegistration = null;
            if (templateFiller.Prefixes.Any())
            {
                _logger.LogInformation($"Prefixes {string.Join(",", templateFiller.Prefixes)}");
                //_logger.LogInformation($"mainRegistrationRole {mainRegistrationRole}");

                //var partnerResponses = await GetResponses(registrationId_Partner, context.Responses);
                partnerRegistration = await _registrations.FirstOrDefaultAsync(reg => reg.Id == registration.RegistrationId_Partner, cancellationToken);
                if (mainRegistrationRole == Role.Leader)
                {
                    //leaderResponses = responses;
                    //followerResponses = partnerResponses;

                    leaderRegistration = registration;
                    followerRegistration = partnerRegistration;
                }
                else
                {
                    //leaderResponses = partnerResponses;
                    //followerResponses = responses;

                    leaderRegistration = partnerRegistration;
                    followerRegistration = registration;
                }
            }

            foreach (var key in templateFiller.Parameters.Keys.ToList())
            {
                var parts = GetPrefix(key);

                //var responsesForPrefix = responses;
                var registrationForPrefix = registration;
                if (parts.prefix == PrefixLeader)
                {
                    //responsesForPrefix = leaderResponses;
                    registrationForPrefix = leaderRegistration;
                }
                else if (parts.prefix == PrefixFollower)
                {
                    //responsesForPrefix = followerResponses;
                    registrationForPrefix = followerRegistration;
                }

                if (parts.key == "FIRSTNAME")
                {
                    templateFiller[key] = registrationForPrefix?.RespondentFirstName;
                }
                else if (parts.key == "LASTNAME")
                {
                    templateFiller[key] = registrationForPrefix?.RespondentLastName;
                }
                else if (parts.key == "SEATLIST")
                {
                    templateFiller[key] = await GetSeatList(registrationForPrefix, language);
                }
                else if (parts.key == "PRICE")
                {
                    var price = parts.prefix == null
                                ? (registration?.Price ?? 0m) + (partnerRegistration?.Price ?? 0m)
                                : registrationForPrefix?.Price;

                    templateFiller[key] = (price ?? 0m).ToString("F2"); // HACK: format hardcoded
                }
                //else if (parts.key == "PAIDAMOUNT")
                //{
                //    templateFiller[key] = (await GetPaidAmount(context, registrationForPrefix.Id)).ToString("F2"); // HACK: format hardcoded
                //}
                // ToDo: cancellation mail
                //else if (parts.key == "CANCELLATIONREASON")
                //{
                //    var cancellation = (await context
                //                              .RegistrationCancellations
                //                              .FirstOrDefaultAsync(rcl => rcl.RegistrationId == command.RegistrationId));

                //    if (cancellation != null)
                //    {
                //        var reason = cancellation.Reason;
                //        if (cancellation.Refund > 0m)
                //        {
                //            // HACK: hardcoded addition to template
                //            reason += language == "de"
                //                ? ". Bitte sende und deine Kontoinformationen (IBAN, Kontoinhaber, PLZ/Ort) für die Rückzahlung"
                //                : ". Please give us your bank details (IBAN, account holder name, ZIP/town) for a refund";
                //        }
                //        templateFiller[key] = reason;
                //    }
                //}
                else if (parts.key == "ACCEPTEDDATE" && registration.AdmittedAt.HasValue)
                {
                    templateFiller[key] = registration.AdmittedAt.Value.ToString(DateFormat);
                }
                //else if (parts.key == "REMINDER1DATE")
                //{
                //    var reminder1Date = await context.MailToRegistrations
                //                                     .Where(map => map.RegistrationId == command.RegistrationId &&
                //                                                   SendReminderCommandHandler.MailTypes_Reminder1.Contains(map.Mail.Type))
                //                                     .Select(map => (DateTime?)map.Mail.Created)
                //                                     .FirstOrDefaultAsync();
                //    if (reminder1Date.HasValue)
                //    {
                //        templateFiller[key] = reminder1Date.Value.ToString(DateFormat);
                //    }
                //}
                //else
                //{
                //    templateFiller[key] = responsesForPrefix.Lookup(parts.key);
                //}
            }

            var content = templateFiller.Fill();

            var mappings = new List<Registration> { registration };
            if (registration.RegistrationId_Partner.HasValue && partnerRegistration != null)
            {
                mappings.Add(partnerRegistration);
            }

            var mail = new Mail
            {
                Id = Guid.NewGuid(),
                Type = command.MailType,
                SenderMail = template.SenderMail,
                SenderName = template.SenderName,
                Subject = template.Subject,
                Recipients = string.Join(";", mappings.Select(reg => reg.RespondentEmail)),
                Withhold = command.Withhold,
                Created = DateTime.UtcNow
            };

            if (template.ContentType == MailContentType.Html)
            {
                mail.ContentHtml = content;
            }
            else
            {
                mail.ContentPlainText = content;
            }

            await _mails.InsertOrUpdateEntity(mail, cancellationToken);
            foreach (var mailToRegistration in mappings.Select(reg => new MailToRegistration { Id = Guid.NewGuid(), MailId = mail.Id, RegistrationId = reg.Id }))
            {
                await _mailsToRegistrations.InsertOrUpdateEntity(mailToRegistration, cancellationToken);
            }

            var sendMailCommand = new SendMailCommand
            {
                MailId = mail.Id,
                ContentHtml = mail.ContentHtml,
                ContentPlainText = mail.ContentPlainText,
                Subject = mail.Subject,
                Sender = new EmailAddress { Email = mail.SenderMail, Name = mail.SenderName },
                To = mappings.Select(reg => new EmailAddress { Email = reg.RespondentEmail, Name = reg.RespondentFirstName }).ToList()
            };

            if (!command.Withhold)
            {
                await _serviceBusClient.SendCommand(sendMailCommand);
            }
            // ToDo
            //foreach (var registrable in registrablesToCheckWaitingList)
            //{
            //    await _serviceBusClient.SendCommand(new TryPromoteFromWaitingListCommand { RegistrableId = registrable.Id }, TryPromoteFromWaitingList.TryPromoteFromWaitingListQueueName);
            //}
            return Unit.Value;
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

        private static string GetSeatText(Seat seat, Role role, string language)
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
                //var role = responses.Lookup("ROLE", "?");
                //var partner = responses.Lookup("PARTNER", "?");
                if (language == Language.Deutsch)
                {
                    text += $", Rolle: {role}"; // + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
                }
                else
                {
                    text += $", Role: {role}"; // + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
                }
            }

            if (seat.IsCancelled)
            {
                text += language == Language.Deutsch ? " (storniert)" : " (cancelled)";
            }
            if (seat.IsWaitingList)
            {
                text += language == Language.Deutsch ? " (Warteliste)" : " (waiting list)";
            }
            return text;
        }

        private async Task<string> GetSeatList(Registration registration, string language)
        {
            //var seats = await context.Seats
            //    .Where(seat => (seat.RegistrationId == registration.Id
            //                    || seat.RegistrationId_Follower == registration.Id)
            //                   && seat.Registrable.ShowInMailListOrder.HasValue
            //                   && !seat.IsCancelled)
            //    .OrderBy(seat => seat.Registrable.ShowInMailListOrder.Value)
            //    .Include(seat => seat.Registrable)
            //    .ToListAsync();
            //_logger.LogInformation($"Seat count: {seats.Count}");
            var seatLines = new List<(int sortKey, string seatLine)>();
            if (registration.Seats_AsLeader != null)
            {
                seatLines.AddRange(registration.Seats_AsLeader
                                               .Where(seat => seat.Registrable.ShowInMailListOrder.HasValue
                                                          && !seat.IsCancelled)
                                               .Select(seat => (seat.Registrable.ShowInMailListOrder ?? int.MaxValue,
                                                                GetSeatText(seat, Role.Leader, language))));
            }

            if (registration.Seats_AsFollower != null)
            {
                seatLines.AddRange(registration.Seats_AsFollower
                                               .Where(seat => seat.Registrable.ShowInMailListOrder.HasValue
                                                          && !seat.IsCancelled)
                                               .Select(seat => (seat.Registrable.ShowInMailListOrder ?? int.MaxValue,
                                                                GetSeatText(seat, Role.Follower, language))));
            }

            var seatList = string.Join("<br />", seatLines.OrderBy(tmp => tmp.sortKey)
                                                          .Select(tmp => tmp.seatLine));

            if (registration.SoldOutMessage != null)
            {
                seatList += $"<br /><br />{registration.SoldOutMessage.Replace(Environment.NewLine, "<br />")}";
            }

            _logger.LogInformation($"seat list {seatList}");
            return seatList;
        }
    }
}