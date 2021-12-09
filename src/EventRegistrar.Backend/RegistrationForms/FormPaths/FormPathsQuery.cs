using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using EventRegistrar.Backend.Registrations.Register;
using MediatR;

namespace EventRegistrar.Backend.RegistrationForms.FormPaths;

public class FormPathsQuery : IRequest<IEnumerable<RegistrationFormGroup>>
{
    public Guid EventId { get; set; }
}

public class RegistrationFormGroup
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
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

    public async Task<IEnumerable<RegistrationFormGroup>> Handle(FormPathsQuery query,
                                                                 CancellationToken cancellationToken)
    {
        var forms = await _registrationForms.Where(frm => frm.EventId == query.EventId)
                                            .Select(frm => new
                                                           {
                                                               frm.Id,
                                                               frm.Title,
                                                               Questions = frm.Questions.Select(qst => new
                                                                   {
                                                                       qst.Id,
                                                                       qst.Section,
                                                                       qst.Index,
                                                                       qst.Title,
                                                                       qst.Type,
                                                                       qst.Mapping,
                                                                       Options = qst.QuestionOptions.Select(qop => new
                                                                           {
                                                                               qop.Id,
                                                                               qop.Answer,
                                                                               MappedRegistrables =
                                                                                   qop.Mappings.Select(map =>
                                                                                       new
                                                                                       {
                                                                                           map.RegistrableId,
                                                                                           map.Type,
                                                                                           map.Registrable!.Name,
                                                                                           map.Language
                                                                                       })
                                                                           })
                                                                   })
                                                           })
                                            .ToListAsync(cancellationToken);

        return forms.Select(frm => new RegistrationFormGroup
                                   {
                                       Id = frm.Id,
                                       Title = frm.Title,
                                       Sections = frm.Questions.GroupBy(qst => qst.Section)
                                                     .Select(grp => new FormSection
                                                                    {
                                                                        Name = grp.Key,
                                                                        SortKey = grp.Min(qst => qst.Index),
                                                                        Questions = grp.Where(qst =>
                                                                                qst.Type != QuestionType
                                                                                    .SectionHeader &&
                                                                                qst.Type != QuestionType.PageBreak)
                                                                            .Select(qst =>
                                                                                new QuestionMappingDisplayItem
                                                                                {
                                                                                    Id = qst.Id,
                                                                                    Question = qst.Title,
                                                                                    Type = qst.Type,
                                                                                    SortKey = qst.Index,
                                                                                    Mappable = qst.Type ==
                                                                                        QuestionType.Text
                                                                                     || qst.Type ==
                                                                                        QuestionType.ParagraphText,
                                                                                    Mapping = qst.Mapping,
                                                                                    Options = qst.Options.Select(qop =>
                                                                                        new
                                                                                        QuestionOptionMappingDisplayItem
                                                                                        {
                                                                                            Id = qop.Id,
                                                                                            Answer = qop.Answer,
                                                                                            MappedRegistrables =
                                                                                                qop.MappedRegistrables
                                                                                                    .Select(map =>
                                                                                                        new
                                                                                                        AvailableQuestionOptionMapping
                                                                                                        {
                                                                                                            CombinedId =
                                                                                                                $"{map.RegistrableId}/{map.Type}/{map.Language}",
                                                                                                            Id = map
                                                                                                                .RegistrableId,
                                                                                                            Type = map
                                                                                                                .Type,
                                                                                                            Name =
                                                                                                                GetName(
                                                                                                                    map
                                                                                                                        .Type,
                                                                                                                    map
                                                                                                                        .Name,
                                                                                                                    map
                                                                                                                        .Language)
                                                                                                        })
                                                                                        })
                                                                                })
                                                                            .OrderBy(qst => qst.SortKey)
                                                                    })
                                                     .Where(sec => sec.Questions?.Any() == true)
                                                     .OrderBy(sec => sec.SortKey)
                                   });
    }

    private string GetName(MappingType? type, string registrableName, string? language)
    {
        switch (type)
        {
            case MappingType.Language:
            {
                return language switch
                {
                    "en" => $"{Properties.Resources.Language}: {Properties.Resources.English}",
                    "de" => $"{Properties.Resources.Language}: {Properties.Resources.German}",
                    _ => $"{Properties.Resources.Language}: ?"
                };
            }

            case MappingType.Reduction:
                return Properties.Resources.Reduction;

            case MappingType.PartnerRegistrableLeader:
                return $"{registrableName} ({Properties.Resources.Leader})";
            case MappingType.PartnerRegistrableFollower:
                return $"{registrableName} ({Properties.Resources.Follower})";

            case MappingType.RoleLeader:
                return $"{Properties.Resources.Role}: {Properties.Resources.Leader}";
            case MappingType.RoleFollower:
                return $"{Properties.Resources.Role}: {Properties.Resources.Follower}";
        }

        return registrableName;
    }
}

public class QuestionOptionMappingDisplayItem
{
    public Guid Id { get; set; }
    public string? Answer { get; set; }
    public IEnumerable<AvailableQuestionOptionMapping>? MappedRegistrables { get; set; }
}

public class QuestionMappingDisplayItem
{
    public Guid Id { get; set; }
    public string? Question { get; set; }
    public QuestionType Type { get; set; }
    public IEnumerable<QuestionOptionMappingDisplayItem>? Options { get; set; }
    public int SortKey { get; set; }
    public bool Mappable { get; set; }
    public QuestionMappingType? Mapping { get; set; }
}

public class FormSection
{
    public string? Name { get; set; }
    public int SortKey { get; set; }
    public IEnumerable<QuestionMappingDisplayItem> Questions { get; set; }
}