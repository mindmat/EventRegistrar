namespace EventRegistrar.Backend.Mailing
{
    public enum MailType
    {
        SingleRegistrationAccepted = 1,
        SingleRegistrationOnWaitingList = 2,
        RegistrationReceived = 3,
        PartnerRegistrationFirstPartnerAccepted = 11,
        PartnerRegistrationMatchedAndAccepted = 12,
        PartnerRegistrationFirstPartnerOnWaitingList = 13,
        PartnerRegistrationMatchedOnWaitingList = 14,
        SoldOut = 21,
        MoneyOwed = 22,
        TooMuchPaid = 23,
        //OnlyOneRegistrationPerEmail = 22,
        RegistrationCancelled = 31,
        SingleRegistrationFullyPaid = 41,
        PartnerRegistrationFirstPaid = 42,
        PartnerRegistrationFullyPaid = 43,
        SingleRegistrationFirstReminder = 51,
        SingleRegistrationSecondReminder = 52,
        PartnerRegistrationFirstReminder = 61,
        PartnerRegistrationSecondReminder = 62,
        OptionsForRegistrationsOnWaitingList = 101,

    }
}