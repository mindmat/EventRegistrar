using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrator.Functions.Infrastructure.DataAccess;

namespace EventRegistrator.Functions.Registrations
{
    public static class CheckinDataView
    {
        public class CheckinItem
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Kurs { get; set; }
            public string MittagessenSamstag { get; set; }
            public string MittagessenSonntag { get; set; }
            public bool PartyPass { get; set; }
            public string Status { get; set; }
        }

        public static async Task<List<CheckinItem>> GetCheckinData(Guid eventId)
        {
            using (var dbContext = new EventRegistratorDbContext())
            {
                var registrations = await dbContext.Registrations
                                                   .Where(reg => reg.RegistrationForm.EventId == eventId &&
                                                                 reg.IsWaitingList == false &&
                                                                 reg.State != RegistrationState.Cancelled)
                                                   .Select(reg => new CheckinItem
                                                   {
                                                       Id = reg.Id,
                                                       Email = reg.RespondentEmail,
                                                       FirstName = reg.RespondentFirstName,
                                                       LastName = reg.RespondentLastName,
                                                       Kurs = reg.Seats_AsLeader.Where(seat => seat.Registrable.CheckinListColumn == "Kurs" && !seat.IsCancelled)
                                                                                .Select(seat => seat.Registrable.Name)
                                                                                .FirstOrDefault() ??
                                                              reg.Seats_AsFollower.Where(seat => seat.Registrable.CheckinListColumn == "Kurs" && !seat.IsCancelled)
                                                                                  .Select(seat => seat.Registrable.Name)
                                                                                  .FirstOrDefault(),
                                                       MittagessenSamstag = reg.Seats_AsLeader.Where(seat => seat.Registrable.CheckinListColumn == "Mittagessen Samstag" && !seat.IsCancelled)
                                                                                              .Select(seat => seat.Registrable.Name)
                                                                                              .FirstOrDefault(),
                                                       MittagessenSonntag = reg.Seats_AsLeader.Where(seat => seat.Registrable.CheckinListColumn == "Mittagessen Sonntag" && !seat.IsCancelled)
                                                                                              .Select(seat => seat.Registrable.Name)
                                                                                              .FirstOrDefault(),
                                                       PartyPass = reg.Seats_AsLeader.Count(seat => seat.Registrable.CheckinListColumn == "Parties" && !seat.IsCancelled) == 3,
                                                       Status = reg.State.ToString()
                                                   })
                                                   .Where(reg => reg.PartyPass || reg.Kurs != null)
                                                   .OrderBy(reg => reg.Kurs)
                                                   .ToListAsync();

                return registrations;
            }
        }
    }
}