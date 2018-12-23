using System.Collections.Generic;
using System.Linq;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses
{
    public class ResendLastMailWhenEmailAddressFixed : IEventToCommandTranslation<InvalidEmailAddressFixed>
    {
        private readonly IQueryable<MailToRegistration> _mails;

        public ResendLastMailWhenEmailAddressFixed(IQueryable<MailToRegistration> mails)
        {
            _mails = mails;
        }

        public IEnumerable<IQueueBoundMessage> Translate(InvalidEmailAddressFixed e)
        {
            if (e.EventId.HasValue)
            {
                var lastMail = _mails.Where(mail => mail.RegistrationId == e.RegistrationId)
                                     .OrderByDescending(mail => mail.Mail.Sent)
                                     .First();

                //yield return new ReleaseMailCommand { EventId = e.EventId.Value, MailId = lastMail.MailId, Withhold = true };
            }

            yield break;
        }
    }
}