namespace EventRegistrator.Functions.Mailing
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
        OnlyOneRegistrationPerEmail = 22
    }
}