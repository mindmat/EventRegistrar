using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace EventRegistrar.Backend.Mailing.Templates
{
    public class MailTypesQueryHandler : IRequestHandler<MailTypesQuery, IEnumerable<MailTypeItem>>
    {
        public Task<IEnumerable<MailTypeItem>> Handle(MailTypesQuery request, CancellationToken cancellationToken)
        {
            var list = Enum.GetValues(typeof(MailType))
                .Cast<MailType>().Select(mtp => new MailTypeItem { Type = mtp, UserText = mtp.ToString() });
            return Task.FromResult(list);
        }
    }
}