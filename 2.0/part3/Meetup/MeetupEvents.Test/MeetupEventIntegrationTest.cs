using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MeetupEvents.Application;
using MeetupEvents.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class MeetupEventIntegrationTest : IClassFixture<WebApiFixture>
    {
        readonly         WebApiFixture Fixture;
        private readonly HttpClient    Client;

        public MeetupEventIntegrationTest(WebApiFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture        = fixture;
            Fixture.Output = testOutputHelper;
            Client         = Fixture.CreateClient();
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

            var expectedMeetup = await Get(meetupEventId);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            expectedMeetup.Status.Should().Be(MeetupEventStatus.Published);
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
            expectedMeetup.Status.Should().Be(MeetupEventStatus.Cancelled);
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

        const string BaseUrl  = "/api/meetup/events";
        const string Title    = "How to create integration test with dotnet core";
        const int    Capacity = 0;

        async Task<(HttpResponseMessage, MeetupEvent)> CreateMeetup(Guid meetupEventId)
        {
            var meetup   = new MeetupEvent(meetupEventId, Title, Capacity);
            var response = await Client.PostAsJsonAsync(BaseUrl, meetup);
            return (response, meetup);
        }

        Task<HttpResponseMessage> Publish(Guid meetupEventId)
            => Client.PutAsJsonAsync($"{BaseUrl}/publish", new Publish(meetupEventId));

        Task<HttpResponseMessage> Cancel(Guid meetupEventId, string reason)
            => Client.PutAsJsonAsync($"{BaseUrl}/cancel", new Cancel(meetupEventId, reason));

        Task<MeetupEvent> Get(Guid meetupEventId)
            => Client.GetFromJsonAsync<MeetupEvent>($"{BaseUrl}/{meetupEventId}");
    }
}