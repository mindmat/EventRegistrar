using System.Collections.Generic;
using MediatR;

namespace EventRegistrar.Backend.Registrations.Raw
{
    public class AllExternalRegistrationIdentifiersQuery : IRequest<IEnumerable<string>>
    {
        public string RegistrationFormExternalIdentifier { get; set; }
    }
}