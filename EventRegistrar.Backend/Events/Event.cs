using System;
using System.Collections.Generic;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrar.Backend.Registrations;

namespace EventRegistrar.Backend.Events
{
    public class Event : Entity
    {
        public ICollection<AccessToEventRequest> AccessRequests { get; set; }
        public string AccountIban { get; set; }
        public string Acronym { get; set; }
        public string Configuration { get; set; }
        public ICollection<EventConfiguration> Configurations { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public State State { get; set; }
        public string TwilioAccountSid { get; set; }
        public Guid? PredecessorEventId { get; set; }
        public ICollection<UserInEvent> Users { get; set; }
        public ICollection<Registrable> Registrables { get; set; }
        public ICollection<Registration> Registrations { get; set; }
        public Event PredecessorEvent { get; set; }
    }
}