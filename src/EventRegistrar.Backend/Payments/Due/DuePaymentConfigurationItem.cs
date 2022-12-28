using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Mailing;

namespace EventRegistrar.Backend.Payments.Due;

public class DefaultDuePaymentConfigurationItem : DuePaymentConfiguration, IDefaultConfigurationItem
{
    public DefaultDuePaymentConfigurationItem()
    {
        PaymentGracePeriod = 6;
        MailTypes_Accepted = new[] { MailType.PartnerRegistrationMatchedAndAccepted, MailType.SingleRegistrationAccepted };
        MailTypes_Reminder1 = new[] { MailType.PartnerRegistrationFirstReminder, MailType.SingleRegistrationFirstReminder };
        MailTypes_Reminder2 = new[] { MailType.PartnerRegistrationSecondReminder, MailType.SingleRegistrationSecondReminder };
    }
}

public class DuePaymentConfiguration : IConfigurationItem
{
    public MailType[] MailTypes_Accepted { get; set; }
    public MailType[] MailTypes_Reminder1 { get; set; }
    public MailType[] MailTypes_Reminder2 { get; set; }
    public int PaymentGracePeriod { get; set; }
}