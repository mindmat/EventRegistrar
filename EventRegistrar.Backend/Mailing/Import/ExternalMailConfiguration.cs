using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Mailing.Import
{
    public class ExternalMailConfiguration : IConfigurationItem
    {
        public string ImapHost { get; set; }
        public int ImapPort { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }

    internal class NullExternalMailConfiguration : ExternalMailConfiguration, IDefaultConfigurationItem
    {
    }
}