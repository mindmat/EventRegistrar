using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Registrables.Calendar
{
    public class CalendarConfiguration : IConfigurationItem
    {
        public string TimeZone { get; set; }
        public TimeSpan? FallbackOffset { get; set; }
    }

    public class DefaultCalendarConfiguration : CalendarConfiguration, IDefaultConfigurationItem
    {
        public DefaultCalendarConfiguration()
        {
            TimeZone = "Europe/Zurich";
            FallbackOffset = TimeSpan.FromHours(2);
        }
    }
}