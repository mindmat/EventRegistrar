using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Statements;
using EventRegistrar.Backend.Payments.Unrecognized;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Search;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Authorization
{
    internal class RightsOfEventRoleProvider : IRightsOfEventRoleProvider
    {
        /// <summary>
        /// hook for dynamic setup of rights in roles per event
        /// hardcoded to start
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="usersRolesInEvent"></param>
        /// <returns></returns>
        public IEnumerable<string> GetRightsOfEventRoles(Guid eventId, ICollection<UserInEventRole> usersRolesInEvent)
        {
            if (usersRolesInEvent.Contains(UserInEventRole.Reader) ||
                usersRolesInEvent.Contains(UserInEventRole.Writer) ||
                usersRolesInEvent.Contains(UserInEventRole.Admin))
            {
                yield return typeof(SingleRegistrablesOverviewQuery).Name;
                yield return typeof(DoubleRegistrablesOverviewQuery).Name;
                yield return typeof(PaymentOverviewQuery).Name;
                yield return typeof(SearchRegistrationQuery).Name;
                yield return typeof(RegistrablesQuery).Name;
                yield return typeof(RegistrationQuery).Name;
                yield return typeof(MailsOfRegistrationQuery).Name;
                yield return typeof(SpotsOfRegistrationQuery).Name;
                yield return typeof(DuePaymentsQuery).Name;
                yield return typeof(MailTemplatesQuery).Name;
                yield return typeof(ParticipantsOfRegistrableQuery).Name;
                yield return typeof(QuestionToRegistrablesQuery).Name;
                yield return typeof(GetPendingMailsQuery).Name;
                yield return typeof(MailTypesQuery).Name;
                yield return typeof(LanguagesQuery).Name;
                yield return typeof(AllExternalRegistrationIdentifiersQuery).Name;
                yield return typeof(PaymentStatementsQuery).Name;
            }
            if (usersRolesInEvent.Contains(UserInEventRole.Writer) ||
                usersRolesInEvent.Contains(UserInEventRole.Admin))
            {
                yield return typeof(UnrecognizedPaymentsQuery).Name;
                yield return typeof(SetRecognizedEmailCommand).Name;
                yield return typeof(SaveMailTemplateCommand).Name;
                yield return typeof(ReleaseMailCommand).Name;
                yield return typeof(DeleteMailCommand).Name;
                yield return typeof(SendReminderCommand).Name;
                yield return typeof(AddSpotCommand).Name;
                yield return typeof(SavePaymentFileCommand).Name;
                yield return typeof(SetDoubleRegistrableLimitsCommand).Name;
                yield return typeof(SetSingleRegistrableLimitsCommand).Name;
            }

            if (usersRolesInEvent.Contains(UserInEventRole.Admin))
            {
                yield return typeof(AccessRequestsOfEventQuery).Name;
                yield return typeof(UsersOfEventQuery).Name;
                yield return typeof(AddUserToRoleInEventCommand).Name;
                yield return typeof(RemoveUserFromRoleInEventCommand).Name;
                yield return typeof(RespondToRequestCommand).Name;
                yield return typeof(SaveRegistrationFormDefinitionCommand).Name;
            }
        }
    }
}