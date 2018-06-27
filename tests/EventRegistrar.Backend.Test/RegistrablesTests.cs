using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Registrables;
using EventRegistrar.Backend.Test.Infrastructure;
using Shouldly;
using Xunit;

namespace EventRegistrar.Backend.Test
{
    public class RegistrablesTests : IClassFixture<IntegrationTestEnvironment>
    {
        private readonly IntegrationTestEnvironment _integrationTestEnvironment;

        public RegistrablesTests(IntegrationTestEnvironment integrationTestEnvironment)
        {
            _integrationTestEnvironment = integrationTestEnvironment;
        }

        [Fact]
        public async Task GetDoubleRegistrablesOverview()
        {
            var client = _integrationTestEnvironment.GetClient(UserInEventRole.Reader);

            var response = await client.GetAsync("api/events/tev/DoubleRegistrableOverview");
            response.EnsureSuccessStatusCode();
            var registrables = (await response.Content.ReadAsAsync<IEnumerable<DoubleRegistrableDisplayItem>>()).ToList();
            registrables.Count.ShouldBe(3);
            registrables.ShouldContain(rbl => rbl.Id == _integrationTestEnvironment.Scenario.RegistrableDouble1.Id
                                              && rbl.Name == _integrationTestEnvironment.Scenario.RegistrableDouble1.Name
                                              && rbl.SpotsAvailable == _integrationTestEnvironment.Scenario.RegistrableDouble1.MaximumDoubleSeats);
            registrables.ShouldContain(rbl => rbl.Id == _integrationTestEnvironment.Scenario.RegistrableDouble2.Id
                                              && rbl.Name == _integrationTestEnvironment.Scenario.RegistrableDouble2.Name
                                              && rbl.SpotsAvailable == _integrationTestEnvironment.Scenario.RegistrableDouble2.MaximumDoubleSeats);
            registrables.ShouldContain(rbl => rbl.Id == _integrationTestEnvironment.Scenario.RegistrableDouble3.Id
                                              && rbl.Name == _integrationTestEnvironment.Scenario.RegistrableDouble3.Name
                                              && rbl.SpotsAvailable == _integrationTestEnvironment.Scenario.RegistrableDouble3.MaximumDoubleSeats);
        }

        [Fact]
        public async Task GetSingleRegistrablesOverview()
        {
            var client = _integrationTestEnvironment.GetClient(UserInEventRole.Reader);

            var response = await client.GetAsync("api/events/tev/SingleRegistrableOverview");
            response.EnsureSuccessStatusCode();
            var registrables = (await response.Content.ReadAsAsync<IEnumerable<SingleRegistrableDisplayItem>>()).ToList();
            registrables.Count.ShouldBe(2);
            registrables.ShouldContain(rbl => rbl.Id == _integrationTestEnvironment.Scenario.RegistrableSingle1.Id
                                           && rbl.Name == _integrationTestEnvironment.Scenario.RegistrableSingle1.Name
                                           && rbl.SpotsAvailable == _integrationTestEnvironment.Scenario.RegistrableSingle1.MaximumSingleSeats);
            registrables.ShouldContain(rbl => rbl.Id == _integrationTestEnvironment.Scenario.RegistrableSingle2.Id
                                              && rbl.Name == _integrationTestEnvironment.Scenario.RegistrableSingle2.Name
                                              && rbl.SpotsAvailable == _integrationTestEnvironment.Scenario.RegistrableSingle2.MaximumSingleSeats);
        }
    }
}