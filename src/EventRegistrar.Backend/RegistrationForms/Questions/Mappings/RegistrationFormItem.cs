using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;

namespace EventRegistrar.Backend.RegistrationForms.Questions.Mappings;

public class RegistrationFormItem
{
    public Guid? RegistrationFormId { get; internal set; }
    public string ExternalIdentifier { get; internal set; }
    public EventState State { get; internal set; }
    public string? Title { get; internal set; }
    public IEnumerable<FormSection> Sections { get; set; }

    //public string Language { get; internal set; }
    public DateTimeOffset? LastImport { get; internal set; }
    public DateTimeOffset? PendingRawFormCreated { get; internal set; }
    public Guid? PendingRawFormId { get; internal set; }
    public bool Deletable { get; set; }
    public IEnumerable<MultiMapping> MultiMappings { get; set; }
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