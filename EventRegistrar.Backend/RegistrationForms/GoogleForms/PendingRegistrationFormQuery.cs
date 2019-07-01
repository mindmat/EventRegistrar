using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.GoogleForms
{
    public class PendingRegistrationFormQuery : IRequest<IEnumerable<RegistrationFormItem>>
    {
        public Guid EventId { get; set; }
    }

    public class PendingRegistrationFormQueryHandler : IRequestHandler<PendingRegistrationFormQuery, IEnumerable<RegistrationFormItem>>
    {
        private readonly IQueryable<RawRegistrationForm> _rawForms;
        private readonly IQueryable<RegistrationForm> _forms;
        private readonly IQueryable<Event> _events;

        public PendingRegistrationFormQueryHandler(IQueryable<RawRegistrationForm> rawForms,
                                                   IQueryable<RegistrationForm> forms,
                                                   IQueryable<Event> events)
        {
            _rawForms = rawForms;
            _forms = forms;
            _events = events;
        }

        public async Task<IEnumerable<RegistrationFormItem>> Handle(PendingRegistrationFormQuery query,
                                                                    CancellationToken cancellationToken)
        {
            var acronym = await _events.FirstAsync(evt => evt.Id == query.EventId, cancellationToken);
            var rawForms = await _rawForms.Where(frm => frm.EventAcronym == acronym.Acronym)
                                          .GroupBy(frm => frm.FormExternalIdentifier)
                                          .Select(grp => new
                                          {
                                              ExternalIdentifier = grp.Key,
                                              PendingRawForm = grp.Where(frm => !frm.Processed)
                                                                  .OrderByDescending(frm => frm.Created)
                                                                  .FirstOrDefault(),
                                              LastProcessedRawForm = grp.Where(frm => frm.Processed)
                                                                        .OrderByDescending(frm => frm.Created)
                                                                        .FirstOrDefault(),
                                          })
                                          .ToListAsync();
            var forms = await _forms.Where(frm => frm.EventId == query.EventId)
                                    .Select(frm => new RegistrationFormItem
                                    {
                                        RegistrationFormId = frm.Id,
                                        ExternalIdentifier = frm.ExternalIdentifier,
                                        State = frm.State,
                                        Title = frm.Title,
                                        Language = frm.Language
                                    })
                                    .ToListAsync();

            foreach (var rawForm in rawForms)
            {
                var existingForm = forms.FirstOrDefault(frm => frm.ExternalIdentifier == rawForm.ExternalIdentifier);
                if (existingForm != null)
                {
                    existingForm.LastImport = rawForm.LastProcessedRawForm?.Created;
                    existingForm.PendingRawFormId = rawForm.PendingRawForm?.Id;
                    existingForm.PendingRawFormCreated = rawForm.PendingRawForm?.Created;
                }
                else
                {
                    forms.Add(new RegistrationFormItem
                    {
                        ExternalIdentifier = rawForm.PendingRawForm?.FormExternalIdentifier,
                        PendingRawFormId = rawForm.PendingRawForm?.Id,
                        PendingRawFormCreated = rawForm.PendingRawForm?.Created
                    });
                }
            }
            return forms;
        }
    }
}
