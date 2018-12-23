using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses
{
    public class FixInvalidAddressCommand : IRequest, IEventBoundRequest
    {
        public Guid EventId { get; set; }
        public string NewEmailAddress { get; set; }
        public string OldEmailAddress { get; set; }
        public Guid RegistrationId { get; set; }
    }

    public class FixInvalidAddressCommandHandler : IRequestHandler<FixInvalidAddressCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly IRepository<Registration> _registrations;

        public FixInvalidAddressCommandHandler(IRepository<Registration> registrations,
                                               IEventBus eventBus)
        {
            _registrations = registrations;
            _eventBus = eventBus;
        }

        public async Task<Unit> Handle(FixInvalidAddressCommand command, CancellationToken cancellationToken)
        {
            var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                                   .Include(reg => reg.Mails).ThenInclude(map => map.Mail)
                                                   .FirstAsync(cancellationToken);

            if (registration.RespondentEmail != command.OldEmailAddress)
            {
                throw new ArgumentException($"Email address has been changed by womebody else. Expected {command.OldEmailAddress}, actual {registration.RespondentEmail}");
            }

            if (!Regex.IsMatch(command.NewEmailAddress,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
            {
                throw new ArgumentException($"{command.NewEmailAddress} is not a valid email address");
            }

            registration.RespondentEmail = command.NewEmailAddress;

            _eventBus.Publish(new InvalidEmailAddressFixed { EventId = command.EventId, OldEmailAddress = command.OldEmailAddress, NewEmailAddress = command.NewEmailAddress });

            return Unit.Value;
        }
    }
}