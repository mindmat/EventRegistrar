using System;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Spots;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Events
{
    public class OpenRegistrationCommandHandler : IRequestHandler<OpenRegistrationCommand>
    {
        private readonly IRepository<Event> _events;
        private readonly IRepository<IndividualReduction> _individualReductions;
        private readonly IRepository<MailEvent> _mailEvents;
        private readonly IRepository<Mail> _mails;
        private readonly IRepository<MailToRegistration> _mailsOfRegistrations;
        private readonly IRepository<RegistrationCancellation> _registrationCancellations;
        private readonly IRepository<Registration> _registrations;
        private readonly IRepository<Response> _responses;
        private readonly IRepository<Seat> _seats;
        private readonly IRepository<Sms> _sms;

        public OpenRegistrationCommandHandler(IRepository<Event> events,
                                              IRepository<IndividualReduction> individualReductions,
                                              IRepository<Mail> mails,
                                              IRepository<MailToRegistration> mailsOfRegistrations,
                                              IRepository<RegistrationCancellation> registrationCancellations,
                                              IRepository<Registration> registrations,
                                              IRepository<Response> responses,
                                              IRepository<Seat> seats,
                                              IRepository<Sms> sms,
                                              IRepository<MailEvent> mailEvents)
        {
            _events = events;
            _individualReductions = individualReductions;
            _mails = mails;
            _mailsOfRegistrations = mailsOfRegistrations;
            _registrationCancellations = registrationCancellations;
            _registrations = registrations;
            _responses = responses;
            _seats = seats;
            _sms = sms;
            _mailEvents = mailEvents;
        }

        public async Task<Unit> Handle(OpenRegistrationCommand command, CancellationToken cancellationToken)
        {
            var eventToOpen = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
            if (eventToOpen.State != State.Setup)
            {
                throw new ArgumentException($"Event {eventToOpen.Id} is in state {eventToOpen.State} and can therefore not be opened");
            }

            eventToOpen.State = State.RegistrationOpen;

            _individualReductions.Remove(ird => ird.Registration.EventId == command.EventId);
            _mails.Remove(mev => mev.EventId == command.EventId);
            _mailsOfRegistrations.Remove(mev => mev.Registration.EventId == command.EventId);
            _registrationCancellations.Remove(cnc => cnc.Registration.EventId == command.EventId);
            _registrations.Remove(reg => reg.EventId == command.EventId);
            _responses.Remove(rsp => rsp.Registration.EventId == command.EventId);
            _seats.Remove(seat => seat.Registrable.EventId == command.EventId);
            _sms.Remove(sms => sms.Registration.EventId == command.EventId);

            return Unit.Value;
        }
    }
}