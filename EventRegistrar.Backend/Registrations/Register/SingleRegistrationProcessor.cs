using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventRegistrar.Backend.Properties;
using EventRegistrar.Backend.RegistrationForms.Questions;
using EventRegistrar.Backend.Seats;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Registrations.Register
{
    public class SingleRegistrationProcessor
    {
        private readonly IQueryable<QuestionOptionToRegistrableMapping> _optionToRegistrableMappings;
        private readonly PhoneNormalizer _phoneNormalizer;
        private readonly SeatManager _seatManager;

        public SingleRegistrationProcessor(PhoneNormalizer phoneNormalizer,
                                           IQueryable<QuestionOptionToRegistrableMapping> optionToRegistrableMappings,
                                           SeatManager seatManager)
        {
            _phoneNormalizer = phoneNormalizer;
            _optionToRegistrableMappings = optionToRegistrableMappings;
            _seatManager = seatManager;
        }

        public async Task<IEnumerable<Seat>> Process(Registration registration, SingleRegistrationProcessConfiguration config)
        {
            registration.RespondentFirstName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_FirstName)?.ResponseString;
            registration.RespondentLastName = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_LastName)?.ResponseString;
            registration.RespondentEmail = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Email)?.ResponseString;
            if (config.QuestionId_Phone.HasValue)
            {
                registration.Phone = registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == config.QuestionId_Phone.Value)?.ResponseString;
                registration.PhoneNormalized = _phoneNormalizer.NormalizePhone(registration.Phone);
            }

            var ownSeats = new List<Seat>();

            var questionOptionIds = new HashSet<Guid>(registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue).Select(rsp => rsp.QuestionOptionId.Value));
            var registrables = await _optionToRegistrableMappings
                                            .Where(map => questionOptionIds.Contains(map.QuestionOptionId))
                                            .Include(map => map.Registrable)
                                            .Include(map => map.Registrable.Seats)
                                            .ToListAsync();
            //var registrableIds_CheckWaitingList = new List<Guid>();
            foreach (var response in registration.Responses.Where(rsp => rsp.QuestionOptionId.HasValue))
            {
                foreach (var registrable in registrables.Where(rbl => rbl.QuestionOptionId == response.QuestionOptionId))
                {
                    //var partnerEmail = registrable.QuestionId_PartnerEmail.HasValue
                    //    ? registration.Responses.FirstOrDefault(rsp => rsp.QuestionId == registrable.QuestionId_PartnerEmail.Value)?.ResponseString
                    //    : null;
                    string partnerEmail = null;
                    var isLeader = registration.Responses.Any(rsp => rsp.QuestionOptionId == config.QuestionOptionId_Leader);
                    var isFollower = registration.Responses.Any(rsp => rsp.QuestionOptionId == config.QuestionOptionId_Follower);
                    var role = isLeader ? Role.Leader : (isFollower ? Role.Follower : (Role?)null);
                    var seat = _seatManager.ReserveSeat(registration.EventId, registrable.Registrable, registration.Id, registration.RespondentEmail, partnerEmail, role);
                    //if (registrableId_CheckWaitingList != null)
                    //{
                    //    registrableIds_CheckWaitingList.Add(registrableId_CheckWaitingList.Value);
                    //}
                    if (seat == null)
                    {
                        registration.SoldOutMessage = (registration.SoldOutMessage == null ? string.Empty : registration.SoldOutMessage + Environment.NewLine) +
                                                      string.Format(Resources.RegistrableSoldOut, registrable.Registrable.Name);
                    }
                    else
                    {
                        ownSeats.Add(seat);
                    }
                }
            }
            return ownSeats;
        }
    }
}