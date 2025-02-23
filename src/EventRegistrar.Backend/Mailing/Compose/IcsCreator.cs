using System.Text;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables.Calendar;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using NodaTime;

namespace EventRegistrar.Backend.Mailing.Compose;

public class IcsCreator(IQueryable<Seat> spots,
                        IQueryable<Registration> registrations,
                        CalendarConfiguration calendarConfiguration,
                        IDateTimeProvider dateTimeProvider)
{
    public async Task<MailAttachment?> Create(Guid registrationId, CancellationToken cancellationToken)
    {
        var tracks = await spots.Where(spt => spt.RegistrationId == registrationId
                                           || spt.RegistrationId_Follower == registrationId)
                                .Where(spt => !spt.IsCancelled
                                           && !spt.IsWaitingList)
                                .SelectMany(spt => spt.Registrable!.Ics!)
                                .Include(ics => ics.Registrable)
                                .ToListAsync(cancellationToken);

        if (!tracks.Any())
        {
            return null;
        }

        var registration = await registrations.Where(reg => reg.Id == registrationId)
                                              .Select(reg => new
                                                             {
                                                                 reg.RespondentFirstName,
                                                                 reg.RespondentLastName,
                                                                 EventName = reg.Event!.Name,
                                                             })
                                              .FirstAsync(cancellationToken);
        var calendarName = $"{registration.EventName} - {registration.RespondentFirstName} {registration.RespondentLastName}";

        //var serialized = ComposeWithLibrary(registration.EventName, calendarName, tracks);
        var serialized = ComposeManually(registration.EventName, calendarName, tracks);
        var attachment = new MailAttachment
                         {
                             Id = Guid.NewGuid(),
                             Name = $"{calendarName}.ics",
                             Content = serialized,
                             ContentType = "text/calendar"
                         };
        return attachment;
    }

    private byte[] ComposeManually(string eventName, string calendarName, List<RegistrableIcs> tracks)
    {
        var now = dateTimeProvider.Now;
        var sb = new StringBuilder(1000);
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine($"PRODID:-//EventRegistrar//{eventName}//DE");
        sb.AppendLine($"TIMEZONE:{calendarConfiguration.TimeZone}");
        sb.AppendLine($"X-WR-TIMEZONE:{calendarConfiguration.TimeZone}");
        sb.AppendLine($"NAME:{calendarName}");
        sb.AppendLine($"X-WR-CALNAME:{calendarName}");

        for (var i = 0; i < tracks.Count; i++)
        {
            var track = tracks[i];
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{track.Id}");
            sb.AppendLine($"SEQUENCE:{i}");
            sb.AppendLine($"DTSTART:{FormatDateTime(track.Start)}");
            sb.AppendLine($"DTEND:{FormatDateTime(track.End)}");
            sb.AppendLine($"DTSTAMP:{FormatDateTime(now)}");
            sb.AppendLine($"SUMMARY:{GetName(track)}");
            sb.AppendLine($"LOCATION:{track.Location}");
            if (!string.IsNullOrWhiteSpace(track.ContentHtml))
            {
                sb.AppendLine($"DESCRIPTION:{HtmlUtilities.ConvertToPlainText(track.ContentHtml)}");
                sb.AppendLine($"X-ALT-DESC;FMTTYPE=text/html:<!doctype html><html><body>{track.ContentHtml}</body></html>");
            }

            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private string GetName(RegistrableIcs track)
    {
        return string.IsNullOrWhiteSpace(track.Title)
                   ? track.Registrable!.DisplayName
                   : track.Title;
    }

    private static string FormatDateTime(DateTimeOffset dateTime)
    {
        return dateTime.ToString("yyyyMMdd'T'HHmmss");
    }

    private byte[] ComposeWithLibrary(string eventName, string calendarName, List<RegistrableIcs> tracks)
    {
        MemoryStream? ms = null;
        try
        {
            var calendar = new Calendar();
            var tzId = DateTimeZoneProviders.Tzdb.GetZoneOrNull(calendarConfiguration.TimeZone)!.Id;
            calendar.AddTimeZone(tzId);
            calendar.ProductId = $"EventRegistrar//{eventName}";
            calendar.Name = calendarName;
            foreach (var track in tracks)
            {
                var calendarEvent = new CalendarEvent
                                    {
                                        Uid = track.Registrable!.Id.ToString(),
                                        // If Name property is used, it MUST be RFC 5545 compliant
                                        Summary = track.Registrable!.DisplayName, // Should always be present
                                        Location = track.Location,
                                        Description = track.ContentHtml,
                                        Start = new CalDateTime(track.Start.LocalDateTime),
                                        End = new CalDateTime(track.End.LocalDateTime)
                                    };

                calendar.Events.Add(calendarEvent);
            }

            var serializer = new CalendarSerializer(calendar);
            ms = new MemoryStream();
            serializer.Serialize(calendar, ms, Encoding.ASCII);
            return ms.ToArray();
        }
        catch
        {
            ms?.Dispose();
            throw;
        }
    }
}