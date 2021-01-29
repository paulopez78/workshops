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
using static MeetupEvents.Contracts.MeetupEventsCommands.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Test
{
    public class MeetupEventIntegrationTest : IClassFixture<WebApiFixture>
    {
        readonly WebApiFixture Fixture;
        readonly HttpClient    Client;

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
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();

            // act
            await CreateMeetup(meetupEventId, attendantListId);

            // assert
            var expectedMeetup = await Get(meetupEventId);

            Assert.Equal(expectedMeetup.Title, Title);
            Assert.Equal(expectedMeetup.Description, Description);
        }

        [Fact]
        public async Task Should_Not_Duplicate_Meetup()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();
            await CreateMeetup(meetupEventId, attendantListId);

            // act
            var result = await CreateMeetup(meetupEventId, attendantListId);

            // assert
            result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Publish_MeetupEvent()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();
            await CreateMeetup(meetupEventId, attendantListId);

            // act
            var result = await Publish(meetupEventId, attendantListId);

            // arrange
            var expectedMeetup = await Get(meetupEventId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            expectedMeetup.Status.Should().Be(MeetupEventStatus.Published.ToString());
        }

        [Fact]
        public async Task Should_Cancel_MeetupEvent()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();

            await CreateMeetup(meetupEventId, attendantListId);
            await Publish(meetupEventId, attendantListId);

            // act
            var result = await Cancel(meetupEventId, "covid", attendantListId);

            // assert
            var expectedMeetup = await Get(meetupEventId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            expectedMeetup.Status.Should().Be(MeetupEventStatus.Cancelled.ToString());
        }

        [Fact]
        public async Task Should_Return_NotFound()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();

            await CreateMeetup(meetupEventId, attendantListId);

            // act
            var result = await Publish(NewGuid(), NewGuid());

            // assert
            result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Should_Attend()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();

            await CreateMeetup(meetupEventId, attendantListId);
            await Publish(meetupEventId, attendantListId);
            var joe = NewGuid();

            // act
            await Attend(attendantListId, joe);

            // assert
            var expectedMeetup = await Get(meetupEventId);
            expectedMeetup.Going(joe).Should().BeTrue();
        }

        [Fact]
        public async Task Should_Cancel_Attendance()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();

            await CreateMeetup(meetupEventId, attendantListId);
            await Publish(meetupEventId, attendantListId);

            var joe = NewGuid();
            await Attend(attendantListId, joe);

            // act
            await CancelAttendance(attendantListId, joe);

            // assert
            var expectedMeetup = await Get(meetupEventId);
            expectedMeetup.NotGoing(joe).Should().BeTrue();
        }

        [Fact]
        public async Task Should_Accept_Concurrent_Attendants_WhenRetrying()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();
            await CreateMeetup(meetupEventId, attendantListId, capacity: 2);
            await Publish(meetupEventId, attendantListId);

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
                await Attend(attendantListId, userId);
            }

            async Task AttendWithRetry(Guid userId)
            {
                await Retry().ExecuteAsync(() => Attend(attendantListId, userId));
            }
        }

        [Fact]
        public async Task Should_Create_ConcurrencyProblem()
        {
            // arrange
            var meetupEventId   = NewGuid();
            var attendantListId = NewGuid();
            await CreateMeetup(meetupEventId, attendantListId, capacity: 2);
            await Publish(meetupEventId, attendantListId);

            var joe   = NewGuid();
            var carla = NewGuid();
            var title = "Concurrency with Aggregates";

            // await Task.WhenAll(
            //     Retry().ExecuteAsync(() => Attend(meetupEventId, joe)),
            //     Retry().ExecuteAsync(() => UpdateDetails(meetupEventId, title))
            // );

            await Task.WhenAll(
                Attend(attendantListId, joe),
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

        const string  BaseUrl          = "/api/meetup/events";
        string        AttendantListUrl = $"{BaseUrl}/attendant-list";
        const  string Title            = "How to create integration test with dotnet core";
        const  string Description      = "We will talk about and show and demo ....";
        static Guid   BarcelonaNetCore = NewGuid();

        async Task<HttpResponseMessage> CreateMeetup(Guid meetupEventId, Guid attendantListId, int capacity = 0)
        {
            var meetup   = new CreateMeetupEvent(meetupEventId, BarcelonaNetCore, Title, Description);
            var response = await Client.PostAsJsonAsync(BaseUrl, meetup);
            await Client.PostAsJsonAsync($"{AttendantListUrl}",
                new CreateAttendantList(attendantListId, meetupEventId, capacity));

            return response;
        }

        Task<HttpResponseMessage> UpdateDetails(Guid meetupEventId, string title)
            => Client.PutAsJsonAsync($"{BaseUrl}/details",
                new UpdateDetails(meetupEventId, title, Description));

        async Task<HttpResponseMessage> Publish(Guid meetupEventId, Guid attendantListId)
        {
            var response = await Client.PutAsJsonAsync($"{BaseUrl}/publish", new Publish(meetupEventId));
            await Client.PutAsJsonAsync($"{AttendantListUrl}/open", new Open(attendantListId));
            return response;
        }

        async Task<HttpResponseMessage> Cancel(Guid meetupEventId, string reason, Guid attendantListId)
        {
            var response = await Client.PutAsJsonAsync($"{BaseUrl}/cancel", new Cancel(meetupEventId, reason));
            response.EnsureSuccessStatusCode();
            
            await Client.PutAsJsonAsync($"{AttendantListUrl}/close", new Close(attendantListId));
            return response;
        }

        Task<HttpResponseMessage> Attend(Guid attendantListId, Guid memberId)
            => Client.PutAsJsonAsync($"{AttendantListUrl}/attend", new Attend(attendantListId, memberId));

        Task<HttpResponseMessage> CancelAttendance(Guid attendantListId, Guid memberId)
            => Client.PutAsJsonAsync($"{AttendantListUrl}/cancel-attendance",
                new CancelAttendance(attendantListId, memberId));

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
            => meetup.Attendants.Any(x => x.UserId == userId && !x.Waiting);

        public static bool NotGoing(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.All(x => x.UserId != userId);

        public static bool Waiting(this ReadModels.V1.MeetupEvent meetup, Guid userId)
            => meetup.Attendants.Any(x => x.UserId == userId && x.Waiting);
    }
}