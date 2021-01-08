using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Xunit;
using Xunit.Abstractions;
using Meetup.Scheduling.Application;
using Meetup.Scheduling.Application.Details;
using Meetup.Scheduling.Application.Queries;
using static Meetup.Scheduling.Application.Details.Commands.V1;
using static Meetup.Scheduling.Application.AttendantList.Commands.V1;
using static Meetup.Scheduling.Test.MeetupSchedulingTestExtensions;

namespace Meetup.Scheduling.Test
{
    public class MeetupSchedulingIntegrationTest : IClassFixture<WebApiFixture>, IDisposable
    {
        readonly WebApiFixture Fixture;
        readonly HttpClient    Client;

        public MeetupSchedulingIntegrationTest(WebApiFixture fixture, ITestOutputHelper output)
        {
            Fixture        = fixture;
            Fixture.Output = output;
            Client         = Fixture.CreateClient();
        }

        public void Dispose() => Fixture.Output = null;

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

        [Fact]
        public async Task Should_Retry_When_Concurrency_Conflict_Detected()
        {
            // arrange
            var eventId = await Client.CreateMeetup(capacity: 2).ThenOk();
            await Client.Publish(eventId).ThenOk();

            await Task.WhenAll(
                // Accept(carla),
                Accept(carla),
                Accept(alice),
                Accept(joe)
            );

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal(3, meetupEvent.Attendants?.Count);
            Assert.Equal(1, meetupEvent.Attendants?.Count(x => x.Status == "Waiting"));

            Task Accept(Guid userId) =>
                RandomJitter(() => Client.AcceptInvitation(eventId, userId), 0);
        }

        [Fact]
        public async Task Should_Throw_When_Concurrency_Conflict_Detected()
        {
            // arrange
            var expectedTitle = "Microservices successful case study";
            var eventId       = await Client.CreateMeetup(capacity: 2).ThenOk();
            await Client.Publish(eventId).ThenOk();

            await Task.WhenAll(
                Accept(carla),
                UpdateTitle(expectedTitle)
            );

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal("Going", meetupEvent.Attendants.Status(carla));
            Assert.Equal(expectedTitle, meetupEvent.Title);

            Task Accept(Guid userId) =>
                RandomJitter(() => Client.AcceptInvitation(eventId, userId), 0);

            Task UpdateTitle(string title) =>
                RandomJitter(() => Client.UpdateTitle(eventId, title), 0);
        }

        async Task RandomJitter(Func<Task> action, int max = 1000)
        {
            var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, max));
            await Task.Delay(jitter);
            await action();
        }

        static Guid joe   = Guid.NewGuid();
        static Guid carla = Guid.NewGuid();
        static Guid alice = Guid.NewGuid();
    }

    public static class MeetupSchedulingTestExtensions
    {
        public const string Group    = "netcorebcn";
        public const string Title    = "Microservices failures";
        public const int    Capacity = 2;

        static string BaseUrl = $"/api/meetup/{Group}/events";

        public static Task<HttpResponseMessage> CreateMeetup(this HttpClient client, string title = Title,
            int capacity = Capacity) =>
            client.PostAsync(BaseUrl, Serialize(new Commands.V1.Create(Group, title, capacity)));

        public static Task<HttpResponseMessage> UpdateTitle(this HttpClient client, Guid eventId, string title)
            => client.Put($"details", eventId, new UpdateDetails(eventId, title));

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

        public static async Task<MeetupEvent> Get(this HttpClient client, Guid eventId)
        {
            var queryResponse = await client.GetAsync($"{BaseUrl}/{eventId}");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadFromJsonAsync<MeetupEvent>();
            return queryResult;
        }

        static Task<HttpResponseMessage> Put(this HttpClient client, string url, Guid eventId, object command)
            => Retry().ExecuteAsync(() => client.PutAsync($"{BaseUrl}/{eventId}/{url}", Serialize(command)));

        static StringContent Serialize(object command)
            => new(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");

        public static string Status(this IEnumerable<Attendant> attendants, Guid userId) =>
            attendants.FirstOrDefault(x => x.UserId == userId)?.Status;

        static AsyncRetryPolicy<HttpResponseMessage> Retry()
        {
            Random jitterer = new();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(5, _ => TimeSpan.FromMilliseconds(jitterer.Next(0, 100)));
        }
    }
}