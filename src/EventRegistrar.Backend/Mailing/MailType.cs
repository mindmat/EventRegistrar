namespace EventRegistrar.Backend.Mailing;

public enum MailType
{
    SingleRegistrationAccepted = 1,
    SingleRegistrationOnWaitingList = 2,
    RegistrationReceived = 3,

    [PartnerMailType]
    PartnerRegistrationFirstPartnerAccepted = 11,

    [PartnerMailType]
    PartnerRegistrationMatchedAndAccepted = 12,

    [PartnerMailType]
    PartnerRegistrationFirstPartnerOnWaitingList = 13,

    [PartnerMailType]
    PartnerRegistrationMatchedOnWaitingList = 14,
    SoldOut = 21,
    MoneyOwed = 22,
    TooMuchPaid = 23,

    //OnlyOneRegistrationPerEmail = 22,
    RegistrationCancelled = 31,
    SingleRegistrationFullyPaid = 41,

    [PartnerMailType]
    PartnerRegistrationFirstPaid = 42,

    [PartnerMailType]
    PartnerRegistrationFullyPaid = 43,
    SingleRegistrationFirstReminder = 51,
    SingleRegistrationSecondReminder = 52,

    [PartnerMailType]
    PartnerRegistrationFirstReminder = 61,

    [PartnerMailType]
    PartnerRegistrationSecondReminder = 62,
    OptionsForRegistrationsOnWaitingList = 101
}

public class PartnerMailTypeAttribute : Attribute;