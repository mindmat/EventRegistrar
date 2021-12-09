using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Mailing.Import;

public class ImportedMailToRegistration : Entity
{
    public Guid ImportedMailId { get; set; }
    public ImportedMail? Mail { get; set; }
    public Guid RegistrationId { get; set; }
    public Registration? Registration { get; set; }
}