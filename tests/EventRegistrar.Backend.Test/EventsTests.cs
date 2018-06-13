using System;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.RegistrationForms;
using EventRegistrator.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class EventsTests : IClassFixture<WebApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _factory;

        public EventsTests(WebApplicationFactory<TestStartup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetEventsOfUser()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-MS-TOKEN-GOOGLE-ID-TOKEN", "");
            var container = _factory.Server.Host.Services.GetService<Container>();
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var events = container.GetInstance<IRepository<Event>>();
                await events.InsertOrUpdateEntity(new Event
                {
                    Id = Guid.NewGuid(),
                    Name = "TestEvent",
                    State = State.Setup,
                    Acronym = "tev"
                });
                await container.GetInstance<DbContext>().SaveChangesAsync();
            }

            var response = await client.GetAsync("me/events");
            response.EnsureSuccessStatusCode();
        }
    }
}