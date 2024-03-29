﻿using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Authentication.Users;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Events.UsersInEvents.AccessRequests;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.RegistrationForms;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;

public class TestScenario : IDisposable
{
    public const string IdentityProviderUserIdentifierReader = "ulysses.user@gmail.com";
    public const string UserIdReader = "E24CFA7C-20D7-4AA4-B646-4CB0B1E8D6FC";

    public AccessToEventRequest AccessRequest => new()
                                                 {
                                                     Id = new Guid("7DF6C289-B282-4F86-BAE5-14160BA6CD72"),
                                                     UserId_Requestor = Reader.Id,
                                                     EventId = FutureEvent.Id,
                                                     IdentityProvider = Reader.IdentityProvider,
                                                     Identifier = Reader.IdentityProviderUserIdentifier,
                                                     FirstName = Reader.FirstName,
                                                     LastName = Reader.LastName,
                                                     Email = Reader.Email,
                                                     RequestReceived = new DateTime(2018, 1, 1)
                                                 };

    public User Administrator => new()
                                 {
                                     Id = new Guid("BC4E197E-9EB3-4014-A38F-C08160563267"),
                                     FirstName = "Alice",
                                     LastName = "Admin",
                                     IdentityProvider = IdentityProvider.Google,
                                     IdentityProviderUserIdentifier = "john.admin@gmail.com"
                                 };

    public Event OtherCurrentEvent => new()
                                      {
                                          Id = new Guid("6A916C80-AD0F-4548-BCD6-80F2DC617365"),
                                          Name = "OtherCurrentEvent",
                                          Acronym = "cev",
                                          State = EventState.RegistrationOpen
                                      };

    public Event OtherOwnEvent => new()
                                  {
                                      Id = new Guid("733A954C-A751-46DD-A8BA-3AFBBC54D459"),
                                      Name = "OtherOwnEvent",
                                      Acronym = "ooe",
                                      State = EventState.RegistrationOpen
                                  };

    public Event PastEvent => new()
                              {
                                  Id = new Guid("F569251D-E1FB-444B-AD68-3BBAFD64319D"),
                                  Name = "PastEvent",
                                  Acronym = "pev",
                                  State = EventState.Finished
                              };

    public User Reader => new()
                          {
                              Id = new Guid(UserIdReader),
                              FirstName = "Ulysses",
                              LastName = "User",
                              IdentityProvider = IdentityProvider.Google,
                              IdentityProviderUserIdentifier = IdentityProviderUserIdentifierReader
                          };

    public Registrable RegistrableDouble1 => new()
                                             {
                                                 Id = new Guid("E8EA681E-26A9-4C24-8951-2C45FE2D456C"),
                                                 Name = "Double 1",
                                                 EventId = TestEvent.Id,
                                                 HasWaitingList = true,
                                                 IsCore = true,
                                                 MaximumAllowedImbalance = 3,
                                                 MaximumDoubleSeats = 30,
                                                 Price = 100,
                                                 ShowInMailListOrder = 1
                                             };

    public Registrable RegistrableDouble2 => new()
                                             {
                                                 Id = new Guid("967DBBD8-8BC4-4F57-AEE7-C50010C89570"),
                                                 Name = "Double 2",
                                                 EventId = TestEvent.Id,
                                                 HasWaitingList = true,
                                                 IsCore = true,
                                                 MaximumAllowedImbalance = 3,
                                                 MaximumDoubleSeats = 30,
                                                 Price = 100,
                                                 ShowInMailListOrder = 2
                                             };

    public Registrable RegistrableDouble3 => new()
                                             {
                                                 Id = new Guid("4A210228-743E-44A3-B2DD-F75A663C05D5"),
                                                 Name = "Double 3",
                                                 EventId = TestEvent.Id,
                                                 HasWaitingList = true,
                                                 IsCore = true,
                                                 MaximumAllowedImbalance = 3,
                                                 MaximumDoubleSeats = 25,
                                                 Price = 100,
                                                 ShowInMailListOrder = 3
                                             };

    public Registrable RegistrableSingle1 => new()
                                             {
                                                 Id = new Guid("9E5ABA02-4C2C-44FA-B988-A0ED2F074847"),
                                                 Name = "Single 1",
                                                 EventId = TestEvent.Id,
                                                 HasWaitingList = true,
                                                 IsCore = true,
                                                 MaximumSingleSeats = 40,
                                                 Price = 80,
                                                 ShowInMailListOrder = 4
                                             };

    public Registrable RegistrableSingle2 => new()
                                             {
                                                 Id = new Guid("5EA300D6-698F-422C-A63C-5F0A596992C8"),
                                                 Name = "Single 2",
                                                 EventId = TestEvent.Id,
                                                 HasWaitingList = true,
                                                 IsCore = true,
                                                 MaximumSingleSeats = 35,
                                                 Price = 80,
                                                 ShowInMailListOrder = 5
                                             };

    public Event TestEvent => new()
                              {
                                  Id = new Guid("ACBCA10C-B53F-4DAA-8D00-D1ED9394A294"),
                                  Name = "TestEvent",
                                  Acronym = "tev",
                                  State = EventState.RegistrationOpen
                              };

    private Event FutureEvent => new()
                                 {
                                     Id = new Guid("E5AB67E4-9D1E-49CA-8069-FAA6F785C107"),
                                     Name = "FutureEvent",
                                     Acronym = "fev",
                                     State = EventState.Setup
                                 };

    public async Task Create(Container container)
    {
        using (AsyncScopedLifestyle.BeginScope(container))
        {
            await InsertData(container);
            await container.GetInstance<DbContext>().SaveChangesAsync();
        }
    }

    public void Dispose() { }

    private async Task InsertAccessToEventRequests(Container container)
    {
        var accessToEventRequests = container.GetInstance<IRepository<AccessToEventRequest>>();
        await accessToEventRequests.InsertOrUpdateEntity(AccessRequest);
    }

    private async Task InsertData(Container container)
    {
        await InsertEvents(container);
        await InsertUsers(container);
        await InsertUsersInEvents(container);
        await InsertAccessToEventRequests(container);
        await InsertRegistrables(container);
    }

    private async Task InsertEvents(Container container)
    {
        var events = container.GetInstance<IRepository<Event>>();
        await events.InsertOrUpdateEntity(PastEvent);
        await events.InsertOrUpdateEntity(TestEvent);
        await events.InsertOrUpdateEntity(OtherCurrentEvent);
        await events.InsertOrUpdateEntity(OtherOwnEvent);
        await events.InsertOrUpdateEntity(FutureEvent);
    }

    private async Task InsertRegistrables(Container container)
    {
        var users = container.GetInstance<IRepository<Registrable>>();
        await users.InsertOrUpdateEntity(RegistrableDouble1);
        await users.InsertOrUpdateEntity(RegistrableDouble2);
        await users.InsertOrUpdateEntity(RegistrableDouble3);
        await users.InsertOrUpdateEntity(RegistrableSingle1);
        await users.InsertOrUpdateEntity(RegistrableSingle2);
    }

    private async Task InsertUsers(Container container)
    {
        var users = container.GetInstance<IRepository<User>>();
        await users.InsertOrUpdateEntity(Reader);
        await users.InsertOrUpdateEntity(Administrator);
        await users.InsertOrUpdateEntity(new User
                                         {
                                             Id = new Guid("0083875C-3945-4CCD-88F7-4F39AF52765E"),
                                             FirstName = "Bob",
                                             LastName = "Burns",
                                             IdentityProvider = IdentityProvider.Google,
                                             IdentityProviderUserIdentifier = "bob.burns@gmail.com"
                                         });
    }

    private async Task InsertUsersInEvents(Container container)
    {
        var usersInEvents = container.GetInstance<IRepository<UserInEvent>>();
        await usersInEvents.InsertOrUpdateEntity(new UserInEvent
                                                 {
                                                     Id = new Guid("313EE6C4-87DB-4957-946E-661BF375B370"),
                                                     EventId = TestEvent.Id,
                                                     UserId = Administrator.Id,
                                                     Role = UserInEventRole.Admin
                                                 });
        await usersInEvents.InsertOrUpdateEntity(new UserInEvent
                                                 {
                                                     Id = new Guid("657111B0-180C-496D-8A64-A875893E9D0A"),
                                                     EventId = TestEvent.Id,
                                                     UserId = Reader.Id,
                                                     Role = UserInEventRole.Writer
                                                 });
        await usersInEvents.InsertOrUpdateEntity(new UserInEvent
                                                 {
                                                     Id = new Guid("8A166C5C-14FA-4FC8-B787-3520D3E013A1"),
                                                     EventId = OtherOwnEvent.Id,
                                                     UserId = Administrator.Id,
                                                     Role = UserInEventRole.Admin
                                                 });
        await usersInEvents.InsertOrUpdateEntity(new UserInEvent
                                                 {
                                                     Id = new Guid("38AB3F52-D3BD-43CF-978B-564782C26734"),
                                                     EventId = OtherOwnEvent.Id,
                                                     UserId = Reader.Id,
                                                     Role = UserInEventRole.Reader
                                                 });
    }
}