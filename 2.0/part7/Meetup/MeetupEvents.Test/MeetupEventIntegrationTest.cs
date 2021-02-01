using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MeetupEvents.Contracts;
using MeetupEvents.Domain;
using MeetupEvents.Queries;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Xunit;
using Xunit.Abstractions;
using static System.Guid;
using static MeetupEvents.Contracts.MeetupCommands.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Test
{
    public class MeetupEventIntegrationTest : IClassFixture<WebApiFixture>
    {
        readonly WebApiFixture      Fixture;
        readonly HttpClient         Client;

        public MeetupEventIntegrationTest(WebApiFixture fixture, ITestOutputHelper testOutput)
        {
            Fixture        = fixture;
            Fixture.Output = testOutput;
            Client         = Fixture.CreateClient();
        }

        [Fact]
        public async Task Should_Create_MeetupEvent()
        {
            // arrange
            var meetupEventId = NewGuid();

            // act
            await CreateMeetup(meetupEventId);

            // assert
            var expectedMeetup = await Get(meetupEventId);

            Assert.Equal(expectedMeetup.Title, Title);
            Assert.Equal(expectedMeetup.Description, Description);
        }

        [Fact]
        public async Task Should_Not_Duplicate_Meetup()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);

            // act
            var result = await CreateMeetup(meetupEventId);

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
            await AttendWithRetry(joe);

            // assert
            var expectedMeetup = await Get(meetupEventId);
            expectedMeetup.Going(joe).Should().BeTrue();

            async Task AttendWithRetry(Guid userId)
                => await Retry().ExecuteAsync(() => Attend(meetupEventId, userId));
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
            expectedMeetup.NotGoing(joe).Should().BeTrue();
        }

        [Fact]
        public async Task Should_Accept_Concurrent_Attendants_WhenRetrying()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);
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

            async Task AttendWithRetry(Guid userId)
                => await Retry().ExecuteAsync(() => Attend(meetupEventId, userId));
        }

        [Fact]
        public async Task Should_Create_ConcurrencyProblem()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);
            await Publish(meetupEventId);

            var joe   = NewGuid();
            var title = "Concurrency with Aggregates";

            await Task.WhenAll(
                Attend(meetupEventId, joe),
                UpdateDetails(meetupEventId, title)
            );

            // assert
            var expectedMeetup = await Get(meetupEventId);

            expectedMeetup.Title.Should().Be(title);
            expectedMeetup.Going(joe).Should().BeTrue();
        }

        const string  BaseUrl          = "/api/meetup/events";
        string        AttendantListUrl = $"{BaseUrl}/attendant-list";
        const  string Title            = "How to create integration test with dotnet core";
        const  string Description      = "We will talk about and show and demo ....";
        static Guid   BarcelonaNetCore = NewGuid();

        async Task<HttpResponseMessage> CreateMeetup(Guid meetupEventId)
        {
            var response = await Client.PostAsJsonAsync(BaseUrl,
                new CreateMeetupEvent(meetupEventId, BarcelonaNetCore, Title, Description));

            await Client.PutAsJsonAsync($"{BaseUrl}/online",
                new MakeOnline(meetupEventId, new Uri("http://zoom.us/netcorebn")));

            var now = DateTimeOffset.UtcNow;
            await Client.PutAsJsonAsync($"{BaseUrl}/schedule",
                new Schedule(meetupEventId, now.AddDays(7), now.AddDays(7).AddHours(2)));

            // harcoded delay for eventual consistency, we should poll and retry using a query
            // await Task.Delay(1_000);

            return response;
        }

        Task<HttpResponseMessage> UpdateDetails(Guid meetupEventId, string title)
            => Client.PutAsJsonAsync($"{BaseUrl}/details",
                new UpdateDetails(meetupEventId, title, Description));

        async Task<HttpResponseMessage> Publish(Guid meetupEventId)
        {
            var response = await Client.PutAsJsonAsync($"{BaseUrl}/publish", new Publish(meetupEventId));
            // harcoded delay for eventual consistency, we should poll and retry using a query
            // await Task.Delay(1_000);
            return response;
        }

        async Task<HttpResponseMessage> Cancel(Guid meetupEventId, string reason)
        {
            var response = await Client.PutAsJsonAsync($"{BaseUrl}/cancel", new Cancel(meetupEventId, reason));
            response.EnsureSuccessStatusCode();

            return response;
        }

        Task<HttpResponseMessage> Attend(Guid meetupEventId, Guid memberId)
            => Client.PutAsJsonAsync($"{AttendantListUrl}/attend", new Attend(meetupEventId, memberId));

        Task<HttpResponseMessage> CancelAttendance(Guid meetupEventId, Guid memberId)
            => Client.PutAsJsonAsync($"{AttendantListUrl}/cancel-attendance",
                new CancelAttendance(meetupEventId, memberId));

        Task<ReadModels.V1.MeetupEvent> Get(Guid meetupEventId) =>
            Fixture.Queries.Handle(new Contracts.Queries.V1.Get(meetupEventId));

        AsyncRetryPolicy<HttpResponseMessage> Retry()
        {
            Random jitterer = new();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(jitterer.Next(50, 200)));
        }
    }

    public static class IntegrationTestExtensions
    {
        public static bool Going(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.Any(x => x.UserId == userId && !x.Waiting);

        public static bool NotGoing(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.All(x => x.UserId != userId);

        public static bool Waiting(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.Any(x => x.UserId == userId && x.Waiting);
    }
}