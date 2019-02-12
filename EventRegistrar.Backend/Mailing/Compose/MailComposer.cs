using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class MailComposer
    {
        private const string DateFormat = "dd.MM.yy";
        private const string PrefixFollower = "FOLLOWER";
        private const string PrefixLeader = "LEADER";
        private readonly DuePaymentConfiguration _duePaymentConfiguration;
        private readonly IQueryable<Event> _events;
        private readonly ILogger _log;
        private readonly PaidAmountSummarizer _paidAmountSummarizer;
        private readonly IQueryable<Registration> _registrations;

        public MailComposer(IQueryable<Registration> registrations,
                            IQueryable<Event> events,
                            ILogger log,
                            PaidAmountSummarizer paidAmountSummarizer,
                            DuePaymentConfiguration duePaymentConfiguration)
        {
            _registrations = registrations;
            _events = events;
            _log = log;
            _paidAmountSummarizer = paidAmountSummarizer;
            _duePaymentConfiguration = duePaymentConfiguration;
        }

        public async Task<string> Compose(Guid registrationId, string template, string language, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == registrationId)
                                                   .Include(reg => reg.Seats_AsLeader).ThenInclude(seat => seat.Registrable)
                                                   .Include(reg => reg.Seats_AsFollower).ThenInclude(seat => seat.Registrable)
                                                   .Include(reg => reg.Responses).ThenInclude(rsp => rsp.Question)
                                                   .Include(reg => reg.Cancellations)
                                                   .Include(reg => reg.Mails).ThenInclude(map => map.Mail)
                                                   .FirstAsync(cancellationToken);

            var mainRegistrationRole = registration.Seats_AsFollower.Any(spt => !spt.IsCancelled) ? Role.Follower : Role.Leader;
            Registration leaderRegistration = null;
            Registration followerRegistration = null;
            var templateFiller = new TemplateFiller(template);
            Registration partnerRegistration = null;
            if (templateFiller.Prefixes.Any())
            {
                _log.LogInformation($"Prefixes {string.Join(",", templateFiller.Prefixes)}");
                partnerRegistration = await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner)
                                                          .Include(reg => reg.Seats_AsLeader).ThenInclude(seat => seat.Registrable)
                                                          .Include(reg => reg.Seats_AsFollower).ThenInclude(seat => seat.Registrable)
                                                          .Include(reg => reg.Responses).ThenInclude(rsp => rsp.Question)
                                                          .Include(reg => reg.Cancellations)
                                                          .Include(reg => reg.Mails).ThenInclude(map => map.Mail)
                                                          .FirstOrDefaultAsync(cancellationToken);
                if (mainRegistrationRole == Role.Leader)
                {
                    leaderRegistration = registration;
                    followerRegistration = partnerRegistration;
                }
                else
                {
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
                else if (parts.key == "PHONE")
                {
                    templateFiller[key] = registrationForPrefix?.Phone;
                }
                else if (parts.key == "SEATLIST" && registrationForPrefix != null)
                {
                    templateFiller[key] = GetSeatList(registrationForPrefix, language);
                }
                else if (parts.key == "PARTNER")
                {
                    templateFiller[key] = registrationForPrefix?.PartnerOriginal;
                }
                else if (parts.key == "PRICE")
                {
                    var price = parts.prefix == null
                                ? registration.Price + (partnerRegistration == null ? 0m : partnerRegistration.Price)
                                : (registrationForPrefix ?? registration).Price;

                    templateFiller[key] = (price ?? 0m).ToString("F2"); // HACK: format hardcoded
                }
                else if (parts.key == "PAIDAMOUNT")
                {
                    templateFiller[key] = (await _paidAmountSummarizer.GetPaidAmount((registrationForPrefix ?? registration).Id)).ToString("F2"); // HACK: format hardcoded
                }
                else if (parts.key == "UNPAIDAMOUNT")
                {
                    var currency = (await _events.FirstAsync(evt => evt.Id == registration.EventId, cancellationToken))?.Currency;
                    var unpaidAmount = (registration.Price ?? 0m) - await _paidAmountSummarizer.GetPaidAmount(registration.Id)
                                     + (partnerRegistration == null ? 0m : (partnerRegistration.Price ?? 0m) - await _paidAmountSummarizer.GetPaidAmount(partnerRegistration.Id));
                    if (unpaidAmount > 0m)
                    {
                        templateFiller[key] = $" Please transfer the remaining {unpaidAmount:F2}{currency} today or pay at the checkin (ignore this message if you already paid)."; // HACK: format hardcoded
                    }
                }
                // ToDo: cancellation mail
                else if (parts.key == "CANCELLATIONREASON")
                {
                    var cancellation = registration.Cancellations.FirstOrDefault();

                    if (cancellation != null)
                    {
                        var reason = cancellation.Reason;
                        //if (cancellation.Refund > 0m)
                        //{
                        //    // HACK: hardcoded addition to template
                        //    reason += language == "de"
                        //        ? ". Bitte sende und deine Kontoinformationen (IBAN, Kontoinhaber, PLZ/Ort) für die Rückzahlung"
                        //        : ". Please give us your bank details (IBAN, account holder name, ZIP/town) for a refund";
                        //}
                        templateFiller[key] = reason;
                    }
                }
                else if (parts.key == "ACCEPTEDDATE" && registration.AdmittedAt.HasValue)
                {
                    templateFiller[key] = registration.AdmittedAt.Value.ToString(DateFormat);
                }
                else if (parts.key == "REMINDER1DATE")
                {
                    var reminder1Date = registration.Mails
                                                    .Where(map => map.Mail.Type != null
                                                               && _duePaymentConfiguration.MailTypes_Reminder1.Contains(map.Mail.Type.Value)
                                                               && map.Mail.Sent.HasValue)
                                                    .Select(map => map.Mail.Sent)
                                                    .FirstOrDefault();
                    if (reminder1Date.HasValue)
                    {
                        templateFiller[key] = reminder1Date.Value.ToString(DateFormat);
                    }
                }
                else if (parts.key != null && key != null && registrationForPrefix?.Responses != null)
                {
                    // check responses with Question.TemplateKey
                    templateFiller[key] = registrationForPrefix.Responses.FirstOrDefault(rsp => string.Equals(rsp.Question?.TemplateKey, parts.key, StringComparison.InvariantCultureIgnoreCase))?.ResponseString;
                }
            }

            var content = templateFiller.Fill();
            return content;
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

        private string GetSeatList(Registration registration, string language)
        {
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

            _log.LogInformation($"seat list {seatList}");
            return seatList;
        }
    }
}