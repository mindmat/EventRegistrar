using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.RegistrationForms
{
    public class Question : Entity
    {
        public int ExternalId { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public QuestionType Type { get; set; }
    }
}