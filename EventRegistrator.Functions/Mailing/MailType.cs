namespace EventRegistrator.Functions.Mailing
{
    public enum MailType
    {
        SingleRegistrationAccepted = 1,
        SingleRegistrationOnWaitingList = 2,
        DoubleRegistrationFirstPartnerAccepted = 11,
        DoubleRegistrationMatchedAndAccepted = 12,
        DoubleRegistrationOnWaitingList = 13,
    }
}