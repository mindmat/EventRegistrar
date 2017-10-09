using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Registrations
{
    public class Response : Entity
    {
        public int QuestionId { get; set; }
        public string ResponseString { get; set; }
    }
}