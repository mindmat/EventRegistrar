using System.Diagnostics.CodeAnalysis;

namespace EventRegistrar.Backend.Registrations;

public static class RegistrationExtensionMethods
{
    public static bool IsPartnerRegistration(this Registration registration)
    {
        return registration.RegistrationId_Partner != null || registration.PartnerNormalized != null;
    }
    public  static bool IsAlreadyMatchedToOtherThan([NotNullWhen(false)] this Registration? registration,
                                                            Guid? ownRegistrationId)
    {
        return registration == null
            || registration.RegistrationId_Partner != null
            && registration.RegistrationId_Partner != ownRegistrationId;
    }

    public static bool MatchesPartnerText(this Registration registration, string? partner)
    {
        if (string.Equals(registration.RespondentEmail,
                          partner,
                          StringComparison.InvariantCultureIgnoreCase))
        {
            // email matches
            return true;
        }

        if ($" {partner} ".Contains($" {registration.RespondentFirstName} ",
                                    StringComparison.InvariantCultureIgnoreCase)
         && $" {partner} ".Contains($" {registration.RespondentLastName} ",
                                    StringComparison.InvariantCultureIgnoreCase))
        {
            // names match
            return true;
        }

        return false;
    }
}