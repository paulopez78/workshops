using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using static Meetup.Scheduling.Commands.V1;
using static Meetup.Scheduling.Test.MeetupSchedulingTestExtensions;

namespace Meetup.Scheduling.Test
{
    public class MeetupSchedulingTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        readonly HttpClient Client;

        public MeetupSchedulingTest(WebApplicationFactory<Startup> fixture) => Client = fixture.CreateClient();

        [Fact]
        public async Task Should_Create_Meetup_Event()
        {
            // act
            var eventId = await Client.CreateMeetup().ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal(Title, meetupEvent.Title);
        }

        [Fact]
        public async Task Should_Publish_Meetup_Event()
        {
            // arrange
            var eventId = await Client.CreateMeetup().ThenOk();

            // act
            await Client.Publish(eventId).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal("Scheduled", meetupEvent.Status);
        }

        [Fact]
        public async Task Should_Accept_Invitation()
        {
            // arrange
            var eventId = await Client.CreateMeetup().ThenOk();
            await Client.Publish(eventId).ThenOk();

            // act
            await Client.AcceptInvitation(eventId, joe).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal("Going", meetupEvent.Attendants.Status(joe));
        }

        [Fact]
        public async Task Should_Not_Accept_Invitation_When_Not_Scheduled()
        {
            // arrange
            var eventId = await Client.CreateMeetup().ThenOk();

            // act
            await Client.AcceptInvitation(eventId, joe).ThenBadRequest();
        }

        [Fact]
        public async Task Should_Decline_Invitation()
        {
            // arrange
            var eventId = await Client.CreateMeetup().ThenOk();
            await Client.Publish(eventId).ThenOk();

            // act
            await Client.DeclineInvitation(eventId, joe).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal("NotGoing", meetupEvent.Attendants.Status(joe));
        }

        [Fact]
        public async Task Should_Joe_Wait()
        {
            // arrange
            var eventId = await Client.CreateMeetup(capacity: 2).ThenOk();
            await Client.Publish(eventId).ThenOk();

            // act
            await Client.AcceptInvitation(eventId, carla).ThenOk();
            await Client.AcceptInvitation(eventId, alice).ThenOk();
            await Client.AcceptInvitation(eventId, joe).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal(3, meetupEvent.Attendants.Count);
            Assert.Equal("Going", meetupEvent.Attendants.Status(carla));
            Assert.Equal("Going", meetupEvent.Attendants.Status(alice));
            Assert.Equal("Waiting", meetupEvent.Attendants.Status(joe));
        }

        static Guid joe = Guid.NewGuid();
        static Guid carla = Guid.NewGuid();
        static Guid alice = Guid.NewGuid();
    }

    public static class MeetupSchedulingTestExtensions
    {
        public const string Group = "netcorebcn";
        public const string Title = "Microservices failures";
        public const int Capacity = 2;
        static string BaseUrl = $"/api/meetup/{Group}/events";

        public static Task<HttpResponseMessage> CreateMeetup(this HttpClient client, string title = Title,
            int capacity = Capacity) =>
            client.PostAsync(BaseUrl, Serialize(new Create(Group, title, capacity)));

        public static Task<HttpResponseMessage> Publish(this HttpClient client, Guid eventId)
            => client.Put($"publish", eventId, new Publish(eventId));

        public static Task<HttpResponseMessage> AcceptInvitation(this HttpClient client, Guid eventId, Guid userId)
            => client.Put($"invitations/accept", eventId, new AcceptInvitation(eventId, userId));

        public static Task<HttpResponseMessage> DeclineInvitation(this HttpClient client, Guid eventId, Guid userId)
            => client.Put($"invitations/decline", eventId, new DeclineInvitation(eventId, userId));

        public static async Task<Guid> ThenOk(this Task<HttpResponseMessage> httpResponseMessage)
        {
            var result = await httpResponseMessage;
            Assert.True(result.IsSuccessStatusCode);

            var commandResult = await result.Content.ReadFromJsonAsync<CommandResult>();
            return commandResult.EventId;
        }

        public static async Task ThenBadRequest(this Task<HttpResponseMessage> httpResponseMessage)
        {
            var result = await httpResponseMessage;
            Assert.False(result.IsSuccessStatusCode);
        }

        public static async Task<Data.MeetupEvent> Get(this HttpClient client, Guid eventId)
        {
            var queryResponse = await client.GetAsync($"{BaseUrl}/{eventId}");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadFromJsonAsync<Data.MeetupEvent>();
            return queryResult;
        }

        static Task<HttpResponseMessage> Put(this HttpClient client, string url, Guid eventId, object command)
            => client.PutAsync($"{BaseUrl}/{eventId}/{url}", Serialize(command));

        static StringContent Serialize(object command)
            => new(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");

        public static string Status(this IEnumerable<Data.Attendant> attendants, Guid userId) =>
            attendants.FirstOrDefault(x => x.UserId == userId)?.Status;
    }
}