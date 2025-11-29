using System.Net.Http.Json;

using ApiTestInfrastructure;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace EventRegistrar.Tests;

public class EventsOfUserQueryApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EventsOfUserQueryApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnauthenticatedUser_ShouldReturnEmptyLists()
    {
        // Act
        var response = await _client.PostAsync("/api/EventsOfUserQuery", new StringContent(string.Empty));
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EventsOfUserDto>();

        // Assert
        result.ShouldNotBeNull();
        result.AuthorizedEvents.ShouldNotBeNull();
        result.Requests.ShouldNotBeNull();
        result.AuthorizedEvents.ShouldBeEmpty();
        result.Requests.ShouldBeEmpty();
    }

    private class EventsOfUserDto
    {
        public IEnumerable<EventOfUserDto> AuthorizedEvents { get; set; } = new List<EventOfUserDto>();
        public IEnumerable<AccessRequestDto> Requests { get; set; } = new List<AccessRequestDto>();
    }

    private class EventOfUserDto
    {
        public System.Guid EventId { get; set; }
        public string EventName { get; set; }
        public string EventAcronym { get; set; }
        public int EventState { get; set; }
        public string EventStateText { get; set; }
        public int Role { get; set; }
        public string RoleText { get; set; }
    }

    private class AccessRequestDto
    {
        public System.Guid EventId { get; set; }
        public string EventName { get; set; }
        public string EventAcronym { get; set; }
        public int EventState { get; set; }
        public string EventStateText { get; set; }
        public System.DateTimeOffset RequestSent { get; set; }
    }
}