﻿using System.Globalization;
using System.Text;

using Codecrete.SwissQRBill.Generator;

using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Price;


namespace EventRegistrar.Backend.Mailing.Compose;

public class MailComposer
{
    private const string DateFormat = "dd.MM.yy";
    private const string PrefixFollower = "FOLLOWER";
    private const string PrefixLeader = "LEADER";
    private readonly DuePaymentConfiguration _duePaymentConfiguration;
    private readonly PriceCalculator _priceCalculator;
    private readonly QrBillConfiguration _qrBillConfiguration;
    private readonly ILogger _log;
    private readonly PaidAmountSummarizer _paidAmountSummarizer;
    private readonly IQueryable<Registration> _registrations;

    public MailComposer(IQueryable<Registration> registrations,
                        ILogger log,
                        PaidAmountSummarizer paidAmountSummarizer,
                        DuePaymentConfiguration duePaymentConfiguration,
                        PriceCalculator priceCalculator,
                        QrBillConfiguration qrBillConfiguration)
    {
        _registrations = registrations;
        _log = log;
        _paidAmountSummarizer = paidAmountSummarizer;
        _duePaymentConfiguration = duePaymentConfiguration;
        _priceCalculator = priceCalculator;
        _qrBillConfiguration = qrBillConfiguration;
    }

    public async Task<string> Compose(Guid registrationId,
                                      string template,
                                      string language,
                                      CancellationToken cancellationToken)
    {
        var cultureInfoBefore = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(language);
        var registration = await _registrations.Where(reg => reg.Id == registrationId)
                                               .Include(reg => reg.Seats_AsLeader!)
                                               .ThenInclude(seat => seat.Registrable)
                                               .Include(reg => reg.Seats_AsFollower!)
                                               .ThenInclude(seat => seat.Registrable)
                                               .Include(reg => reg.Responses!)
                                               .ThenInclude(rsp => rsp.Question)
                                               .Include(reg => reg.Cancellations)
                                               .Include(reg => reg.Mails!)
                                               .ThenInclude(map => map.Mail)
                                               .Include(reg => reg.Event)
                                               .FirstAsync(cancellationToken);

        var mainRegistrationRole = registration.Seats_AsFollower!.Any(spt => !spt.IsCancelled)
                                       ? Role.Follower
                                       : Role.Leader;
        Registration? leaderRegistration = null;
        Registration? followerRegistration = null;
        var templateFiller = new TemplateFiller(template);
        Registration? partnerRegistration = null;
        var @event = registration.Event!;

        if (templateFiller.Prefixes.Any())
        {
            _log.LogInformation($"Prefixes {string.Join(",", templateFiller.Prefixes)}");
            partnerRegistration = await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner)
                                                      .Include(reg => reg.Seats_AsLeader!)
                                                      .ThenInclude(seat => seat.Registrable)
                                                      .Include(reg => reg.Seats_AsFollower!)
                                                      .ThenInclude(seat => seat.Registrable)
                                                      .Include(reg => reg.Responses!)
                                                      .ThenInclude(rsp => rsp.Question)
                                                      .Include(reg => reg.Cancellations)
                                                      .Include(reg => reg.Mails!)
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
                else if ((placeholderKey == MailPlaceholder.SpotList
                       || parts.key.ToUpperInvariant() == "SEATLIST")
                      && registrationForPrefix != null)
                {
                    templateFiller[key] = await GetSpotList(registrationForPrefix.Id, registrationForPrefix.SoldOutMessage);
                }
                else if (placeholderKey == MailPlaceholder.PartnerName
                      || parts.key?.ToUpperInvariant() == "PARTNER")
                {
                    templateFiller[key] = registrationForPrefix?.PartnerOriginal;
                }
                else if (placeholderKey == MailPlaceholder.Comments)
                {
                    templateFiller[key] = registrationForPrefix?.Remarks?.ReplaceLineEndings("<br/>") ?? string.Empty;
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
                    var currency = @event.Currency;
                    var unpaidAmount = await GetUnpaidAmount(registration, partnerRegistration);
                    if (unpaidAmount > 0m)
                    {
                        templateFiller[key] =
                            //    $" Please transfer the remaining {unpaidAmount:F2}{currency} today or pay at the checkin (ignore this message if you have already paid)."; // HACK: format hardcoded
                            $" Please pay the remaining {unpaidAmount:F2}{currency} at the checkin"; // HACK: format hardcoded
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
                else if (placeholderKey == MailPlaceholder.FormsTimestamp)
                {
                    templateFiller[key] = registration.ExternalTimestamp.ToString(DateFormat);
                }
                else if (placeholderKey == MailPlaceholder.ReceivedAt)
                {
                    templateFiller[key] = registration.ReceivedAt.ToString(DateFormat);
                }
                else if (placeholderKey == MailPlaceholder.AcceptedDate && registration.AdmittedAt != null)
                {
                    templateFiller[key] = registration.AdmittedAt.Value.ToString(DateFormat);
                }
                else if (placeholderKey == MailPlaceholder.ReadableId)
                {
                    templateFiller[key] = registration.ReadableIdentifier;
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
                else if (placeholderKey == MailPlaceholder.QrCode)
                {
                    templateFiller[key] = await GenerateQrCode(registration);
                }
            }
            else if (parts.key != null && key != null && registrationForPrefix?.Responses != null)
            {
                // check responses with Question.TemplateKey
                templateFiller[key] = registrationForPrefix.Responses.FirstOrDefault(rsp => string.Equals(rsp.Question?.TemplateKey, parts.key,
                                                                                                          StringComparison.InvariantCultureIgnoreCase))
                                                           ?.ResponseString;
            }
        }

        var content = templateFiller.Fill();
        CultureInfo.CurrentUICulture = cultureInfoBefore;
        return content;
    }

    private async Task<decimal> GetUnpaidAmount(Registration registration, Registration? partnerRegistration)
    {
        return registration.Price_AdmittedAndReduced
             - await _paidAmountSummarizer.GetPaidAmount(registration.Id)
             + (partnerRegistration == null
                    ? 0m
                    : partnerRegistration.Price_AdmittedAndReduced - await _paidAmountSummarizer.GetPaidAmount(partnerRegistration.Id));
    }

    private async Task<string?> GenerateQrCode(Registration registration)
    {
        var unpaidAmount = await GetUnpaidAmount(registration, null);
        var bill = new Bill
                   {
                       // creditor data
                       Account = _qrBillConfiguration.Iban,
                       Creditor = new Address
                                  {
                                      Name = _qrBillConfiguration.AccountHolderName,
                                      Street = _qrBillConfiguration.AccountHolderStreet,
                                      HouseNo = _qrBillConfiguration.AccountHolderHouseNo,
                                      PostalCode = _qrBillConfiguration.AccountHolderPostalCode,
                                      Town = _qrBillConfiguration.AccountHolderTown,
                                      CountryCode = _qrBillConfiguration.AccountHolderCountryCode
                                  },

                       // payment data
                       Amount = unpaidAmount,
                       Currency = "CHF",

                       // debtor data
                       Debtor = new Address
                                {
                                    Name = $"{registration.RespondentFirstName} {registration.RespondentLastName}",
                                    PostalCode = "3000",
                                    Town = "Bern",
                                    CountryCode = "CH"
                                },

                       // more payment data
                       UnstructuredMessage = registration.ReadableIdentifier,
                       //Reference = "RF19 2320 QF02 T323 4UI2 34",

                       // output format
                       Format = new BillFormat
                                {
                                    Language = Language.DE,
                                    GraphicsFormat = GraphicsFormat.PNG,
                                    OutputSize = OutputSize.QrCodeOnly
                                }
                   };

        // Generate QR bill
        var png = QRBill.Generate(bill);

        // Convert byte[] to Base64 String
        var pngBase64 = Convert.ToBase64String(png);
        return $"""<img alt="QR-Code" style="width: 200px; height: 200px" src="data:image/png;base64,{pngBase64}" />""";
    }

    private static (string? prefix, string? key) GetPrefix(string key)
    {
        var parts = key?.Split('.');
        return parts?.Length > 1
                   ? (parts[0], parts[1])
                   : (null, key);
    }

    private async Task<string> GetSpotList(Guid registrationId, string? soldOutMessage)
    {
        var (_, _, priceAdmittedAndReduced, packagesOriginal, packagesAdmitted, _, _) = await _priceCalculator.CalculatePrice(registrationId);
        var result = new StringBuilder();

        // Label
        result.AppendLine($"<p>{Resources.SpotListLabelAccepted}</p>");

        // Admitted
        if (packagesAdmitted.Any())
        {
            result.AppendLine("<table>");
            result.AppendLine("<tbody>");
            foreach (var package in packagesAdmitted)
            {
                // Package header
                var price = package.Price - package.Spots.Sum(spot => spot.PriceAdjustment ?? 0m);
                result.AppendLine("<tr>");
                result.AppendLine($"<td><strong>{package.Name}</strong></td>");
                if (package.IsReductionsPackage)
                {
                    result.AppendLine("<td></td>");
                }
                else
                {
                    result.AppendLine($"<td style=\"text-align: right;\">{price}</td>");
                }

                result.AppendLine("</tr>");

                // Package content
                foreach (var matchingPackageSpot in package.Spots.Where(spt => spt.SortKey != null).OrderBy(spt => spt.SortKey))
                {
                    result.AppendLine("<tr>");
                    result.AppendLine($"<td>- {matchingPackageSpot.Name}</td>");
                    result.AppendLine($"<td style=\"text-align: right;\">{matchingPackageSpot.PriceAdjustment?.ToString("F2")}</td>");
                    result.AppendLine("</tr>");
                }
            }

            // Total
            result.AppendLine("<tr>");
            result.AppendLine($"<td><strong>{Resources.Total}</strong></td>");
            result.AppendLine($"<td style=\"text-align: right;\"><strong>{priceAdmittedAndReduced.ToString("F2")}</strong></td>");
            result.AppendLine("</tr>");

            // payments
            var paid = await _paidAmountSummarizer.GetPaidAmount(registrationId);
            if (paid > 0)
            {
                // paid
                result.AppendLine("<tr>");
                result.AppendLine($"<td>{Resources.Paid}</td>");
                result.AppendLine($"<td style=\"text-align: right;\">{paid.ToString("F2")}</td>");
                result.AppendLine("</tr>");

                if (paid < priceAdmittedAndReduced)
                {
                    // remaining amount
                    result.AppendLine("<tr>");
                    result.AppendLine($"<td>{Resources.MissingAmount}</td>");
                    result.AppendLine($"<td style=\"text-align: right;\">{(priceAdmittedAndReduced - paid).ToString("F2")}</td>");
                    result.AppendLine("</tr>");
                }
            }

            result.AppendLine("</tbody>");
            result.AppendLine("</table>");
        }
        else
        {
            result.AppendLine("<p>-</p>");
        }


        // Waiting list
        var packagesOnWaitingList = packagesOriginal.ExceptBy(packagesAdmitted.Select(pkg => pkg.Id), pkg => pkg.Id)
                                                    .ToList();
        if (packagesOnWaitingList.Any())
        {
            // Label
            result.AppendLine("<br/>");
            result.AppendLine($"<p>{Resources.SpotListLabelWaitingList}</p>");

            // Table
            result.AppendLine("<table>");
            result.AppendLine("<tbody>");
            foreach (var package in packagesOnWaitingList)
            {
                // Package header
                var price = package.Price - package.Spots.Sum(spot => spot.PriceAdjustment ?? 0m);
                result.AppendLine("<tr>");
                result.AppendLine($"<td><strong>{package.Name}</strong></td>");
                result.AppendLine($"<td style=\"text-align: right;\"><strong>{price}</strong></td>");
                result.AppendLine("</tr>");

                // Package content
                foreach (var matchingPackageSpot in package.Spots.Where(spt => spt.SortKey != null).OrderBy(spt => spt.SortKey))
                {
                    result.AppendLine("<tr>");
                    result.AppendLine($"<td>- {matchingPackageSpot.Name}</td>");
                    result.AppendLine($"<td style=\"text-align: right;\">{matchingPackageSpot.PriceAdjustment?.ToString("F2")}</td>");
                    result.AppendLine("</tr>");
                }
            }

            result.AppendLine("</tbody>");
            result.AppendLine("</table>");
        }

        if (!string.IsNullOrWhiteSpace(soldOutMessage))
        {
            result.AppendLine();
            result.AppendLine();
            foreach (var line in soldOutMessage.Split("\r\n").SelectMany(split => split.Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries)))
            {
                result.AppendLine(line);
            }
        }

        return result.ToString();
    }
}