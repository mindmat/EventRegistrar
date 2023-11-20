using System.Collections.Immutable;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Payments.Due;

public class DuePaymentsCalculator(IQueryable<Registration> registrations,
                                   IDateTimeProvider dateTimeProvider)
    : ReadModelCalculator<IEnumerable<DuePaymentItem>>
{
    public const int DefaultPaymentGracePeriod = 6;

    public static readonly ImmutableHashSet<MailType?> MailTypes_Accepted =
        new HashSet<MailType?> { MailType.PartnerRegistrationMatchedAndAccepted, MailType.SingleRegistrationAccepted }.ToImmutableHashSet();

    public static readonly ImmutableHashSet<MailType?> MailTypes_Reminder1 =
        new HashSet<MailType?> { MailType.PartnerRegistrationFirstReminder, MailType.SingleRegistrationFirstReminder }.ToImmutableHashSet();

    public static readonly ImmutableHashSet<MailType?> MailTypes_Reminder2 =
        new HashSet<MailType?> { MailType.PartnerRegistrationSecondReminder, MailType.SingleRegistrationSecondReminder }.ToImmutableHashSet();


    public override string QueryName => nameof(DuePaymentsQuery);
    public override bool IsDateDependent => true;

    public override async Task<IEnumerable<DuePaymentItem>> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.Now;
        var reminderDueFrom = now.AddDays(-DefaultPaymentGracePeriod);
        var data = await registrations.Where(reg => reg.RegistrationForm!.EventId == eventId
                                                 && reg.State == RegistrationState.Received
                                                 && reg.IsOnWaitingList != true
                                                 && reg.Price_AdmittedAndReduced > 0)
                                      .Select(reg => new
                                                     {
                                                         reg.Id,
                                                         FirstName = reg.RespondentFirstName,
                                                         LastName = reg.RespondentLastName,
                                                         Email = reg.RespondentEmail,
                                                         reg.Price_AdmittedAndReduced,
                                                         reg.ReceivedAt,
                                                         reg.PhoneNormalized,
                                                         reg.ReminderLevel,
                                                         Paid = (decimal?)reg.PaymentAssignments!.Sum(asn => asn.OutgoingPayment == null
                                                                                                                 ? asn.Amount
                                                                                                                 : -asn.Amount),
                                                         Mails = reg.Mails!.Select(rml => new
                                                                                          {
                                                                                              rml.Mail!.Id,
                                                                                              Sent = rml.Mail.Created,
                                                                                              rml.Mail.Type,
                                                                                              rml.Mail.Withhold,
                                                                                              rml.Mail.Discarded
                                                                                          }),
                                                         ReminderSms = reg.Sms!.Where(sms => sms.Type == SmsType.Reminder),
                                                         reg.WillPayAtCheckin
                                                     })
                                      .ToListAsync(cancellationToken);

        var result = data.Select(tmp => new
                                        {
                                            tmp.Id,
                                            tmp.FirstName,
                                            tmp.LastName,
                                            tmp.Email,
                                            tmp.Price_AdmittedAndReduced,
                                            tmp.ReceivedAt,
                                            tmp.PhoneNormalized,
                                            tmp.ReminderLevel,
                                            tmp.Paid,
                                            tmp.WillPayAtCheckin,
                                            AcceptedMail = tmp.Mails.Where(mail => !mail.Withhold
                                                                                && !mail.Discarded
                                                                                && MailTypes_Accepted.Contains(mail.Type))
                                                              .Select(mail => new SentMailDto
                                                                              {
                                                                                  Id = mail.Id,
                                                                                  Sent = mail.Sent
                                                                              })
                                                              .MaxBy(mail => mail.Sent),
                                            Reminder1Mail = tmp.Mails.Where(mail => !mail.Withhold
                                                                                 && !mail.Discarded
                                                                                 && MailTypes_Reminder1.Contains(mail.Type))
                                                               .Select(mail => new SentMailDto
                                                                               {
                                                                                   Id = mail.Id,
                                                                                   Sent = mail.Sent
                                                                               })
                                                               .MaxBy(mail => mail.Sent),
                                            Reminder2Mail = tmp.Mails.Where(mail => !mail.Withhold
                                                                                 && !mail.Discarded
                                                                                 && MailTypes_Reminder2.Contains(mail.Type))
                                                               .Select(mail => new SentMailDto
                                                                               {
                                                                                   Id = mail.Id,
                                                                                   Sent = mail.Sent
                                                                               })
                                                               .MaxBy(mail => mail.Sent),

                                            ReminderSms = tmp.ReminderSms.FirstOrDefault()
                                        })
                         .Select(reg => new DuePaymentItem
                                        {
                                            Id = reg.Id,
                                            FirstName = reg.FirstName,
                                            LastName = reg.LastName,
                                            Email = reg.Email,
                                            Price = reg.Price_AdmittedAndReduced,
                                            Paid = reg.Paid,
                                            ReceivedAt = reg.ReceivedAt,
                                            AcceptedMail = reg.AcceptedMail,
                                            Reminder1Mail = reg.Reminder1Mail,
                                            Reminder2Mail = reg.Reminder2Mail,
                                            ReminderLevel = reg.ReminderLevel,
                                            Reminder1Due = reg.Reminder1Mail == null
                                                        && reg.AcceptedMail != null
                                                        && reg.AcceptedMail.Sent < reminderDueFrom,
                                            Reminder2Due = reg.Reminder2Mail == null
                                                        && reg.Reminder1Mail != null
                                                        && reg.Reminder1Mail.Sent < reminderDueFrom,
                                            ReminderSmsSent = reg.ReminderSms?.Sent,
                                            PhoneNormalized = reg.PhoneNormalized,
                                            ReminderMailPossible = reg.Reminder2Mail == null
                                                                && reg.AcceptedMail != null
                                                                && reg.AcceptedMail.Sent < reminderDueFrom
                                                                && reg.Email != null,
                                            ReminderSmsPossible = reg.ReminderSms == null
                                                               && reg.Reminder2Mail != null
                                                               && reg.AcceptedMail != null
                                                               && reg.AcceptedMail.Sent < reminderDueFrom
                                                               && reg.PhoneNormalized != null,
                                            WillPayAtCheckin = reg.WillPayAtCheckin
                                        })
                         .ToList();

        result.ForEach(dpi =>
        {
            var lastNotification = GetLastNotification(dpi);
            if (lastNotification != null)
            {
                dpi.DaysSinceLastNotification = (int)Math.Floor((now - lastNotification.Value.Date).TotalDays);
                dpi.LastNotificationType = lastNotification.Value.Type;
                dpi.ReminderMailPossible = dpi.Reminder1Due || dpi.Reminder2Due;
            }
        });

        return result.OrderByDescending(dpi => dpi.WillPayAtCheckin)
                     .ThenByDescending(dpi => dpi.DaysSinceLastNotification ?? 0);
    }

    private static (DateTimeOffset Date, string Type)? GetLastNotification(DuePaymentItem dpi)
    {
        var lastNotification = new[]
                               {
                                   (Date: dpi.ReminderSmsSent, Type: Properties.Resources.SMS),
                                   (Date: dpi.Reminder2Mail?.Sent, Type: Properties.Resources.ReminderMail1),
                                   (Date: dpi.Reminder1Mail?.Sent, Type: Properties.Resources.ReminderMail2),
                                   (Date: dpi.AcceptedMail?.Sent, Type: Properties.Resources.AcceptedMail)
                               }.Where(ntf => ntf.Date != null)
                                .DefaultIfEmpty((Date: null, Type: string.Empty))
                                .MaxBy(ntf => ntf.Date);
        return lastNotification.Date == null ? null : (Date: lastNotification.Date!.Value, lastNotification.Type);
    }
}

public class DuePaymentItem
{
    public SentMailDto? AcceptedMail { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public Guid Id { get; set; }
    public string? LastName { get; set; }
    public decimal? Paid { get; set; }
    public string? PhoneNormalized { get; set; }
    public decimal? Price { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public bool Reminder1Due { get; set; }
    public SentMailDto? Reminder1Mail { get; set; }
    public bool Reminder2Due { get; set; }
    public SentMailDto? Reminder2Mail { get; set; }
    public int ReminderLevel { get; set; }
    public bool ReminderMailPossible { get; set; }
    public bool ReminderSmsPossible { get; set; }
    public DateTimeOffset? ReminderSmsSent { get; set; }
    public int? DaysSinceLastNotification { get; set; }
    public string? LastNotificationType { get; set; }
    public bool WillPayAtCheckin { get; internal set; }
}

public class SentMailDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Sent { get; set; }
}