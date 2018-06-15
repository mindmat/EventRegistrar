using System;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace EventRegistrar.Backend.Test.Infrastructure
{
    public class TestScenario : IDisposable
    {
        public User Administrator => new User
        {
            Id = new Guid("BC4E197E-9EB3-4014-A38F-C08160563267"),
            FirstName = "Alice",
            LastName = "Admin",
            IdentityProvider = IdentityProvider.Google,
            IdentityProviderUserIdentifier = "john.admin@gmail.com"
        };

        public Event OtherOwnEvent => new Event
        {
            Id = new Guid("733A954C-A751-46DD-A8BA-3AFBBC54D459"),
            Name = "OtherOwnEvent",
            Acronym = "ooe",
            State = State.RegistrationOpen,
        };

        public Event TestEvent => new Event
        {
            Id = new Guid("ACBCA10C-B53F-4DAA-8D00-D1ED9394A294"),
            Name = "TestEvent",
            Acronym = "tev",
            State = State.RegistrationOpen,
        };

        public User User => new User
        {
            Id = new Guid("E24CFA7C-20D7-4AA4-B646-4CB0B1E8D6FC"),
            FirstName = "Ulysses",
            LastName = "User",
            IdentityProvider = IdentityProvider.Google,
            IdentityProviderUserIdentifier = "ulysses.user@gmail.com"
        };

        public void Create(Container container)
        {
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                InsertData(container);
                container.GetInstance<DbContext>().SaveChanges();
            }
        }

        public void Dispose()
        {
        }

        private void InsertData(Container container)
        {
            var events = container.GetInstance<IRepository<Event>>();
            events.InsertOrUpdateEntity(new Event
            {
                Id = new Guid("F569251D-E1FB-444B-AD68-3BBAFD64319D"),
                Name = "PastEvent",
                Acronym = "pev",
                State = State.Finished
            });

            events.InsertOrUpdateEntity(TestEvent);
            events.InsertOrUpdateEntity(new Event
            {
                Id = new Guid("6A916C80-AD0F-4548-BCD6-80F2DC617365"),
                Name = "OtherCurrentEvent",
                Acronym = "cev",
                State = State.RegistrationOpen
            });
            events.InsertOrUpdateEntity(OtherOwnEvent);
            events.InsertOrUpdateEntity(new Event
            {
                Id = new Guid("E5AB67E4-9D1E-49CA-8069-FAA6F785C107"),
                Name = "FutureEvent",
                Acronym = "fev",
                State = State.Setup
            });

            var users = container.GetInstance<IRepository<User>>();
            users.InsertOrUpdateEntity(User);
            users.InsertOrUpdateEntity(Administrator);
            users.InsertOrUpdateEntity(new User
            {
                Id = new Guid("0083875C-3945-4CCD-88F7-4F39AF52765E"),
                FirstName = "Bob",
                LastName = "Burns",
                IdentityProvider = IdentityProvider.Google,
                IdentityProviderUserIdentifier = "bob.burns@gmail.com"
            });

            var usersInEvents = container.GetInstance<IRepository<UserInEvent>>();
            usersInEvents.InsertOrUpdateEntity(new UserInEvent
            {
                EventId = TestEvent.Id,
                UserId = Administrator.Id,
                Role = UserInEventRole.Admin
            });
            usersInEvents.InsertOrUpdateEntity(new UserInEvent
            {
                EventId = TestEvent.Id,
                UserId = User.Id,
                Role = UserInEventRole.Writer
            });
            usersInEvents.InsertOrUpdateEntity(new UserInEvent
            {
                EventId = OtherOwnEvent.Id,
                UserId = Administrator.Id,
                Role = UserInEventRole.Admin
            });
            usersInEvents.InsertOrUpdateEntity(new UserInEvent
            {
                EventId = OtherOwnEvent.Id,
                UserId = User.Id,
                Role = UserInEventRole.Reader
            });
        }
    }
}