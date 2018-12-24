using System.Collections.Generic;
using System.Linq;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing.Compose;
using MediatR;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses
{
    public class ResendLastMailWhenEmailAddressFixed : IEventToCommandTranslation<InvalidEmailAddressFixed>
    {
        private readonly IQueryable<MailToRegistration> _mails;

        public ResendLastMailWhenEmailAddressFixed(IQueryable<MailToRegistration> mails)
        {
            _mails = mails;
        }

        public IEnumerable<IRequest> Translate(InvalidEmailAddressFixed e)
        {
            if (e.EventId.HasValue)
            {
                var lastMail = _mails.Where(mail => mail.RegistrationId == e.RegistrationId
                                                 && mail.Mail.Type.HasValue)
                                     .OrderByDescending(mail => mail.Mail.Sent)
                                     .First();

                if (lastMail.Mail.Type != null)
                {
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
    }
}