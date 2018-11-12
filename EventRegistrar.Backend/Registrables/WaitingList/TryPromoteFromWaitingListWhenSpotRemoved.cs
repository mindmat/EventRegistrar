﻿using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Spots;

namespace EventRegistrar.Backend.Registrables.WaitingList
{
    public class TryPromoteFromWaitingListWhenSpotRemoved : IEventToCommandTranslation<SpotRemoved>
    {
        public IQueueBoundMessage Translate(SpotRemoved e)
        {
            if (e.WasSpotOnWaitingList)
            {
                return new TryPromoteFromWaitingListCommand { RegistrableId = e.RegistrableId };
            }

            return null;
        }
    }
}