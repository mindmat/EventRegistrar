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

public class TestDataDeleter(IRepository<IndividualReduction> individualReductions,
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
                             IRepository<Registration> _registrations,
                             IRepository<Response> responses,
                             IRepository<Seat> spots,
                             IRepository<Sms> sms,
                             IEventBus eventBus,
                             ChangeTrigger changeTrigger)
{
    private readonly ChangeTrigger _changeTrigger = changeTrigger;

    public async Task DeleteTestData(Event @event, CancellationToken cancellationToken)
    {
        var eventId = @event.Id;
        var eventAcronym = @event.Acronym;
        if (@event.State != EventState.Setup)
        {
            throw new ArgumentException($"Event {eventId} is in state {@event.State}, therefore test data cannot be deleted");
        }

        //await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            individualReductions.Remove(ird => ird.Registration!.EventId == eventId);
            mails.Remove(mev => mev.EventId == eventId);
            mailsOfRegistrations.Remove(mev => mev.Registration!.EventId == eventId);
            registrationCancellations.Remove(cnc => cnc.Registration!.EventId == eventId);
            responses.Remove(rsp => rsp.Registration!.EventId == eventId);
            spots.Remove(spot => spot.Registrable!.EventId == eventId);
            sms.Remove(sms => sms.Registration!.EventId == eventId);
            mailEvents.Remove(mev => mev.Mail!.EventId == eventId);
            if (!string.IsNullOrWhiteSpace(eventAcronym))
            {
                rawRegistrations.Remove(rrg => rrg.EventAcronym == eventAcronym);
            }

            var registrations = await _registrations.AsTracking()
                                                    .Where(reg => reg.EventId == eventId)
                                                    .ToListAsync(cancellationToken);
            registrations.ForEach(reg =>
            {
                reg.RegistrationId_Partner = null;
                reg.Registration_Partner = null;
            });
            var assignments = await paymentAssignments.AsTracking()
                                                      .Where(pas => pas.Registration!.EventId == eventId
                                                                 || pas.IncomingPayment!.Payment!.EventId == eventId
                                                                 || pas.OutgoingPayment!.Payment!.EventId == eventId)
                                                      .Where(pas => pas.PaymentAssignmentId_Counter != null)
                                                      .ToListAsync(cancellationToken);
            assignments.ForEach(pas =>
            {
                pas.PaymentAssignmentId_Counter = null;
            });
            await dbContext.SaveChangesAsync(cancellationToken);

            registrations.ForEach(reg => { _registrations.Remove(reg); });
            paymentAssignments.Remove(pas => pas.Registration!.EventId == eventId
                                          || pas.IncomingPayment!.Payment!.EventId == eventId
                                          || pas.OutgoingPayment!.Payment!.EventId == eventId);
            incomingPayments.Remove(pmt => pmt.Payment!.EventId == eventId);
            outgoingPayments.Remove(pmt => pmt.Payment!.EventId == eventId);
            payments.Remove(pmt => pmt.EventId == eventId);
            paymentFiles.Remove(pmf => pmf.EventId == eventId);

            var readModelsSet = dbContext.Set<ReadModel>();
            var readModels = await readModelsSet.Where(rmd => rmd.EventId == eventId)
                                                .ToListAsync(cancellationToken);
            readModelsSet.RemoveRange(readModels);

            await dbContext.SaveChangesAsync(cancellationToken);

            //await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            //await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        commandQueue.EnqueueCommand(new StartUpdateReadModelsOfEventCommand { EventId = eventId });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = eventId,
                             QueryName = nameof(RegistrablesOverviewQuery)
                         });

        eventBus.Publish(new QueryChanged
                         {
                             EventId = eventId,
                             QueryName = nameof(PricePackageOverviewQuery)
                         });
        eventBus.Publish(new QueryChanged
                         {
                             EventId = eventId,
                             QueryName = nameof(PaymentOverviewQuery)
                         });
    }
}