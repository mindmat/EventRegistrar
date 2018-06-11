﻿using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Users;
using System;

namespace EventRegistrar.Backend.Events.UsersInEvents
{
    public class UserInEvent : Entity
    {
        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public UserInEventRole Role { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
    }
}