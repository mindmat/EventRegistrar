using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.Registrations;

using Resources = EventRegistrar.Backend.Properties.Resources;

namespace EventRegistrar.Backend.Payments.Due;

public class DuePaymentsUpdater : ReadModelUpdater<IEnumerable<DuePaymentItem>>
{
    public const int DefaultPaymentGracePeriod = 6;

    public static readonly HashSet<MailType?> MailTypes_Accepted =
        new() { MailType.PartnerRegistrationMatchedAndAccepted, MailType.SingleRegistrationAccepted };

    public static readonly HashSet<MailType?> MailTypes_Reminder1 =
        new() { MailType.PartnerRegistrationFirstReminder, MailType.SingleRegistrationFirstReminder };

    public static readonly HashSet<MailType?> MailTypes_Reminder2 =
        new() { MailType.PartnerRegistrationSecondReminder, MailType.SingleRegistrationSecondReminder };

    private readonly IQueryable<Registration> _registrations;
    private readonly IDateTimeProvider _dateTimeProvider;


    public DuePaymentsUpdater(IQueryable<Registration> registrations,
                              IDateTimeProvider dateTimeProvider)
    {
        _registrations = registrations;
        _dateTimeProvider = dateTimeProvider;
    }

    public override string QueryName => nameof(DuePaymentsQuery);
    public override bool IsDateDependent => true;

    public override async Task<IEnumerable<DuePaymentItem>> CalculateTyped(Guid eventId, Guid? rowId, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.Now;
        var reminderDueFrom = now.AddDays(-DefaultPaymentGracePeriod);
        var data = await _registrations.Where(reg => reg.RegistrationForm!.EventId == eventId
                                                  && reg.State == RegistrationState.Received
                                                  && reg.IsWaitingList != true
                                                  && reg.OriginalPrice > 0)
                                       .Select(reg => new
                                                      {
                                                          reg.Id,
                                                          FirstName = reg.RespondentFirstName,
                                                          LastName = reg.RespondentLastName,
                                                          Email = reg.RespondentEmail,
                                                          reg.Price,
                                                          reg.ReceivedAt,
                                                          reg.PhoneNormalized,
                                                          reg.ReminderLevel,
                                                          Paid = (decimal?)reg.PaymentAssignments!.Sum(asn => asn.PayoutRequestId == null ? asn.Amount : -asn.Amount),
                                                          Mails = reg.Mails!.Select(rml => new
                                                                                           {
                                                                                               rml.Mail!.Id,
                                                                                               Sent = rml.Mail.Created,
                                                                                               rml.Mail.Type,
                                                                                               rml.Mail.Withhold,
                                                                                               rml.Mail.Discarded
                                                                                           }),
                                                          ReminderSms = reg.Sms!.Where(sms => sms.Type == SmsType.Reminder)
                                                      })
                                       .ToListAsync(cancellationToken);

        var result = data.Select(tmp => new
                                        {
                                            tmp.Id,
                                            tmp.FirstName,
                                            tmp.LastName,
                                            tmp.Email,
                                            tmp.Price,
                                            tmp.ReceivedAt,
                                            tmp.PhoneNormalized,
                                            tmp.ReminderLevel,
                                            tmp.Paid,
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
                                            Price = reg.Price,
                                            Paid = reg.Paid,
                                            ReceivedAt = reg.ReceivedAt,
                                            AcceptedMail = reg.AcceptedMail,
                                            Reminder1Mail = reg.Reminder1Mail,
                                            Reminder2Mail = reg.Reminder2Mail,
                                            ReminderLevel = reg.ReminderLevel,
                                            Reminder1Due = reg.Reminder1Mail == null && reg.AcceptedMail != null && reg.AcceptedMail.Sent < reminderDueFrom,
                                            Reminder2Due = reg.Reminder2Mail == null && reg.Reminder1Mail != null && reg.Reminder1Mail.Sent < reminderDueFrom,
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
                                                               && reg.PhoneNormalized != null
                                        })
                         .ToList();

        result.ForEach(dpi =>
        {
            var lastNotification = GetLastNotification(dpi);
            if (lastNotification != null)
            {
                dpi.DaysSinceLastNotification = (int)Math.Floor((now - lastNotification.Value.Date).TotalDays);
                dpi.LastNotificationType = lastNotification.Value.Type;
            }
        });

        return result.OrderByDescending(dpi => dpi.DaysSinceLastNotification ?? 0);
    }

    private static (DateTime Date, string Type)? GetLastNotification(DuePaymentItem dpi)
    {
        var lastNotification = new[]
                               {
                                   (Date: dpi.ReminderSmsSent, Type: Resources.SMS),
                                   (Date: dpi.Reminder2Mail?.Sent, Type: Resources.ReminderMail1),
                                   (Date: dpi.Reminder1Mail?.Sent, Type: Resources.ReminderMail2),
                                   (Date: dpi.AcceptedMail?.Sent, Type: Resources.AcceptedMail)
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
    public DateTime ReceivedAt { get; set; }
    public bool Reminder1Due { get; set; }
    public SentMailDto? Reminder1Mail { get; set; }
    public bool Reminder2Due { get; set; }
    public SentMailDto? Reminder2Mail { get; set; }
    public int ReminderLevel { get; set; }
    public bool ReminderMailPossible { get; set; }
    public bool ReminderSmsPossible { get; set; }
    public DateTime? ReminderSmsSent { get; set; }
    public int? DaysSinceLastNotification { get; set; }
    public string? LastNotificationType { get; set; }
}

public class SentMailDto
{
    public Guid Id { get; set; }
    public DateTime Sent { get; set; }
}