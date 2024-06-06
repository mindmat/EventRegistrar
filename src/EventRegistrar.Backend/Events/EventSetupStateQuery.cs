using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.Registrations.Raw;

namespace EventRegistrar.Backend.Events;

public class EventSetupStateQuery : IRequest<EventSetupState>, IEventBoundRequest
{
    public Guid EventId { get; set; }
}

public class EventSetupState
{
    public bool FormSent { get; set; }
    public bool FormImported { get; set; }
    public bool TracksDefined { get; set; }
    public bool FormMapped { get; set; }
    public bool PricePackagesDefined { get; set; }
    public bool AutoMailTemplatesDefined { get; set; }
    public int RegistrationsReceived { get; set; }
    public int RegistrationsProcessed { get; set; }
    public IEnumerable<string> ProcessingErrors { get; set; } = null!;
}

public class EventSetupStateQueryHandler(IQueryable<Event> events,
                                         IQueryable<RawRegistrationForm> rawRegistrationForms,
                                         IQueryable<RegistrationForm> registrationForms,
                                         IQueryable<RawRegistration> rawRegistrations,
                                         IQueryable<PricePackage> pricePackages,
                                         IQueryable<AutoMailTemplate> autoMailTemplates)
    : IRequestHandler<EventSetupStateQuery, EventSetupState>
{
    public async Task<EventSetupState> Handle(EventSetupStateQuery query, CancellationToken cancellationToken)
    {
        var @event = await events.Where(evt => evt.Id == query.EventId)
                                 .Select(evt => new
                                                {
                                                    evt.Acronym,
                                                    AnyTracks = evt.Registrables!.Any()
                                                })
                                 .FirstAsync(cancellationToken);
        var formSent = await rawRegistrationForms.AnyAsync(rrf => rrf.EventAcronym == @event.Acronym, cancellationToken);
        var form = await registrationForms.Where(frm => frm.EventId == query.EventId)
                                          .Select(frm => new
                                                         {
                                                             frm.Id,
                                                             Mapped = frm.Questions!.Any(qst => qst.Mapping != null
                                                                                             || qst.QuestionOptions!.Any(qop => qop.Mappings!.Any()))
                                                                   || frm.MultiMappings!.Any()
                                                         })
                                          .FirstOrDefaultAsync(cancellationToken);

        var rawRegistrationCounts = await rawRegistrations.Where(rr => rr.EventAcronym == @event.Acronym)
                                                          .GroupBy(rrg => rrg.EventAcronym)
                                                          .Select(grp => new
                                                                         {
                                                                             Received = grp.Count(),
                                                                             Processed = grp.Count(rrg => rrg.Processed != null),
                                                                             ProcessionErrors = grp.Where(rrg => rrg.Processed == null
                                                                                                              && rrg.LastProcessingError != null)
                                                                                                   .Select(rrg => rrg.LastProcessingError!)
                                                                                                   .Distinct()
                                                                         })
                                                          .FirstOrDefaultAsync(cancellationToken);

        var pricePackagesDefined = await pricePackages.Where(frm => frm.EventId == query.EventId)
                                                      .AnyAsync(ppk => ppk.Parts!.Any(), cancellationToken);

        var autoMailTemplatesOfEvent = await autoMailTemplates.Where(frm => frm.EventId == query.EventId)
                                                              .Select(amt => new
                                                                             {
                                                                                 amt.Type,
                                                                                 amt.Language,
                                                                                 HasContent = !string.IsNullOrWhiteSpace(amt.ContentHtml)
                                                                             })
                                                              .ToListAsync(cancellationToken);

        var autoMailTemplatesDefined = autoMailTemplatesOfEvent.Any(amt => amt.Type == MailType.SingleRegistrationAccepted
                                                                        || amt.Type == MailType.PartnerRegistrationMatchedAndAccepted);
        return new EventSetupState
               {
                   FormSent = formSent,
                   FormImported = form != null,
                   TracksDefined = @event.AnyTracks,
                   FormMapped = form?.Mapped == true,
                   PricePackagesDefined = pricePackagesDefined,
                   AutoMailTemplatesDefined = autoMailTemplatesDefined,
                   RegistrationsReceived = rawRegistrationCounts?.Received ?? 0,
                   ProcessingErrors = rawRegistrationCounts?.ProcessionErrors ?? [],
                   RegistrationsProcessed = rawRegistrationCounts?.Processed ?? 0
               };
    }
}