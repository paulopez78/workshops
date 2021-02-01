using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Xunit;
using Xunit.Abstractions;
using Meetup.Scheduling.Infrastructure;
using Meetup.Scheduling.Queries;
using static System.Guid;
using static Meetup.Scheduling.MeetupDetails.Commands.V1;
using static Meetup.Scheduling.AttendantList.Commands.V1;
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
            var start   = DateTimeOffset.UtcNow.AddDays(7);
            await Client.Schedule(eventId, start, start.AddHours(2)).ThenOk();
            await Client.MakeOnline(eventId, "https://zoom.us/netcorebcn").ThenOk();

            // act
            await Client.Publish(eventId).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal("Published", meetupEvent.Status);
        }

        [Fact]
        public async Task Should_Accept_Invitation()
        {
            // arrange
            var eventId = await Client.CreatePublishedMeetup().ThenOk();

            // act
            await Client.Attend(eventId, joe).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            meetupEvent.Going(joe).Should().BeTrue();
        }

        [Fact]
        public async Task Should_Decline_Invitation()
        {
            // arrange
            var eventId = await Client.CreatePublishedMeetup().ThenOk();

            // act
            await Client.DontAttend(eventId, joe).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            meetupEvent.NotGoing(joe).Should().BeTrue();
        }

        [Fact]
        public async Task Should_Joe_Wait()
        {
            // arrange
            var eventId = await Client.CreatePublishedMeetup().ThenOk();

            // act
            await Client.Attend(eventId, carla).ThenOk();
            await Client.Attend(eventId, alice).ThenOk();
            await Client.Attend(eventId, joe).ThenOk();
            await Client.DontAttend(eventId, alice).ThenOk();
            await Client.Attend(eventId, alice).ThenOk();

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal(3, meetupEvent.Attendants.Count);

            meetupEvent.Going(carla).Should().BeTrue();
            meetupEvent.Waiting(alice).Should().BeTrue();
            meetupEvent.Going(joe).Should().BeTrue();
        }

        [Fact]
        public async Task Should_Retry_When_Concurrency_Conflict_Detected()
        {
            // arrange
            var eventId = await Client.CreatePublishedMeetup().ThenOk();

            await Task.WhenAll(
                Accept(carla),
                Accept(alice),
                Accept(joe)
            );

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.Equal(3, meetupEvent.Attendants?.Count);
            Assert.Equal(1, meetupEvent.Attendants?.Count(x => x.Waiting));

            Task Accept(Guid userId) =>
                RandomJitter(() => Client.Attend(eventId, userId), 0);
        }

        [Fact]
        public async Task Should_Throw_When_Concurrency_Conflict_Detected()
        {
            // arrange
            var expectedTitle       = "Microservices successful case study";
            var expectedDescription = "Microservices successful case study description";

            var eventId = await Client.CreatePublishedMeetup().ThenOk();

            await Task.WhenAll(
                Accept(carla),
                UpdateTitle()
            );

            // assert
            var meetupEvent = await Client.Get(eventId);
            Assert.True(meetupEvent.Going(carla));
            Assert.Equal(expectedTitle, meetupEvent.Title);

            Task Accept(Guid userId) =>
                RandomJitter(() => Client.Attend(eventId, userId), 0);

            Task UpdateTitle() =>
                RandomJitter(() => Client.UpdateTitle(eventId, expectedTitle, expectedDescription), 0);
        }

        async Task RandomJitter(Func<Task> action, int max = 1000)
        {
            var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, max));
            await Task.Delay(jitter);
            await action();
        }

        static Guid joe   = NewGuid();
        static Guid carla = NewGuid();
        static Guid alice = NewGuid();
    }

    public static class MeetupSchedulingTestExtensions
    {
        public const string Group       = "netcorebcn";
        public const string Title       = "Microservices failures";
        public const string Description = "Microservices failures description";
        public const int    Capacity    = 2;

        const  string BaseUrl      = "/api/meetup";
        static string QueryBaseUrl = $"{BaseUrl}/{Group}/events";

        public static async Task<HttpResponseMessage> CreateMeetup(this HttpClient client)
        {
            var result = await client.Post("events/details",
                new Create(NewGuid(), Group, Title, Description, Capacity));

            // eventual consistency hack, better to poll (query) checking consistency with a timeout
            await Task.Delay(5_000);
            return result;
        }

        public static async Task<HttpResponseMessage> CreatePublishedMeetup(this HttpClient client)
        {
            var eventId = await client.CreateMeetup().ThenOk();
            var start   = DateTimeOffset.UtcNow.AddDays(7);
            await client.Schedule(eventId, start, start.AddHours(2)).ThenOk();
            await client.MakeOnline(eventId, "https://zoom.us/netcorebcn").ThenOk();

            var result = await client.Publish(eventId);

            // eventual consistency hack, better to poll (query) checking consistency with a timeout
            await Task.Delay(5_000);

            return result;
        }

        public static Task<HttpResponseMessage> UpdateTitle(this HttpClient client, Guid eventId, string title,
            string description)
            => client.Put($"events/details", new UpdateDetails(eventId, title, description));

        public static Task<HttpResponseMessage> Schedule(this HttpClient client, Guid eventId, DateTimeOffset start,
            DateTimeOffset end)
            => client.Put($"events/schedule", new Schedule(eventId, start, end));

        public static Task<HttpResponseMessage> MakeOnline(this HttpClient client, Guid eventId, string url)
            => client.Put($"events/makeonline", new MakeOnline(eventId, url));

        public static Task<HttpResponseMessage> Publish(this HttpClient client, Guid eventId)
            => client.Put($"events/publish", new Publish(eventId));

        public static Task<HttpResponseMessage> Attend(this HttpClient client, Guid eventId,
            Guid userId)
            => client.Put($"attendants/add", new Attend(eventId, userId));

        public static Task<HttpResponseMessage> DontAttend(this HttpClient client, Guid eventId,
            Guid userId)
            => client.Put($"attendants/remove", new DontAttend(eventId, userId));

        public static async Task<Guid> ThenOk(this Task<HttpResponseMessage> httpResponseMessage)
        {
            var result = await httpResponseMessage;
            Assert.True(result.IsSuccessStatusCode);

            var commandResult = await result.Content.ReadFromJsonAsync<CommandResult>();
            return commandResult!.AggregateId;
        }

        public static async Task ThenBadRequest(this Task<HttpResponseMessage> httpResponseMessage)
        {
            var result = await httpResponseMessage;
            Assert.False(result.IsSuccessStatusCode);
        }

        public static async Task<MeetupEvent> Get(this HttpClient client, Guid eventId)
        {
            var queryResponse = await client.GetAsync($"{QueryBaseUrl}/{eventId}");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadFromJsonAsync<MeetupEvent>();
            return queryResult;
        }

        static Task<HttpResponseMessage> Put(this HttpClient client, string url, object command)
        {
            var messageId = NewGuid();
            return Retry().ExecuteAsync(() =>
            {
                client.AddIdempotencyKey(messageId);
                return client.PutAsync($"{BaseUrl}/{url}", Serialize(command));
            });
        }

        static Task<HttpResponseMessage> Post(this HttpClient client, string url, object command)
        {
            var messageId = NewGuid();
            return Retry().ExecuteAsync(() =>
            {
                client.AddIdempotencyKey(messageId);
                return client.PostAsync($"{BaseUrl}/{url}", Serialize(command));
            });
        }

        static void AddIdempotencyKey(this HttpClient client, Guid id)
        {
            client.DefaultRequestHeaders.Remove("Idempotency-Key");
            client.DefaultRequestHeaders.Add("Idempotency-Key", id.ToString());
        }

        static StringContent Serialize(object command)
            => new(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");

        public static bool Going(this MeetupEvent meetup, Guid userId) =>
            meetup.Attendants.Any(x => x.UserId == userId && !x.Waiting);

        public static bool Waiting(this MeetupEvent meetup, Guid userId) =>
            meetup.Attendants.Any(x => x.UserId == userId && x.Waiting);

        public static bool NotGoing(this MeetupEvent meetup, Guid userId) =>
            meetup.Attendants.All(x => x.UserId != userId);

        static AsyncRetryPolicy<HttpResponseMessage> Retry()
        {
            Random jitterer = new();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(jitterer.Next(0, 100)));
        }
    }
}