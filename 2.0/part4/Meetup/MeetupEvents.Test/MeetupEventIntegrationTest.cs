using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MeetupEvents.Contracts;
using MeetupEvents.Domain;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Xunit;
using Xunit.Abstractions;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class MeetupEventIntegrationTest : IClassFixture<WebApiFixture>
    {
        readonly WebApiFixture     Fixture;
        readonly HttpClient        Client;
        readonly ITestOutputHelper TestOutput;


        public MeetupEventIntegrationTest(WebApiFixture fixture, ITestOutputHelper testOutput)
        {
            Fixture        = fixture;
            Fixture.Output = testOutput;
            Client         = Fixture.CreateClient();
            TestOutput     = testOutput;
        }

        [Fact]
        public async Task Should_Create_MeetupEvent()
        {
            // arrange
            var meetupEventId = NewGuid();

            // act
            var (result, meetup) = await CreateMeetup(meetupEventId);

            // assert
            var expectedMeetup = await Get(meetupEventId);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal(expectedMeetup.Title, meetup.Title);
            Assert.Equal(50, expectedMeetup.Capacity);
        }

        [Fact]
        public async Task Should_Not_Duplicate_Meetup()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);

            // act
            var (result, _) = await CreateMeetup(meetupEventId);

            // assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Publish_MeetupEvent()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);

            // act
            var result = await Publish(meetupEventId);

            // arrange
            var expectedMeetup = await Get(meetupEventId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            expectedMeetup.Status.Should().Be(MeetupEventStatus.Published.ToString());
        }

        [Fact]
        public async Task Should_Cancel_MeetupEvent()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);
            await Publish(meetupEventId);

            // act
            var result = await Cancel(meetupEventId, "covid");

            // assert
            var expectedMeetup = await Get(meetupEventId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            expectedMeetup.Status.Should().Be(MeetupEventStatus.Cancelled.ToString());
        }

        [Fact]
        public async Task Should_Return_NotFound()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);

            // act
            var result = await Publish(NewGuid());

            // assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Attend()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);
            await Publish(meetupEventId);
            var joe = NewGuid();

            // act
            await Attend(meetupEventId, joe);

            // assert
            var expectedMeetup = await Get(meetupEventId);
            expectedMeetup.Attendants.FirstOrDefault(x => x.Id == joe).Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Cancel_Attendance()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);
            await Publish(meetupEventId);

            var joe = NewGuid();
            await Attend(meetupEventId, joe);

            // act
            await CancelAttendance(meetupEventId, joe);

            // assert
            var expectedMeetup = await Get(meetupEventId);
            expectedMeetup.Attendants.FirstOrDefault(x => x.Id == joe).Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Accept_Concurrent_Attendants_WhenRetrying()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId, capacity: 2);
            await Publish(meetupEventId);

            var joe   = NewGuid();
            var carla = NewGuid();
            var bob   = NewGuid();

            await Task.WhenAll(
                AttendWithRetry(joe),
                AttendWithRetry(carla),
                AttendWithRetry(bob)
            );

            // assert
            var expectedMeetup = await Get(meetupEventId);
            expectedMeetup.Attendants.Should().HaveCount(3);
            expectedMeetup.Attendants.Any(x => x.Waiting).Should().BeTrue();

            async Task AttendWithDelay(Guid userId, int maxDelay = 200)
            {
                var jitter = new Random().Next(100, maxDelay);
                await Task.Delay(jitter);
                await Attend(meetupEventId, userId);
            }

            async Task AttendWithRetry(Guid userId)
            {
                await Retry().ExecuteAsync(() => Attend(meetupEventId, userId));
            }
        }

        [Fact]
        public async Task Should_Create_ConcurrencyProblem()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId, capacity: 2);
            await Publish(meetupEventId);

            var joe   = NewGuid();
            var carla = NewGuid();
            var title = "Concurrency with Aggregates";

            // await Task.WhenAll(
            //     Retry().ExecuteAsync(() => Attend(meetupEventId, joe)),
            //     Retry().ExecuteAsync(() => UpdateDetails(meetupEventId, title))
            // );

            await Task.WhenAll(
                Attend(meetupEventId, joe),
                UpdateDetails(meetupEventId, title)
            );

            // await Attend(meetupEventId, joe);
            // await UpdateDetails(meetupEventId, title);

            // assert
            var expectedMeetup = await Get(meetupEventId);

            expectedMeetup.Title.Should().Be(title);
            expectedMeetup.Going(joe).Should().BeTrue();

            async Task WithDelay(Func<Task> command, int maxDelay = 200)
            {
                var jitter = new Random().Next(100, maxDelay);
                await Task.Delay(jitter);
                await command();
            }
        }

        const  string BaseUrl          = "/api/meetup/events";
        const  string Title            = "How to create integration test with dotnet core";
        const  string Description      = "We will talk about and show and demo ....";
        static Guid   BarcelonaNetCore = NewGuid();

        async Task<(HttpResponseMessage, Commands.V1.Create)> CreateMeetup(Guid meetupEventId, int capacity = 0)
        {
            var meetup   = new Commands.V1.Create(meetupEventId, BarcelonaNetCore, Title, Description, capacity);
            var response = await Client.PostAsJsonAsync(BaseUrl, meetup);
            return (response, meetup);
        }

        Task<HttpResponseMessage> UpdateDetails(Guid meetupEventId, string title)
            => Client.PutAsJsonAsync($"{BaseUrl}/details",
                new Commands.V1.UpdateDetails(meetupEventId, title, Description));

        Task<HttpResponseMessage> Publish(Guid meetupEventId)
            => Client.PutAsJsonAsync($"{BaseUrl}/publish", new Commands.V1.Publish(meetupEventId));

        Task<HttpResponseMessage> Cancel(Guid meetupEventId, string reason)
            => Client.PutAsJsonAsync($"{BaseUrl}/cancel", new Commands.V1.Cancel(meetupEventId, reason));

        Task<HttpResponseMessage> Attend(Guid meetupEventId, Guid memberId)
            => Client.PutAsJsonAsync($"{BaseUrl}/attend", new Commands.V1.Attend(meetupEventId, memberId));

        Task<HttpResponseMessage> CancelAttendance(Guid meetupEventId, Guid memberId)
            => Client.PutAsJsonAsync($"{BaseUrl}/cancel-attendance",
                new Commands.V1.CancelAttendance(meetupEventId, memberId));

        Task<ReadModels.V1.MeetupEvent> Get(Guid meetupEventId)
            => Client.GetFromJsonAsync<ReadModels.V1.MeetupEvent>($"{BaseUrl}/{meetupEventId}");

        AsyncRetryPolicy<HttpResponseMessage> Retry()
        {
            Random jitterer = new();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(jitterer.Next(0, 100)));
        }
    }

    public static class IntegrationTestExtensions
    {
        public static bool Going(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.Any(x => x.Id == userId && !x.Waiting);

        public static bool NotGoing(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.Any(x => x.Id != userId);

        public static bool Waiting(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.Any(x => x.Id == userId && x.Waiting);
    }
}