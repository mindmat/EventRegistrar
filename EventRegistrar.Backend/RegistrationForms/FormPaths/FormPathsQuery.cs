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
        public IEnumerable<FormSection> Sections { get; set; }
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
            var forms = await _registrationForms.Where(frm => frm.EventId == query.EventId)
                                                .Select(frm => new
                                                {
                                                    frm.Id,
                                                    frm.Title,
                                                    //Paths = frm.FormPaths.Select(fpt => new RegistrationFormPath
                                                    //{
                                                    //    Id = fpt.Id,
                                                    //    Description = fpt.Description,
                                                    //    SingleConfig = fpt.SingleConfiguration,
                                                    //    PartnerConfig = fpt.PartnerConfiguration
                                                    //}),
                                                    Questions = frm.Questions.Select(qst => new
                                                    {
                                                        qst.Id,
                                                        qst.Section,
                                                        qst.Index,
                                                        qst.Title,
                                                        qst.Type,
                                                        Options = qst.QuestionOptions.Select(qop => new
                                                        {
                                                            qop.Id,
                                                            qop.Answer,
                                                            MappedRegistrables = qop.Registrables.Select(map => new { map.RegistrableId, map.Type, map.Registrable!.Name })
                                                        })
                                                    })

                                                    //UnassignedOptions = frm.Questions.SelectMany(qst => qst.QuestionOptions
                                                    //                                 .Where(qop => !qop.Registrables.Any(map => map.Registrable!.Id != null))
                                                    //                                 .OrderBy(qop => qop.Question!.Index)
                                                    //                                 .Select(qop => new QuestionToRegistrablesDisplayItem
                                                    //                                 {
                                                    //                                     Section = qop.Question!.Section,
                                                    //                                     Question = qop.Question!.Title,
                                                    //                                     QuestionOptionId = qop.Id,
                                                    //                                     Answer = qop.Answer
                                                    //                                 })),
                                                    //Questions = frm.Questions.Where(que => que.Type == QuestionType.Text)
                                                    //                         .Select(que => new QuestionDisplayItem
                                                    //                         {
                                                    //                             Id = que.Id,
                                                    //                             Section = que.Section,
                                                    //                             Question = que.Title
                                                    //                         })

                                                })
                                                .ToListAsync(cancellationToken);

            return forms.Select(frm => new RegistrationFormGroup
            {
                Id = frm.Id,
                Title = frm.Title,
                //Paths = frm.FormPaths.Select(fpt => new RegistrationFormPath
                //{
                //    Id = fpt.Id,
                //    Description = fpt.Description,
                //    SingleConfig = fpt.SingleConfiguration,
                //    PartnerConfig = fpt.PartnerConfiguration
                //}),
                Sections = frm.Questions.GroupBy(qst => qst.Section).Select(grp => new FormSection
                {
                    Name = grp.Key,
                    SortKey = grp.Min(qst => qst.Index),
                    Questions = grp.Where(qst => qst.Type != QuestionType.SectionHeader && qst.Type != QuestionType.PageBreak)
                                   .Select(qst => new QuestionMappingDisplayItem
                                   {
                                       Id = qst.Id,
                                       Question = qst.Title,
                                       Type = qst.Type,
                                       SortKey = qst.Index,
                                       Options = qst.Options.Select(qop => new QuestionOptionMappingDisplayItem
                                       {
                                           Id = qop.Id,
                                           Answer = qop.Answer,
                                           MappedRegistrables = qop.MappedRegistrables.Select(map => new QuestionOptionMapping
                                           {
                                               CombinedId = $"{map.RegistrableId}/{map.Type}",
                                               Id = map.RegistrableId,
                                               Type = map.Type,
                                               Name = map.Name
                                           })
                                       })
                                   }).OrderBy(qst => qst.SortKey)
                }).OrderBy(sec => sec.SortKey),
                //UnassignedOptions = frm.Questions.SelectMany(qst => qst.QuestionOptions
                //                                 .Where(qop => !qop.Registrables.Any(map => map.Registrable!.Id != null))
                //                                 .OrderBy(qop => qop.Question!.Index)
                //                                 .Select(qop => new QuestionToRegistrablesDisplayItem
                //                                 {
                //                                     Section = qop.Question!.Section,
                //                                     Question = qop.Question!.Title,
                //                                     QuestionOptionId = qop.Id,
                //                                     Answer = qop.Answer
                //                                 })),
                //Questions = frm.Questions.Where(que => que.Type == QuestionType.Text)
                //                         .Select(que => new QuestionDisplayItem
                //                         {
                //                             Id = que.Id,
                //                             Section = que.Section,
                //                             Question = que.Title
                //                         })

            });
        }
    }

    public class QuestionOptionMappingDisplayItem
    {
        public Guid Id { get; set; }
        public string? Answer { get; set; }
        public IEnumerable<QuestionOptionMapping>? MappedRegistrables { get; set; }
    }

    public class QuestionMappingDisplayItem
    {
        public Guid Id { get; set; }
        public string? Question { get; set; }
        public QuestionType Type { get; set; }
        public IEnumerable<QuestionOptionMappingDisplayItem>? Options { get; set; }
        public int SortKey { get; set; }
    }


    public class FormSection
    {
        public string? Name { get; set; }
        public int SortKey { get; set; }
        public IEnumerable<QuestionMappingDisplayItem> Questions { get; set; }
    }
}