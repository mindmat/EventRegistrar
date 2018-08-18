using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing
{
    public class GetPendingMailsQueryHandler : IRequestHandler<GetPendingMailsQuery, IEnumerable<Mail>>
    {
        private readonly IEventAcronymResolver _acronymResolver;
        private readonly IQueryable<Mail> _mails;

        public GetPendingMailsQueryHandler(IQueryable<Mail> mails,
            IEventAcronymResolver acronymResolver)
        {
            _mails = mails;
            _acronymResolver = acronymResolver;
        }

        public async Task<IEnumerable<Mail>> Handle(GetPendingMailsQuery query, CancellationToken cancellationToken)
        {
            var eventId = await _acronymResolver.GetEventIdFromAcronym(query.EventAcronym);

            var mails = await _mails
                              .Where(mail => mail.Registrations.Any(map => map.Registration.EventId == eventId)
                                          && mail.Withhold)
                              .OrderByDescending(mail => mail.Created)
                              .ToListAsync(cancellationToken);
            return mails;
        }
    }
}