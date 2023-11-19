namespace EventRegistrar.Backend.Registrations;

public static class RegistrationExtensionMethods
{
    public static bool IsPartnerRegistration(this Registration registration)
    {
        return registration.RegistrationId_Partner != null || registration.PartnerNormalized != null;
    }
}