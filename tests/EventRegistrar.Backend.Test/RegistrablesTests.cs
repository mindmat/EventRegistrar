using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Infrastructure.DataAccess.Migrations;
using EventRegistrar.Backend.Registrables;
using EventRegistrator.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SimpleInjector;
using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class RegistrablesTests : IClassFixture<WebApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _factory;

        public RegistrablesTests(WebApplicationFactory<TestStartup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetDoubleRegistrablesOverview()
        {
            var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Add(GoogleIdentityProvider.HeaderKeyIdToken, "abc");
            var container = _factory.Server.Host.Services.GetService<Container>();
            var scenario = new TestScenario();
            await scenario.Create(container);

            var response = await client.GetAsync("api/events/tev/DoubleRegistrableOverview");
            response.EnsureSuccessStatusCode();
            var registrables = (await response.Content.ReadAsAsync<IEnumerable<DoubleRegistrableDisplayItem>>()).ToList();
            registrables.Count.ShouldBe(3);
            registrables.ShouldContain(rbl => rbl.Id == scenario.RegistrableDouble1.Id
                                              && rbl.Name == scenario.RegistrableDouble1.Name
                                              && rbl.SpotsAvailable == scenario.RegistrableDouble1.MaximumDoubleSeats);
            registrables.ShouldContain(rbl => rbl.Id == scenario.RegistrableDouble2.Id
                                              && rbl.Name == scenario.RegistrableDouble2.Name
                                              && rbl.SpotsAvailable == scenario.RegistrableDouble2.MaximumDoubleSeats);
            registrables.ShouldContain(rbl => rbl.Id == scenario.RegistrableDouble3.Id
                                              && rbl.Name == scenario.RegistrableDouble3.Name
                                              && rbl.SpotsAvailable == scenario.RegistrableDouble3.MaximumDoubleSeats);
        }

        [Fact]
        public async Task GetSingleRegistrablesOverview()
        {
            var client = _factory.CreateClient();

            client.DefaultRequestHeaders.Add(GoogleIdentityProvider.HeaderKeyIdToken, "abc");
            var container = _factory.Server.Host.Services.GetService<Container>();
            var scenario = new TestScenario();
            await scenario.Create(container);

            var response = await client.GetAsync("api/events/tev/SingleRegistrableOverview");
            response.EnsureSuccessStatusCode();
            var registrables = (await response.Content.ReadAsAsync<IEnumerable<SingleRegistrableDisplayItem>>()).ToList();
            registrables.Count.ShouldBe(2);
            registrables.ShouldContain(rbl => rbl.Id == scenario.RegistrableSingle1.Id
                                           && rbl.Name == scenario.RegistrableSingle1.Name
                                           && rbl.SpotsAvailable == scenario.RegistrableSingle1.MaximumSingleSeats);
            registrables.ShouldContain(rbl => rbl.Id == scenario.RegistrableSingle2.Id
                                              && rbl.Name == scenario.RegistrableSingle2.Name
                                              && rbl.SpotsAvailable == scenario.RegistrableSingle2.MaximumSingleSeats);
        }
    }
}