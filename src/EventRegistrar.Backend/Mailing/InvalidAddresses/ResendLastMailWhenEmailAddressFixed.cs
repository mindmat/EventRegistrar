using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Payments.Due;
using MediatR;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class ResendLastMailWhenEmailAddressFixed : IEventToCommandTranslation<InvalidEmailAddressFixed>
{
    private readonly DuePaymentConfiguration _duePaymentConfiguration;
    private readonly IQueryable<MailToRegistration> _mails;

    public ResendLastMailWhenEmailAddressFixed(IQueryable<MailToRegistration> mails,
                                               DuePaymentConfiguration duePaymentConfiguration)
    {
        _mails = mails;
        _duePaymentConfiguration = duePaymentConfiguration;
    }

    public IEnumerable<IRequest> Translate(InvalidEmailAddressFixed e)
    {
        if (e.EventId.HasValue)
        {
            var lastMail = _mails.Where(mail => mail.RegistrationId == e.RegistrationId
                                             && mail.Mail.Type.HasValue
                                             && !_duePaymentConfiguration.MailTypes_Reminder1.Contains(mail.Mail.Type
                                                    .Value)
                                             && !_duePaymentConfiguration.MailTypes_Reminder2.Contains(mail.Mail.Type
                                                    .Value))
                                 .OrderByDescending(mail => mail.Mail.Sent)
                                 .First();

            if (lastMail.Mail.Type != null)
                yield return new ComposeAndSendMailCommand
                             {
                                 EventId = e.EventId.Value,
                                 AllowDuplicate = true,
                                 Withhold = true,
                                 RegistrationId = e.RegistrationId,
                                 MailType = lastMail.Mail.Type.Value
                             };
        }
    }
}