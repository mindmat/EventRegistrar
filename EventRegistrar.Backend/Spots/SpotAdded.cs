﻿using System;
using EventRegistrar.Backend.Infrastructure.Events;

namespace EventRegistrar.Backend.Spots
{
    public class SpotAdded : Event
    {
        public Guid RegistrableId { get; set; }
        public Guid RegistrationId { get; set; }
    }
}