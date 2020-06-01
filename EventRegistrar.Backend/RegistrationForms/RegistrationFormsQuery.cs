using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using EventRegistrar.Backend.Registrations.Register;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms
{
    public class RegistrationFormMappings
    {
        public Guid RegistrationFormId { get; set; }
        public FormType? Type { get; set; }
        public SingleRegistrationProcessConfiguration? SingleConfiguration { get; set; }
        public IEnumerable<QuestionToRegistrablesDisplayItem>? Mappings { get; set; }
        public IEnumerable<QuestionToRegistrablesDisplayItem>? UnassignedOptions { get; set; }
        public IEnumerable<QuestionDisplayItem>? Questions { get; set; }
        public string? Title { get; set; }
    }

    public class RegistrationFormsQuery : IRequest<IEnumerable<RegistrationFormMappings>>
    {
        public Guid EventId { get; set; }
    }

    public class RegistrationFormsQueryHandler : IRequestHandler<RegistrationFormsQuery, IEnumerable<RegistrationFormMappings>>
    {
        private readonly IQueryable<RegistrationForm> _forms;

        public RegistrationFormsQueryHandler(IQueryable<RegistrationForm> forms)
        {
            _forms = forms;
        }

        public async Task<IEnumerable<RegistrationFormMappings>> Handle(RegistrationFormsQuery query, CancellationToken cancellationToken)
        {
            var forms = await _forms.Where(frm => frm.EventId == query.EventId)
                                    .Include(frm => frm.Questions).ThenInclude(qst => qst.QuestionOptions)
                                                                  .ThenInclude(qop => qop.Registrables)
                                                                  .ThenInclude(map => map.Registrable)
                                    .ToListAsync(cancellationToken);

            return forms.Select(form => new RegistrationFormMappings
            {
                RegistrationFormId = form.Id,
                Type = form.Type,
                Title = form.Title,
                SingleConfiguration = form.ProcessConfigurationJson != null && form.Type == FormType.Single
                                      ? JsonHelper.TryDeserialize<SingleRegistrationProcessConfiguration>(form.ProcessConfigurationJson)
                                      : null,
                Mappings = form.Questions.SelectMany(qst => qst.QuestionOptions.SelectMany(qop => qop.Registrables)
                                                                               .OrderBy(map => map.QuestionOption?.Question?.Index)
                                                                               .Select(map => new QuestionToRegistrablesDisplayItem
                                                                               {
                                                                                   MappingId = map.Id,
                                                                                   RegistrableId = map.RegistrableId,
                                                                                   RegistrableName = map.Registrable!.Name,
                                                                                   IsPartnerRegistrable = map.Registrable.MaximumDoubleSeats != null,
                                                                                   QuestionOptionId = map.QuestionOptionId,
                                                                                   Section = map.QuestionOption?.Question?.Section,
                                                                                   Question = map.QuestionOption?.Question?.Title,
                                                                                   Answer = map.QuestionOption?.Answer,
                                                                                   QuestionId_Partner = map.QuestionId_Partner,
                                                                                   QuestionOptionId_Leader = map.QuestionOptionId_Leader,
                                                                                   QuestionOptionId_Follower = map.QuestionOptionId_Follower
                                                                               })),
                UnassignedOptions = form.Questions.SelectMany(qst => qst.QuestionOptions
                                                                        .Where(qop => !qop.Registrables.Any(map => map.Registrable?.RowVersion != null))
                                                                        .OrderBy(qop => qop.Question?.Index)
                                                                        .Select(qop => new QuestionToRegistrablesDisplayItem
                                                                        {
                                                                            RegistrableId = null,
                                                                            RegistrableName = null,
                                                                            QuestionOptionId = qop.Id,
                                                                            Question = qop.Question?.Title,
                                                                            Answer = qop.Answer,
                                                                            Section = qop.Question?.Section
                                                                        })),
                Questions = form.Questions.Where(que => que.Type == QuestionType.Text)
                                          .Select(que => new QuestionDisplayItem
                                          {
                                              Id = que.Id,
                                              Section = que.Section,
                                              Question = que.Title
                                          })

            });
        }


    }
}