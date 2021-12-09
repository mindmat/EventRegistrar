using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Mailing.Import;

public class ExternalMailConfiguration
{
    public string ImapHost { get; set; }
    public int ImapPort { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
}

public class ExternalMailConfigurations : IConfigurationItem
{
    public IEnumerable<ExternalMailConfiguration> MailConfigurations { get; set; }
}

internal class NullExternalMailConfigurations : ExternalMailConfigurations, IDefaultConfigurationItem
{
}