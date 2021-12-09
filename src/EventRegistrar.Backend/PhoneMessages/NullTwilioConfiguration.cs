using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.PhoneMessages;

internal class NullTwilioConfiguration : TwilioConfiguration, IDefaultConfigurationItem
{
}