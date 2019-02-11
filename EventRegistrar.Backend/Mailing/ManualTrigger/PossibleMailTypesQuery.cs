using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Mailing.Templates;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.ManualTrigger
{
    public class PossibleMailTypesQuery : IRequest<IEnumerable<MailTypeItem>>, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public Guid RegistrationId { get; set; }
    }

    public class PossibleMailTypesQueryHandler : IRequestHandler<PossibleMailTypesQuery, IEnumerable<MailTypeItem>>
    {
        private readonly IQueryable<Registration> _registrations;

        public PossibleMailTypesQueryHandler(IQueryable<Registration> registrations)
        {
            _registrations = registrations;
        }

        public async Task<IEnumerable<MailTypeItem>> Handle(PossibleMailTypesQuery query, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == query.RegistrationId
                                                              && reg.EventId == query.EventId)
                                                   .FirstAsync(cancellationToken);
            var partnerRegistration = registration.RegistrationId_Partner == null
                ? null
                : await _registrations.Where(reg => reg.Id == registration.RegistrationId_Partner
                                                 && reg.EventId == query.EventId)
                                      .FirstAsync(cancellationToken);

            var possibleMailTypes = GetPossibleMailTypes(registration, partnerRegistration);

            return possibleMailTypes.Select(typ => new MailTypeItem { Type = typ, UserText = Resources.ResourceManager.GetString($"MailType_{typ}") ?? typ.ToString() });
        }

        private static IEnumerable<MailType> GetPossibleMailTypes(Registration registration, Registration partnerRegistration)
        {
            if (registration.State == RegistrationState.Cancelled)
            {
                yield return MailType.RegistrationCancelled;
            }

            if (registration.OriginalPrice == 0m && !string.IsNullOrEmpty(registration.SoldOutMessage))
            {
                yield return MailType.SoldOut;
            }

            if (registration.IsParterRegistration())
            {
                if (registration.RegistrationId_Partner == null)
                {
                    yield return registration.IsWaitingList == true ? MailType.PartnerRegistrationFirstPartnerOnWaitingList : MailType.PartnerRegistrationFirstPartnerAccepted;
                }

                var paidCount = (registration.State == RegistrationState.Paid ? 1 : 0)
                              + (partnerRegistration?.State == RegistrationState.Paid ? 1 : 0);

                if (paidCount == 0)
                {
                    yield return registration.IsWaitingList == true ? MailType.PartnerRegistrationMatchedOnWaitingList : MailType.PartnerRegistrationMatchedAndAccepted;
                }
                if (paidCount == 1)
                {
                    yield return MailType.PartnerRegistrationFirstPaid;
                }
                if (paidCount == 2)
                {
                    yield return MailType.PartnerRegistrationFullyPaid;
                }
            }
            else
            {
                if (registration.IsWaitingList == true)
                {
                    yield return MailType.SingleRegistrationOnWaitingList;
                    yield return MailType.OptionsForRegistrationsOnWaitingList;
                }

                if (registration.State == RegistrationState.Paid)
                {
                    yield return MailType.SingleRegistrationFullyPaid;
                }
                yield return MailType.SingleRegistrationAccepted;
            }
        }
    }
}