using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Hosting;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing;
using EventRegistrar.Backend.Mailing.Bulk;
using EventRegistrar.Backend.Mailing.Compose;
using EventRegistrar.Backend.Mailing.Import;
using EventRegistrar.Backend.Mailing.InvalidAddresses;
using EventRegistrar.Backend.Mailing.ManualTrigger;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Payments;
using EventRegistrar.Backend.Payments.Assignments;
using EventRegistrar.Backend.Payments.Differences;
using EventRegistrar.Backend.Payments.Due;
using EventRegistrar.Backend.Payments.Files;
using EventRegistrar.Backend.Payments.Files.Fetch;
using EventRegistrar.Backend.Payments.Files.Slips;
using EventRegistrar.Backend.Payments.PayAtCheckin;
using EventRegistrar.Backend.Payments.Refunds;
using EventRegistrar.Backend.Payments.Settlements;
using EventRegistrar.Backend.Payments.Statements;
using EventRegistrar.Backend.Payments.Unassigned;
using EventRegistrar.Backend.PhoneMessages;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Registrables.Participants;
using EventRegistrar.Backend.Registrables.Pricing;
using EventRegistrar.Backend.Registrables.Reductions;
using EventRegistrar.Backend.Registrables.Tags;
using EventRegistrar.Backend.Registrables.WaitingList.MoveUp;
using EventRegistrar.Backend.RegistrationForms.GoogleForms;
using EventRegistrar.Backend.RegistrationForms.Questions.Mappings;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.Cancel;
using EventRegistrar.Backend.Registrations.IndividualReductions;
using EventRegistrar.Backend.Registrations.InternalNotes;
using EventRegistrar.Backend.Registrations.Matching;
using EventRegistrar.Backend.Registrations.Overview;
using EventRegistrar.Backend.Registrations.Price;
using EventRegistrar.Backend.Registrations.Raw;
using EventRegistrar.Backend.Registrations.Reductions;
using EventRegistrar.Backend.Registrations.Register;
using EventRegistrar.Backend.Registrations.Remarks;
using EventRegistrar.Backend.Registrations.Search;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Authorization;

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
        if (usersRolesInEvent.Contains(UserInEventRole.Reader)
         || usersRolesInEvent.Contains(UserInEventRole.Writer)
         || usersRolesInEvent.Contains(UserInEventRole.Admin))
        {
            yield return nameof(RightsOfUserInEventQuery);
            yield return nameof(RegistrablesOverviewQuery);
            yield return nameof(RegistrableTagsQuery);
            yield return nameof(PaymentOverviewQuery);
            yield return nameof(SearchRegistrationQuery);
            yield return nameof(RegistrablesQuery);
            yield return nameof(RegistrationQuery);
            yield return nameof(MailsOfRegistrationQuery);
            yield return nameof(SpotsOfRegistrationQuery);
            yield return nameof(DuePaymentsQuery);
            yield return nameof(AutoMailTemplatesQuery);
            yield return nameof(AutoMailTemplateQuery);
            yield return nameof(ParticipantsOfRegistrableQuery);
            yield return nameof(PendingMailsQuery);
            yield return nameof(AllExternalRegistrationIdentifiersQuery);
            yield return nameof(PaymentsByDayQuery);
            yield return nameof(PaymentAssignmentsQuery);
            yield return nameof(SmsConversationQuery);
            yield return nameof(CheckInQuery);
            yield return nameof(RegistrationsWithUnmatchedPartnerQuery);
            yield return nameof(PotentialPartnersQuery);
            yield return nameof(AssignedPaymentsOfRegistrationQuery);
            yield return nameof(PaymentSlipImageQuery);
            yield return nameof(HostingQuery);
            yield return nameof(InvalidAddressesQuery);
            yield return nameof(PartyOverviewQuery);
            yield return nameof(PossibleMailTypesQuery);
            yield return nameof(UnassignedIncomingPaymentsQuery);
            yield return nameof(NotReceivedMailsQuery);
            yield return nameof(PricingQuery);
            yield return nameof(EventQuery);
            yield return nameof(PossibleAudiencesQuery);
            yield return nameof(CancellationsQuery);
            yield return nameof(DifferencesQuery);
            yield return nameof(PayoutQuery);
            yield return nameof(UnassignedPayoutsQuery);
            yield return nameof(PossiblePayoutAssignmentQuery);
            yield return nameof(DomainEventsQuery);
            yield return nameof(BookingsByStateQuery);
            yield return nameof(MailViewQuery);
            yield return nameof(MailTemplatePreviewQuery);
            yield return nameof(AccessRequestsOfEventQuery);
            yield return nameof(UsersOfEventQuery);
            yield return nameof(UserInEventRolesQuery);
            yield return nameof(EventByAcronymQuery);
            yield return nameof(UpdateUserInfoCommand);
            yield return nameof(FroalaKeyQuery);
            yield return nameof(UnprocessedRawRegistrationCountQuery);
            yield return nameof(RemarksOverviewQuery);
            yield return nameof(InternalNotesQuery);
            yield return nameof(BulkMailTemplatesQuery);
            yield return nameof(BulkMailTemplateQuery);
            yield return nameof(GeneratedBulkMailsQuery);
            yield return nameof(PossibleManualFallbackPricePackagesQuery);
            yield return nameof(ParticipantsOfEventQuery);
            yield return nameof(PricePackageOverviewQuery);
            yield return nameof(AvailableMailersQuery);
            yield return nameof(MailDeliverySuccessQuery);
        }

        if (usersRolesInEvent.Contains(UserInEventRole.Writer)
         || usersRolesInEvent.Contains(UserInEventRole.Admin))
        {
            yield return nameof(ReleaseMailsCommand);
            yield return nameof(DeleteMailsCommand);
            yield return nameof(SendReminderMailCommand);
            yield return nameof(AddSpotCommand);
            yield return nameof(RemoveSpotCommand);
            yield return nameof(SavePaymentFileCommand);
            yield return nameof(CreateBulkMailsCommand);
            yield return nameof(ReleaseBulkMailsCommand);
            yield return nameof(TriggerMoveUpFromWaitingListCommand);
            yield return nameof(AssignIncomingPaymentCommand);
            yield return nameof(ComposeAndSendAutoMailCommand);
            yield return nameof(ComposeAndSendBulkMailCommand);
            yield return nameof(CancelRegistrationCommand);
            yield return nameof(SendSmsCommand);
            yield return nameof(SwapFirstLastNameCommand);
            yield return nameof(MatchPartnerRegistrationsCommand);
            yield return nameof(ChangeUnmatchedPartnerRegistrationToSingleRegistrationCommand);
            yield return nameof(UnassignPaymentCommand);
            yield return nameof(AssignOutgoingPaymentCommand);
            yield return nameof(FixInvalidAddressCommand);
            yield return nameof(AddIndividualReductionCommand);
            yield return nameof(RemoveIndividualReductionCommand);
            yield return nameof(ImportMailsFromImapCommand);
            yield return nameof(UnbindPartnerRegistrationCommand);
            yield return nameof(SetReductionCommand);
            yield return nameof(WillPayAtCheckinCommand);
            yield return nameof(DeleteRegistrableCommand);
            yield return nameof(DeleteBulkMailTemplateCommand);
            yield return nameof(SaveReductionCommand);
            yield return nameof(DeleteReductionCommand);
            yield return nameof(SetRegistrablesPricesCommand);
            yield return nameof(ActivateAutomaticPromotionCommand);
            yield return nameof(DeactivateAutomaticPromotionCommand);
            yield return nameof(IgnorePaymentCommand);
            yield return nameof(SaveRegistrableCommand);
            yield return nameof(FetchBankStatementsFileCommand);
            yield return nameof(ProcessFetchedBankStatementsFileCommand);
            yield return nameof(SendPaymentDueMailCommand);
            yield return nameof(RefundDifferenceCommand);
            yield return nameof(UpdateAutoMailTemplateCommand);
            yield return nameof(UpdateAutoMailConfigurationCommand);
            yield return nameof(CreateAutoMailTemplateCommand);
            yield return nameof(SetReleaseMailCommand);
            yield return nameof(StartProcessAllPendingRawRegistrationsCommand);
            yield return nameof(ResendSentMailsWithoutStateCommand);
            yield return nameof(SetRemarksProcessedStateCommand);
            yield return nameof(UpdateInternalNotesCommand);
            yield return nameof(CreateBulkMailTemplateCommand);
            yield return nameof(UpdateBulkMailTemplateCommand);
            yield return nameof(SetManualFallbackToPricePackageCommand);
            yield return nameof(AssignRepaymentCommand);
            yield return nameof(CreateAndAssignIncomingPaymentCommand);
            yield return nameof(ChangeParticipantNameCommand);
        }

        if (usersRolesInEvent.Contains(UserInEventRole.Admin))
        {
            yield return nameof(SetRoleOfUserInEventCommand);
            yield return nameof(RemoveUserFromEventCommand);
            yield return nameof(RespondToRequestCommand);
            yield return nameof(ImportRegistrationFormCommand);
            yield return nameof(OpenRegistrationCommand);
            yield return nameof(ReleaseAllPendingMailsCommand);
            yield return nameof(DeleteRegistrationFormCommand);
            yield return nameof(SaveRegistrationFormMappingsCommand);
            yield return nameof(SavePricingCommand);
            yield return nameof(RecalculateAllPriceAndWaitingListCommand);
            yield return nameof(DeleteTestDataCommand);
        }
    }
}