using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

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

public class FormPathsQueryHandler : IRequestHandler<FormPathsQuery, IEnumerable<RegistrationFormGroup>>
{
    private readonly IQueryable<RegistrationForm> _forms;

    public FormPathsQueryHandler(IQueryable<RegistrationForm> forms)
    {
        _forms = forms;
    }

    public async Task<IEnumerable<RegistrationFormGroup>> Handle(FormPathsQuery query,
                                                                 CancellationToken cancellationToken)
    {
        var forms = await _forms.Where(frm => frm.EventId == query.EventId)
                                .Select(frm => new
                                               {
                                                   frm.Id,
                                                   frm.Title,
                                                   Questions = frm.Questions!
                                                                  .Select(qst => new
                                                                                 {
                                                                                     qst.Id,
                                                                                     qst.Section,
                                                                                     qst.Index,
                                                                                     qst.Title,
                                                                                     qst.Type,
                                                                                     qst.Mapping,
                                                                                     Options = qst.QuestionOptions!
                                                                                                  .Select(qop => new
                                                                                                                 {
                                                                                                                     qop.Id,
                                                                                                                     qop.Answer,
                                                                                                                     MappedRegistrables =
                                                                                                                         qop.Mappings!
                                                                                                                            .Select(map => new
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
                                       Sections = frm.Questions
                                                     .GroupBy(qst => qst.Section)
                                                     .Select(grp => new FormSection
                                                                    {
                                                                        Name = grp.Key,
                                                                        SortKey = grp.Min(qst => qst.Index),
                                                                        Questions = grp.Where(qst => qst.Type != QuestionType.SectionHeader
                                                                                                  && qst.Type != QuestionType.PageBreak)
                                                                                       .Select(qst => new QuestionMappingDisplayItem
                                                                                                      {
                                                                                                          Id = qst.Id,
                                                                                                          Question = qst.Title,
                                                                                                          Type = qst.Type,
                                                                                                          SortKey = qst.Index,
                                                                                                          Mappable = qst.Type is QuestionType.Text or QuestionType.ParagraphText,
                                                                                                          Mapping = qst.Mapping,
                                                                                                          Options = qst.Options.Select(qop =>
                                                                                                              new QuestionOptionMappingDisplayItem
                                                                                                              {
                                                                                                                  Id = qop.Id,
                                                                                                                  Answer = qop.Answer,
                                                                                                                  MappedRegistrableCombinedIds = qop.MappedRegistrables
                                                                                                                      .Select(map => $"{map.RegistrableId}|{map.Type}|{map.Language}")
                                                                                                              })
                                                                                                      })
                                                                                       .OrderBy(qst => qst.SortKey)
                                                                    })
                                                     .Where(sec => sec.Questions.Any())
                                                     .OrderBy(sec => sec.SortKey)
                                   });
    }

    private static string GetName(MappingType? type, string registrableName, string? language)
    {
        return type switch
        {
            MappingType.Language => language switch
            {
                "en" => $"{Properties.Resources.Language}: {Properties.Resources.English}",
                "de" => $"{Properties.Resources.Language}: {Properties.Resources.German}",
                _    => $"{Properties.Resources.Language}: ?"
            },
            MappingType.Reduction                  => Properties.Resources.Reduction,
            MappingType.PartnerRegistrableLeader   => $"{registrableName} ({Properties.Resources.Leader})",
            MappingType.PartnerRegistrableFollower => $"{registrableName} ({Properties.Resources.Follower})",
            MappingType.RoleLeader                 => $"{Properties.Resources.Role}: {Properties.Resources.Leader}",
            MappingType.RoleFollower               => $"{Properties.Resources.Role}: {Properties.Resources.Follower}",
            _                                      => registrableName
        };
    }
}

public class QuestionOptionMappingDisplayItem
{
    public Guid Id { get; set; }
    public string? Answer { get; set; }
    public IEnumerable<string>? MappedRegistrableCombinedIds { get; set; }
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