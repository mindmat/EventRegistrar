using System;

namespace EventRegistrator.Functions.Registrations
{
    public class RegistrationDisplayInfo
    {
        public Guid? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public RegistrationState State { get; set; }
        public string Email { get; set; }
    }
}