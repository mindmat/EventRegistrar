using System.Text;

using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Spots;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using NodaTime;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class IcsCreator(IQueryable<Seat> spots,
                            IQueryable<Registration> registrations)
    {
        public async Task<MailAttachment?> Create(Guid registrationId, CancellationToken cancellationToken)
        {
            var tracks = await spots.Where(spt => spt.RegistrationId == registrationId
                                               || spt.RegistrationId_Follower == registrationId)
                                    .Where(spt => !spt.IsCancelled
                                               && !spt.IsWaitingList)
                                    .Where(spt => spt.Registrable!.Ics!.AddToCalendar)
                                    .Select(spt => new
                                                   {
                                                       spt.RegistrableId,
                                                       spt.Registrable!.DisplayName,
                                                       spt.Registrable.NameSecondary,
                                                       spt.Registrable.Ics!.Location,
                                                       spt.Registrable.Ics!.Start,
                                                       spt.Registrable.Ics!.End,
                                                       spt.Registrable.Ics!.ContentHtml
                                                   })
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

            var calendar = new Calendar { Name = calendarName };
            foreach (var track in tracks)
            {
                var calendarEvent = new CalendarEvent
                                    {
                                        Uid = track.RegistrableId.ToString(),
                                        // If Name property is used, it MUST be RFC 5545 compliant
                                        Summary = track.DisplayName, // Should always be present
                                        Location = track.Location,
                                        Description = track.ContentHtml
                                    };
                var tzId = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Zurich")!.Id;
                calendarEvent.Start = new CalDateTime(track.Start, tzId);
                calendarEvent.End = new CalDateTime(track.End, tzId);

                calendar.Events.Add(calendarEvent);
            }

            var serializer = new CalendarSerializer(calendar);
            using var ms = new MemoryStream();
            serializer.Serialize(calendar, ms, Encoding.ASCII);
            var attachment = new MailAttachment
                             {
                                 Id = Guid.NewGuid(),
                                 Name = calendarName,
                                 Content = ms.ToArray(),
                                 ContentType = "text/calendar"
                             };
            return attachment;
        }
    }
}