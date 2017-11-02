using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Mailing
{
    public class Mail : Entity
    {
        public string ContentHtml { get; set; }
        public string ContentPlainText { get; set; }
        //public string ContentPlainText { get; set; }
    }
}