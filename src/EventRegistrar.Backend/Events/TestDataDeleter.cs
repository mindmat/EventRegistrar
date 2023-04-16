using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Feedback;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Responses;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Events;

public class TestDataDeleter
{
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
    private readonly IEventBus _eventBus;
    private readonly ReadModelUpdater _readModelUpdater;

    public TestDataDeleter(IRepository<IndividualReduction> individualReductions,
                           IRepository<MailEvent> mailEvents,
                           IRepository<Payment> payments,
                           IRepository<PaymentAssignment> paymentAssignments,
                           IRepository<IncomingPayment> incomingPayments,
                           IRepository<OutgoingPayment> outgoingPayments,
                           IRepository<PaymentsFile> paymentFiles,
                           IRepository<RawRegistration> rawRegistrations,
                           CommandQueue commandQueue,
                           DbContext dbContext,
                           IRepository<Mail> mails,
                           IRepository<MailToRegistration> mailsOfRegistrations,
                           IRepository<RegistrationCancellation> registrationCancellations,
                           IRepository<Registration> registrations,
                           IRepository<Response> responses,
                           IRepository<Seat> spots,
                           IRepository<Sms> sms,
                           IEventBus eventBus,
                           ReadModelUpdater readModelUpdater)
    {
        _individualReductions = individualReductions;
        _mailEvents = mailEvents;
        _payments = payments;
        _paymentAssignments = paymentAssignments;
        _incomingPayments = incomingPayments;
        _outgoingPayments = outgoingPayments;
        _paymentFiles = paymentFiles;
        _rawRegistrations = rawRegistrations;
        _commandQueue = commandQueue;
        _dbContext = dbContext;
        _mails = mails;
        _mailsOfRegistrations = mailsOfRegistrations;
        _registrationCancellations = registrationCancellations;
        _registrations = registrations;
        _responses = responses;
        _spots = spots;
        _sms = sms;
        _eventBus = eventBus;
        _readModelUpdater = readModelUpdater;
    }

    public async Task DeleteTestData(Event @event, CancellationToken cancellationToken)
    {
        var eventId = @event.Id;
        var eventAcronym = @event.Acronym;
        if (@event.State != EventState.Setup)
        {
            throw new ArgumentException($"Event {eventId} is in state {@event.State}, therefore test data cannot be deleted");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _individualReductions.Remove(ird => ird.Registration!.EventId == eventId);
            _mails.Remove(mev => mev.EventId == eventId);
            _mailsOfRegistrations.Remove(mev => mev.Registration!.EventId == eventId);
            _registrationCancellations.Remove(cnc => cnc.Registration!.EventId == eventId);
            _responses.Remove(rsp => rsp.Registration!.EventId == eventId);
            _spots.Remove(spot => spot.Registrable!.EventId == eventId);
            _sms.Remove(sms => sms.Registration!.EventId == eventId);
            _mailEvents.Remove(mev => mev.Mail!.EventId == eventId);
            _paymentAssignments.Remove(pas => pas.Registration!.EventId == eventId
                                           || pas.IncomingPayment!.Payment!.EventId == eventId
                                           || pas.OutgoingPayment!.Payment!.EventId == eventId);
            _incomingPayments.Remove(pmt => pmt.Payment!.EventId == eventId);
            _outgoingPayments.Remove(pmt => pmt.Payment!.EventId == eventId);
            _payments.Remove(pmt => pmt.EventId == eventId);
            _paymentFiles.Remove(pmf => pmf.EventId == eventId);
            if (!string.IsNullOrWhiteSpace(eventAcronym))
            {
                _rawRegistrations.Remove(rrg => rrg.EventAcronym == eventAcronym);
            }

            var registrations = await _registrations.Where(reg => reg.EventId == eventId).ToListAsync(cancellationToken);
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
            return;
        }

        _commandQueue.EnqueueCommand(new StartUpdateReadModelsOfEventCommand { EventId = eventId });
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = eventId,
                              QueryName = nameof(RegistrablesOverviewQuery)
                          });

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = eventId,
                              QueryName = nameof(PricePackageOverviewQuery)
                          });
        _eventBus.Publish(new QueryChanged
                          {
                              EventId = eventId,
                              QueryName = nameof(PaymentOverviewQuery)
                          });
    }
}