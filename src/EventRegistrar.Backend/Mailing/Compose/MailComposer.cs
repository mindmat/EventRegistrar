using System.Text;
using System.Text.RegularExpressions;

using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Mailing.Compose;

public class MailComposer
{
    private const string DateFormat = "dd.MM.yy";
    private const string PrefixFollower = "FOLLOWER";
    private const string PrefixLeader = "LEADER";
    private readonly DuePaymentConfiguration _duePaymentConfiguration;
    private readonly PriceCalculator _priceCalculator;
    private readonly IQueryable<Event> _events;
    private readonly IQueryable<SpotMailLine> _spotMailLines;
    private readonly ILogger _log;
    private readonly PaidAmountSummarizer _paidAmountSummarizer;
    private readonly IQueryable<Registration> _registrations;

    public MailComposer(IQueryable<Registration> registrations,
                        IQueryable<Event> events,
                        IQueryable<SpotMailLine> spotMailLines,
                        ILogger log,
                        PaidAmountSummarizer paidAmountSummarizer,
                        DuePaymentConfiguration duePaymentConfiguration,
                        PriceCalculator priceCalculator)
    {
        _registrations = registrations;
        _events = events;
        _spotMailLines = spotMailLines;
        _log = log;
        _paidAmountSummarizer = paidAmountSummarizer;
        _duePaymentConfiguration = duePaymentConfiguration;
        _priceCalculator = priceCalculator;
    }

    public async Task<string> Compose(Guid registrationId,
                                      string template,
                                      string language,
                                      CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == registrationId)
                                               .Include(reg => reg.Seats_AsLeader!)
                                               .ThenInclude(seat => seat.Registrable)
                                               .Include(reg => reg.Seats_AsFollower)
                                               .ThenInclude(seat => seat.Registrable)
                                               .Include(reg => reg.Responses)
                                               .ThenInclude(rsp => rsp.Question)
                                               .Include(reg => reg.Cancellations)
                                               .Include(reg => reg.Mails)
                                               .ThenInclude(map => map.Mail)
                                               .FirstAsync(cancellationToken);

        var mainRegistrationRole = registration.Seats_AsFollower!.Any(spt => !spt.IsCancelled)
                                       ? Role.Follower
                                       : Role.Leader;
        Registration? leaderRegistration = null;
        Registration? followerRegistration = null;
        var templateFiller = new TemplateFiller(template);
        Registration? partnerRegistration = null;
        if (templateFiller.Prefixes.Any())
        {
            _log.LogInformation($"Prefixes {string.Join(",", templateFiller.Prefixes)}");
            partnerRegistration = await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner)
                                                      .Include(reg => reg.Seats_AsLeader)
                                                      .ThenInclude(seat => seat.Registrable)
                                                      .Include(reg => reg.Seats_AsFollower)
                                                      .ThenInclude(seat => seat.Registrable)
                                                      .Include(reg => reg.Responses)
                                                      .ThenInclude(rsp => rsp.Question)
                                                      .Include(reg => reg.Cancellations)
                                                      .Include(reg => reg.Mails)
                                                      .ThenInclude(map => map.Mail)
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

            var registrationForPrefix = parts.prefix switch
            {
                PrefixLeader   => leaderRegistration,
                PrefixFollower => followerRegistration,
                _              => registration
            };

            if (Enum.TryParse<MailPlaceholder>(parts.key, true, out var placeholderKey)
             || parts.key?.ToUpperInvariant() == "SEATLIST"
             || parts.key?.ToUpperInvariant() == "PARTNER")
            {
                if (placeholderKey == MailPlaceholder.FirstName)
                {
                    templateFiller[key] = registrationForPrefix?.RespondentFirstName;
                }
                else if (placeholderKey == MailPlaceholder.LastName)
                {
                    templateFiller[key] = registrationForPrefix?.RespondentLastName;
                }
                else if (placeholderKey == MailPlaceholder.Phone)
                {
                    templateFiller[key] = registrationForPrefix?.Phone;
                }
                else if ((placeholderKey == MailPlaceholder.SpotList || parts.key.ToUpperInvariant() == "SEATLIST") && registrationForPrefix != null)
                {
                    templateFiller[key] = await GetSpotList(registrationForPrefix, language);
                }
                else if (placeholderKey == MailPlaceholder.PartnerName || parts.key?.ToUpperInvariant() == "PARTNER")
                {
                    templateFiller[key] = registrationForPrefix?.PartnerOriginal;
                }
                else if (placeholderKey == MailPlaceholder.Price)
                {
                    var price = parts.prefix == null
                                    ? registration.Price_AdmittedAndReduced + (partnerRegistration?.Price_AdmittedAndReduced ?? 0m)
                                    : (registrationForPrefix ?? registration).Price_AdmittedAndReduced;

                    templateFiller[key] = price.ToString("F2"); // HACK: format hardcoded
                }
                else if (placeholderKey == MailPlaceholder.PaidAmount)
                {
                    templateFiller[key] = (await _paidAmountSummarizer.GetPaidAmount((registrationForPrefix ?? registration).Id))
                        .ToString("F2"); // HACK: format hardcoded
                }
                else if (placeholderKey is MailPlaceholder.DueAmount or MailPlaceholder.OverpaidAmount)
                {
                    var paid = await _paidAmountSummarizer.GetPaidAmount((registrationForPrefix ?? registration).Id);
                    var price = parts.prefix == null
                                    ? registration.Price_AdmittedAndReduced + (partnerRegistration?.Price_AdmittedAndReduced ?? 0m)
                                    : (registrationForPrefix ?? registration).Price_AdmittedAndReduced;
                    var difference = price - paid;
                    if (placeholderKey == MailPlaceholder.OverpaidAmount)
                    {
                        difference = -difference;
                    }

                    templateFiller[key] = difference.ToString("F2"); // HACK: format hardcoded
                }
                else if (placeholderKey == MailPlaceholder.UnpaidAmount)
                {
                    var currency = (await _events.FirstAsync(evt => evt.Id == registration.EventId, cancellationToken))
                        ?.Currency;
                    var unpaidAmount = registration.Price_AdmittedAndReduced
                                     - await _paidAmountSummarizer.GetPaidAmount(registration.Id)
                                     + (partnerRegistration == null
                                            ? 0m
                                            : partnerRegistration.Price_AdmittedAndReduced - await _paidAmountSummarizer.GetPaidAmount(partnerRegistration.Id));
                    if (unpaidAmount > 0m)
                    {
                        templateFiller[key] =
                            $" Please transfer the remaining {unpaidAmount:F2}{currency} today or pay at the checkin (ignore this message if you already paid)."; // HACK: format hardcoded
                    }
                }
                else if (placeholderKey == MailPlaceholder.CancellationReason)
                {
                    var cancellation = registration.Cancellations!.FirstOrDefault();

                    if (cancellation != null)
                    {
                        templateFiller[key] = cancellation.Reason;
                    }
                }
                else if (placeholderKey == MailPlaceholder.AcceptedDate && registration.AdmittedAt != null)
                {
                    templateFiller[key] = registration.AdmittedAt.Value.ToString(DateFormat);
                }
                else if (placeholderKey == MailPlaceholder.Reminder1Date)
                {
                    var reminder1Date = registration.Mails!
                                                    .Where(map => map.Mail!.Type != null
                                                               && _duePaymentConfiguration.MailTypes_Reminder1.Contains(
                                                                      map.Mail.Type.Value)
                                                               && map.Mail.Sent.HasValue)
                                                    .Select(map => map.Mail!.Sent)
                                                    .FirstOrDefault();
                    if (reminder1Date.HasValue)
                    {
                        templateFiller[key] = reminder1Date.Value.ToString(DateFormat);
                    }
                }
                else if (parts.key != null && key != null && registrationForPrefix?.Responses != null)
                {
                    // check responses with Question.TemplateKey
                    templateFiller[key] = registrationForPrefix.Responses.FirstOrDefault(rsp =>
                                                                                             string.Equals(rsp.Question?.TemplateKey, parts.key,
                                                                                                           StringComparison.InvariantCultureIgnoreCase))
                                                               ?.ResponseString;
                }
            }
        }

        var content = templateFiller.Fill();
        return content;
    }

    private static (string? prefix, string? key) GetPrefix(string key)
    {
        var parts = key?.Split('.');
        return parts?.Length > 1
                   ? (parts[0], parts[1])
                   : (null, key);
    }

    private string GetSpotText(Seat seat,
                               Role role,
                               string language,
                               ICollection<Response> responses)
    {
        if (seat?.Registrable == null)
        {
            return "?";
        }

        var text = _spotMailLines
                   .FirstOrDefault(sml => sml.RegistrableId == seat.RegistrableId && sml.Language == language)
                   ?.Text;
        if (text != null)
        {
            if (!text.StartsWith("- "))
            {
                text = "- " + text;
            }

            text = text.Replace("{Name}", seat.Registrable.DisplayName, StringComparison.InvariantCultureIgnoreCase)
                       .Replace("{Role}", role.ToString(), StringComparison.InvariantCultureIgnoreCase);
            text = Regex.Replace(text, "{.*?}", mtc => FillResponse(mtc, responses));
        }
        else
        {
            text = $"- {seat.Registrable.DisplayName}";
            if (seat.Registrable.MaximumDoubleSeats.HasValue)
            {
                // enrich info, e.g. "Lindy Hop Intermediate, Role: {role}, Partner: {email}"
                // HACK: hardcoded
                //var role = responses.Lookup("ROLE", "?");
                //var partner = responses.Lookup("PARTNER", "?");
                if (language == Language.German)
                {
                    text += $", Rolle: {role}"; // + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
                }
                else
                {
                    text += $", Role: {role}"; // + (seat.PartnerEmail == null ? string.Empty : $", Partner: {partner}");
                }
            }
        }

        if (seat.IsCancelled)
        {
            text += language == Language.German ? " (storniert)" : " (cancelled)";
        }

        if (seat.IsWaitingList)
        {
            text += language == Language.German ? " (Warteliste)" : " (waiting list)";
        }

        return text;
    }

    private string? FillResponse(Match match, ICollection<Response> responses)
    {
        var questionId = Guid.Parse(match.Value);
        var response = responses.FirstOrDefault(rsp => rsp.QuestionId == questionId)?.ResponseString;
        return response;
    }

    private async Task<string> GetSpotList(Registration registration, string language)
    {
        var (priceOriginal, priceAdmitted, _, packagesOriginal, packagesAdmitted) = await _priceCalculator.CalculatePrice(registration.Id);

        var result = new StringBuilder();
        result.AppendLine("<table>");
        result.AppendLine("<tbody>");
        foreach (var package in packagesAdmitted)
        {
            // Package header
            result.AppendLine("<tr>");
            result.AppendLine($"<td><strong>{package.Name}</strong></td>");
            result.AppendLine($"<td style=\"text-align: right;\"><strong>{package.Price}</strong></td>");
            result.AppendLine("</tr>");

            // Package content
            foreach (var matchingPackageSpot in package.Spots)
            {
                result.AppendLine("<tr>");
                result.AppendLine($"<td>- {matchingPackageSpot.Name}</td>");
                result.AppendLine($"<td style=\"text-align: right;\">{matchingPackageSpot.PriceAdjustment?.ToString("F2")}</td>");
                result.AppendLine("</tr>");
            }
        }

        result.AppendLine("</tbody>");
        result.AppendLine("</table>");

        return result.ToString();
        var seatLines = new List<(int sortKey, string seatLine)>();
        if (registration.Seats_AsLeader != null)
        {
            seatLines.AddRange(registration.Seats_AsLeader
                                           .Where(seat => seat.Registrable.ShowInMailListOrder.HasValue
                                                       && !seat.IsCancelled)
                                           .Select(seat => (seat.Registrable.ShowInMailListOrder ?? int.MaxValue,
                                                               GetSpotText(seat, Role.Leader, language, registration.Responses))));
        }

        if (registration.Seats_AsFollower != null)
        {
            seatLines.AddRange(registration.Seats_AsFollower
                                           .Where(seat => seat.Registrable.ShowInMailListOrder.HasValue
                                                       && !seat.IsCancelled)
                                           .Select(seat => (seat.Registrable.ShowInMailListOrder ?? int.MaxValue,
                                                               GetSpotText(seat, Role.Follower, language, registration.Responses))));
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