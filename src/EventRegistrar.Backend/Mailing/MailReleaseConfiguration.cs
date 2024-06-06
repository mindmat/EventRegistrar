using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Mailing;

public class MailReleaseConfiguration : IConfigurationItem
{
    public IEnumerable<MailType> MailsToReleaseAutomatically { get; set; }
}

public class DefaultMailReleaseConfiguration : MailReleaseConfiguration, IDefaultConfigurationItem
{
    public DefaultMailReleaseConfiguration()
    {
        MailsToReleaseAutomatically = new[]
                                      {
                                          //MailType.SingleRegistrationAccepted ,
                                          //MailType.SingleRegistrationOnWaitingList ,
                                          MailType.RegistrationReceived,
                                          //MailType.PartnerRegistrationFirstPartnerAccepted ,
                                          //MailType.PartnerRegistrationMatchedAndAccepted ,
                                          //MailType.PartnerRegistrationFirstPartnerOnWaitingList ,
                                          //MailType.PartnerRegistrationMatchedOnWaitingList ,
                                          //MailType.SoldOut ,
                                          //MailType.OnlyOneRegistrationPerEmail ,
                                          //MailType.RegistrationCancelled ,
                                          MailType.SingleRegistrationFullyPaid,
                                          MailType.PartnerRegistrationFirstPaid,
                                          MailType.PartnerRegistrationFullyPaid
                                          //MailType.SingleRegistrationFirstReminder ,
                                          //MailType.SingleRegistrationSecondReminder ,
                                          //MailType.PartnerRegistrationFirstReminder ,
                                          //MailType.PartnerRegistrationSecondReminder,
                                          //MailType.OptionsForRegistrationsOnWaitingList
                                      };
    }
}