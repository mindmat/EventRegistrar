using EventRegistrar.Backend.RegistrationForms;

namespace EventRegistrar.Backend.Registrations.Register;

public class PartnerRegistrationProcessConfiguration : IRegistrationProcessConfiguration
{
    public Guid Id { get; set; }
    public Guid RegistrationFormId { get; set; }
    public string? Description { get; set; }
    public FormPathType Type { get; set; }
    public IEnumerable<LanguageMapping>? LanguageMappings { get; set; }
    public Guid QuestionId_Follower_Email { get; set; }
    public Guid QuestionId_Follower_FirstName { get; set; }
    public Guid QuestionId_Follower_LastName { get; set; }
    public Guid? QuestionId_Follower_Phone { get; set; }
    public Guid QuestionId_Leader_Email { get; set; }
    public Guid QuestionId_Leader_FirstName { get; set; }
    public Guid QuestionId_Leader_LastName { get; set; }
    public Guid? QuestionId_Leader_Phone { get; set; }
    public Guid QuestionOptionId_Trigger { get; set; }
    public IEnumerable<(Guid QuestionOptionId, Role Role, Guid RegistrableId)>? RoleSpecificMappings { get; set; }
}