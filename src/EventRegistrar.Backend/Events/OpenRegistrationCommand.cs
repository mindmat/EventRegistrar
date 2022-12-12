using DocumentFormat.OpenXml.InkML;

using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Assignments.Candidates;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Events;

public class OpenRegistrationCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public bool DeleteTestData { get; set; }
}

public class OpenRegistrationCommandHandler : IRequestHandler<OpenRegistrationCommand>
{
    private readonly IRepository<Event> _events;
    private readonly IRepository<IndividualReduction> _individualReductions;
    private readonly IRepository<MailEvent> _mailEvents;
    private readonly IRepository<Payment> _payments;
    private readonly IRepository<PaymentAssignment> _paymentAssignments;
    private readonly IRepository<IncomingPayment> _incomingPayments;
    private readonly IRepository<OutgoingPayment> _outgoingPayments;
    private readonly IRepository<PaymentsFile> _paymentFiles;
    private readonly IRepository<RawRegistration> _rawRegistrations;
    private readonly CommandQueue _commandQueue;
    private readonly DbContext _dbContext;
    private readonly IRepository<Mail> _mails;
    private readonly IRepository<MailToRegistration> _mailsOfRegistrations;
    private readonly IRepository<RegistrationCancellation> _registrationCancellations;
    private readonly IRepository<Registration> _registrations;
    private readonly IRepository<Response> _responses;
    private readonly IRepository<Seat> _spots;
    private readonly IRepository<Sms> _sms;

    public OpenRegistrationCommandHandler(IRepository<Event> events,
                                          IRepository<IndividualReduction> individualReductions,
                                          IRepository<Mail> mails,
                                          IRepository<MailToRegistration> mailsOfRegistrations,
                                          IRepository<RegistrationCancellation> registrationCancellations,
                                          IRepository<Registration> registrations,
                                          IRepository<Response> responses,
                                          IRepository<Seat> spots,
                                          IRepository<Sms> sms,
                                          IRepository<MailEvent> mailEvents,
                                          IRepository<Payment> payments,
                                          IRepository<PaymentAssignment> paymentAssignments,
                                          IRepository<IncomingPayment> incomingPayments,
                                          IRepository<OutgoingPayment> outgoingPayments,
                                          IRepository<PaymentsFile> paymentFiles,
                                          IRepository<RawRegistration> rawRegistrations,
                                          CommandQueue commandQueue,
                                          DbContext dbContext)
    {
        _events = events;
        _individualReductions = individualReductions;
        _mails = mails;
        _mailsOfRegistrations = mailsOfRegistrations;
        _registrationCancellations = registrationCancellations;
        _registrations = registrations;
        _responses = responses;
        _spots = spots;
        _sms = sms;
        _mailEvents = mailEvents;
        _payments = payments;
        _paymentAssignments = paymentAssignments;
        _incomingPayments = incomingPayments;
        _outgoingPayments = outgoingPayments;
        _paymentFiles = paymentFiles;
        _rawRegistrations = rawRegistrations;
        _commandQueue = commandQueue;
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(OpenRegistrationCommand command, CancellationToken cancellationToken)
    {
        var eventToOpen = await _events.FirstAsync(evt => evt.Id == command.EventId, cancellationToken);
        if (eventToOpen.State != EventState.Setup)
        {
            throw new ArgumentException($"Event {eventToOpen.Id} is in state {eventToOpen.State} and can therefore not be opened");
        }

        eventToOpen.State = EventState.RegistrationOpen;
        if (command.DeleteTestData)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _individualReductions.Remove(ird => ird.Registration!.EventId == command.EventId);
                _mails.Remove(mev => mev.EventId == command.EventId);
                _mailsOfRegistrations.Remove(mev => mev.Registration!.EventId == command.EventId);
                _registrationCancellations.Remove(cnc => cnc.Registration!.EventId == command.EventId);
                _responses.Remove(rsp => rsp.Registration!.EventId == command.EventId);
                _spots.Remove(spot => spot.Registrable!.EventId == command.EventId);
                _sms.Remove(sms => sms.Registration!.EventId == command.EventId);
                _mailEvents.Remove(mev => mev.Mail!.EventId == command.EventId);
                _paymentAssignments.Remove(pas => pas.Registration!.EventId == command.EventId
                                               || pas.IncomingPayment!.Payment!.PaymentsFile!.EventId == command.EventId
                                               || pas.OutgoingPayment!.Payment!.PaymentsFile!.EventId == command.EventId);
                _incomingPayments.Remove(pmt => pmt.Payment!.PaymentsFile!.EventId == command.EventId);
                _outgoingPayments.Remove(pmt => pmt.Payment!.PaymentsFile!.EventId == command.EventId);
                _payments.Remove(pmt => pmt.PaymentsFile!.EventId == command.EventId);
                _paymentFiles.Remove(pmf => pmf.EventId == command.EventId);
                if (!string.IsNullOrWhiteSpace(eventToOpen.Acronym))
                {
                    _rawRegistrations.Remove(rrg => rrg.EventAcronym == eventToOpen.Acronym);
                }

                var registrations = await _registrations.Where(reg => reg.EventId == command.EventId).ToListAsync(cancellationToken);
                registrations.ForEach(reg =>
                {
                    reg.RegistrationId_Partner = null;
                    reg.Registration_Partner = null;
                });
                await _dbContext.SaveChangesAsync(cancellationToken);

                registrations.ForEach(reg => { _registrations.Remove(reg); });
                await _dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                return Unit.Value;
            }

            _commandQueue.EnqueueCommand(new StartUpdateReadModelsOfEventCommand { EventId = command.EventId });
        }

        return Unit.Value;
    }
}