namespace EventRegistrar.Backend.Registrations.Register;

public interface IQuestionMapping
{
    Guid QuestionOptionId { get; }
    Guid RegistrableId { get; }
}

public class QuestionMapping : IQuestionMapping
{
    public Guid QuestionOptionId { get; set; }
    public Guid RegistrableId { get; set; }
}

public class PartnerQuestionMapping : IQuestionMapping
{
    public Guid QuestionOptionId { get; set; }
    public Guid RegistrableId { get; set; }

    public Guid QuestionId_Partner { get; set; }
    public Guid QuestionOptionId_Follower { get; set; }
    public Guid QuestionOptionId_Leader { get; set; }
}