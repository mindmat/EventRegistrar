using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Payments.Due;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class ResendLastMailWhenEmailAddressFixed(IQueryable<MailToRegistration> mails,
                                                 DuePaymentConfiguration duePaymentConfiguration)
    : IEventToCommandTranslation<InvalidEmailAddressFixed>
{
    public IEnumerable<IRequest> Translate(InvalidEmailAddressFixed e)
    {
        if (e.EventId != null)
        {
            var lastMailType = mails.Where(mail => mail.RegistrationId == e.RegistrationId
                                                && mail.Mail!.Type != null
                                                && !duePaymentConfiguration.MailTypes_Reminder1.Contains(mail.Mail.Type.Value)
                                                && !duePaymentConfiguration.MailTypes_Reminder2.Contains(mail.Mail.Type.Value))
                                    .Include(mail => mail.Mail)
                                    .OrderByDescending(mail => mail.Mail!.Sent)
                                    .Select(mail => mail.Mail!.Type)
                                    .First();

            if (lastMailType != null)
            {
                yield return new ComposeAndSendAutoMailCommand
                             {
                                 EventId = e.EventId.Value,
                                 AllowDuplicate = true,
                                 Withhold = true,
                                 RegistrationId = e.RegistrationId,
                                 MailType = lastMailType.Value
                             };
            }
        }
    }
}