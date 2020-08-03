using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using EventRegistrar.Backend.Registrations.Register;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.RegistrationForms.FormPaths
{
    public class FormPathsQuery : IRequest<IEnumerable<RegistrationFormGroup>>
    {
        public Guid EventId { get; set; }
    }

    public class RegistrationFormGroup
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public IEnumerable<RegistrationFormPath>? Paths { get; set; }
        public IEnumerable<QuestionToRegistrablesDisplayItem>? UnassignedOptions { get; set; }
        public IEnumerable<QuestionDisplayItem>? Questions { get; set; }
    }

    public class RegistrationFormPath
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public SingleRegistrationProcessConfiguration? SingleConfig { get; set; }
        public PartnerRegistrationProcessConfiguration? PartnerConfig { get; set; }
    }

    public class FormPathsQueryHandler : IRequestHandler<FormPathsQuery, IEnumerable<RegistrationFormGroup>>
    {
        private readonly IQueryable<RegistrationForm> _registrationForms;

        public FormPathsQueryHandler(IQueryable<RegistrationForm> registrationForms)
        {
            _registrationForms = registrationForms;
        }

        public async Task<IEnumerable<RegistrationFormGroup>> Handle(FormPathsQuery query, CancellationToken cancellationToken)
        {
            var paths = await _registrationForms.Where(frm => frm.EventId == query.EventId)
                                                .Select(frm => new RegistrationFormGroup
                                                {
                                                    Id = frm.Id,
                                                    Title = frm.Title,
                                                    Paths = frm.FormPaths.Select(fpt => new RegistrationFormPath
                                                    {
                                                        Id = fpt.Id,
                                                        Description = fpt.Description,
                                                        SingleConfig = fpt.SingleConfiguration,
                                                        PartnerConfig = fpt.PartnerConfiguration
                                                    }),
                                                    UnassignedOptions = frm.Questions.SelectMany(qst => qst.QuestionOptions
                                                                                     .Where(qop => !qop.Registrables.Any(map => map.Registrable!.RowVersion != null))
                                                                                     .OrderBy(qop => qop.Question!.Index)
                                                                                     .Select(qop => new QuestionToRegistrablesDisplayItem
                                                                                     {
                                                                                         RegistrableId = null,
                                                                                         RegistrableName = null,
                                                                                         QuestionOptionId = qop.Id,
                                                                                         Question = qop.Question!.Title,
                                                                                         Answer = qop.Answer,
                                                                                         Section = qop.Question!.Section
                                                                                     })),
                                                    Questions = frm.Questions.Where(que => que.Type == QuestionType.Text)
                                                                             .Select(que => new QuestionDisplayItem
                                                                             {
                                                                                 Id = que.Id,
                                                                                 Section = que.Section,
                                                                                 Question = que.Title
                                                                             })

                                                })
                                                .ToListAsync(cancellationToken);

            return paths;
            //// transfer properties from table
            //return paths.Select(fpt =>
            //{
            //    if (fpt.Type == FormPathType.Single)
            //    {
            //        var singleConfig = _jsonHelper.Deserialize<SingleRegistrationProcessConfiguration>(fpt.ConfigurationJson);
            //        singleConfig.Id = fpt.Id;
            //        singleConfig.Description = fpt.Description;
            //        singleConfig.RegistrationFormId = fpt.RegistrationFormId;
            //        singleConfig.Type = fpt.Type;
            //        return (IRegistrationProcessConfiguration)singleConfig;
            //    }
            //    if (fpt.Type == FormPathType.Partner)
            //    {
            //        var partnerConfig = _jsonHelper.Deserialize<PartnerRegistrationProcessConfiguration>(fpt.ConfigurationJson);
            //        partnerConfig.Id = fpt.Id;
            //        partnerConfig.Description = fpt.Description;
            //        partnerConfig.RegistrationFormId = fpt.RegistrationFormId;
            //        partnerConfig.Type = FormPathType.Partner;
            //        return (IRegistrationProcessConfiguration)partnerConfig;
            //    }
            //    throw new Exception($"Unknown form type {fpt.Type}");
            //});
        }
    }
}