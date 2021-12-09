using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.PhoneMessages;

public class TwilioConfiguration : IConfigurationItem
{
    public string Number { get; set; }
    public string Sid { get; set; }
    public string Token { get; set; }
}