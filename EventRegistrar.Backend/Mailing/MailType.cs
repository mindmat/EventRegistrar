namespace EventRegistrar.Backend.Mailing
{
    public enum MailType
    {
        SingleRegistrationAccepted = 1,
        SingleRegistrationOnWaitingList = 2,
        DoubleRegistrationFirstPartnerAccepted = 11,
        DoubleRegistrationMatchedAndAccepted = 12,
        DoubleRegistrationFirstPartnerOnWaitingList = 13,
        DoubleRegistrationMatchedOnWaitingList = 14,
        SoldOut = 21,
        OnlyOneRegistrationPerEmail = 22,
        RegistrationCancelled = 31,
        SingleRegistrationFullyPaid = 41,
        PartnerRegistrationFirstPaid = 42,
        PartnerRegistrationFullyPaid = 43,
        SingleRegistrationFirstReminder = 51,
        SingleRegistrationSecondReminder = 52,
        DoubleRegistrationFirstReminder = 61,
        DoubleRegistrationSecondReminder = 62,
        OptionsForRegistrationsOnWaitingList = 101
    }
}