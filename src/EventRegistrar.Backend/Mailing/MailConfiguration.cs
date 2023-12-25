using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Mailing;

public class MailConfiguration : IConfigurationItem
{
    public string? SenderName { get; set; }
    public string SenderMail { get; set; } = null!;
    public bool SingleRegistrationPossible { get; set; }
    public bool PartnerRegistrationPossible { get; set; }
    public IEnumerable<string> AvailableLanguages { get; set; } = null!;
    public string? FallbackLanguage { get; set; }
    public MailSender MailSender { get; set; }
    public SmtpConfiguration? SmtpConfiguration { get; set; }
}

public class SmtpConfiguration
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class DefaultMailConfiguration : MailConfiguration, IDefaultConfigurationItem
{
    public DefaultMailConfiguration()
    {
        AvailableLanguages = new[] { Language.German, Language.English };
        SingleRegistrationPossible = true;
        PartnerRegistrationPossible = true;
        SenderMail = "registration@leapinlindy.ch";
        SenderName = "Leapin' Lindy";
        FallbackLanguage = Language.English;
        MailSender = MailSender.SendGrid;
    }
}

public enum MailSender
{
    Smtp = 1,
    SendGrid = 2,
    Postmark = 3
}