using System.Text;

using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Registrables.Calendar;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;
using EventRegistrar.Backend.VolunteerPlanning;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using NodaTime;

namespace EventRegistrar.Backend.Mailing.Compose;

public class IcsCreator(IQueryable<Seat> spots,
                        IQueryable<Registration> registrations,
                        IQueryable<ShiftAssignment> shiftAssignments,
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

        var shifts = await shiftAssignments.Where(sas => sas.RegistrationId == registrationId
                                                      && sas.IsConfirmed)
                                           .Select(sas => sas.Shift!)
                                           .ToListAsync(cancellationToken);

        if (!tracks.Any() && !shifts.Any())
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
        var serialized = ComposeManually(registration.EventName, calendarName, tracks, shifts);
        var attachment = new MailAttachment
                         {
                             Id = Guid.NewGuid(),
                             Name = $"{calendarName}.ics",
                             Content = serialized,
                             ContentType = "text/calendar"
                         };
        return attachment;
    }

    private byte[] ComposeManually(string eventName,
                                   string calendarName,
                                   List<RegistrableIcs> tracks,
                                   List<Shift> shifts)
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

        var entries = Enumerable.Concat(tracks.Select(track => new IcsEntry(track.Id,
                                                                            GetName(track),
                                                                            track.Start,
                                                                            track.End,
                                                                            track.Location,
                                                                            track.ContentHtml)),
                                        shifts.Select(shift => new IcsEntry(shift.Id,
                                                                            shift.Name,
                                                                            shift.StartTime,
                                                                            shift.EndTime,
                                                                            shift.Location,
                                                                            shift.Description)))
                                .ToList();

        for (var i = 0; i < entries.Count; i++)
        {
            AppendEvent(sb, entries[i], i, now);
        }

        sb.AppendLine("END:VCALENDAR");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private void AppendEvent(StringBuilder sb,
                             IcsEntry entry,
                             int sequence,
                             DateTimeOffset now)
    {
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:{entry.Id}");
        sb.AppendLine($"SEQUENCE:{sequence}");
        sb.AppendLine($"DTSTART:{FormatDateTime(entry.Start)}");
        sb.AppendLine($"DTEND:{FormatDateTime(entry.End)}");
        sb.AppendLine($"DTSTAMP:{FormatDateTime(now)}");
        sb.AppendLine($"SUMMARY:{entry.Summary}");
        if (!string.IsNullOrWhiteSpace(entry.Location))
        {
            sb.AppendLine($"LOCATION:{entry.Location}");
        }

        if (!string.IsNullOrWhiteSpace(entry.Description))
        {
            sb.AppendLine($"DESCRIPTION:{HtmlUtilities.ConvertToPlainText(entry.Description)}");
            sb.AppendLine($"X-ALT-DESC;FMTTYPE=text/html:<!doctype html><html><body>{entry.Description}</body></html>");
        }

        sb.AppendLine("END:VEVENT");
    }

    private record IcsEntry(Guid Id,
                            string? Summary,
                            DateTimeOffset Start,
                            DateTimeOffset End,
                            string? Location,
                            string? Description);

    private string GetName(RegistrableIcs track)
    {
        return string.IsNullOrWhiteSpace(track.Title)
                   ? track.Registrable!.DisplayName
                   : track.Title;
    }

    private string FormatDateTime(DateTimeOffset dateTime)
    {
        var eventDateTime = calendarConfiguration.ConvertToEventTime(dateTime).DateTime;
        return eventDateTime.ToString("yyyyMMdd'T'HHmmss");
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