using System.Configuration;

using EventRegistrar.Backend.Infrastructure.Configuration;

namespace EventRegistrar.Backend.Registrables.Calendar;

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

public static class CalendarConfigurationExtensions
{
    public static  DateTimeOffset ConvertToEventTime(this CalendarConfiguration config, DateTimeOffset dateTime)
    {
        try
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, config.TimeZone);
        }
        catch
        {
            if (config.FallbackOffset != null)
            {
                return dateTime.ToOffset(config.FallbackOffset.Value);
            }

            return dateTime;
        }
    }
}