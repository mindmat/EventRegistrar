using System.Text;

using EventRegistrar.Backend.Spots;

using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using NodaTime;

namespace EventRegistrar.Backend.Mailing.Compose
{
    public class IcsCreator(IQueryable<Seat> spots)
    {
        public async Task<MailAttachment?> Create(Guid registrationId, string calendarName, CancellationToken cancellationToken)
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

            var calendar = new Calendar { Name = calendarName };
            foreach (var track in tracks)
            {
                var date = new CalDateTime(2025, 3, 13, 15, 0,
                                           0);
                var calendarEvent = new CalendarEvent
                                    {
                                        Uid = track.RegistrableId.ToString(),
                                        // If Name property is used, it MUST be RFC 5545 compliant
                                        Summary = track.DisplayName, // Should always be present
                                        Location = track.Location,
                                        Description = track.ContentHtml
                                    };
                var tzId = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Zurich").Id;
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
                                 Name = "Calendar",
                                 Content = ms.ToArray(),
                                 ContentType = "text/calendar"
                             };
            return attachment;
        }
    }
}