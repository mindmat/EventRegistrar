using System.Text.RegularExpressions;

using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Registrations;
using EventRegistrar.Backend.Registrations.ReadModels;

namespace EventRegistrar.Backend.Mailing.InvalidAddresses;

public class FixInvalidAddressCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? NewEmailAddress { get; set; }
    public string? OldEmailAddress { get; set; }
    public Guid RegistrationId { get; set; }
}

public class FixInvalidAddressCommandHandler : AsyncRequestHandler<FixInvalidAddressCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IRepository<Registration> _registrations;
    private readonly ReadModelUpdater _readModelUpdater;

    public FixInvalidAddressCommandHandler(IRepository<Registration> registrations,
                                           ReadModelUpdater readModelUpdater,
                                           IEventBus eventBus)
    {
        _registrations = registrations;
        _readModelUpdater = readModelUpdater;
        _eventBus = eventBus;
    }

    protected override async Task Handle(FixInvalidAddressCommand command, CancellationToken cancellationToken)
    {
        var registration = await _registrations.Where(reg => reg.Id == command.RegistrationId)
                                               .FirstAsync(cancellationToken);

        if (registration.RespondentEmail != command.OldEmailAddress)
        {
            throw new ArgumentException(
                $"Email address has been changed by somebody else. Expected {command.OldEmailAddress}, actual {registration.RespondentEmail}");
        }

        if (command.NewEmailAddress == null
         || !Regex.IsMatch(command.NewEmailAddress,
                           @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                           RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
        {
            throw new ArgumentException($"{command.NewEmailAddress} is not a valid email address");
        }

        registration.RespondentEmail = command.NewEmailAddress;

        _eventBus.Publish(new InvalidEmailAddressFixed
                          {
                              EventId = command.EventId,
                              RegistrationId = command.RegistrationId,
                              OldEmailAddress = command.OldEmailAddress,
                              NewEmailAddress = command.NewEmailAddress
                          });

        _readModelUpdater.TriggerUpdate<RegistrationCalculator>(registration.Id, registration.EventId);
    }
}