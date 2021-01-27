using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        public async Task Should_Publish_MeetupEvent()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);

            // act
            var result = await Publish(meetupEventId);

            var expectedMeetup = await Get(meetupEventId);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(expectedMeetup.Published);
        }

        [Fact]
        public async Task Should_Cancel_MeetupEvent()
        {
            // arrange
            var meetupEventId = NewGuid();
            await CreateMeetup(meetupEventId);
            await Publish(meetupEventId);

            // act
            var result = await Cancel(meetupEventId);

            // assert
            var exists = await Exists(meetupEventId);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.False(exists);
        }

        const string BaseUrl = "/api/meetup/events";

        const string Title    = "How to create integration test with dotnet core";
        const int    Capacity = 0;

        async Task<(HttpResponseMessage, MeetupEvent)> CreateMeetup(Guid meetupEventId)
        {
            var meetup   = new MeetupEvent(meetupEventId, Title, Capacity);
            var response = await Client.PostAsJsonAsync(BaseUrl, meetup);
            return (response, meetup);
        }

        Task<HttpResponseMessage> Publish(Guid meetupEventId)
            => Client.PutAsync($"{BaseUrl}/{meetupEventId}", null);

        Task<HttpResponseMessage> Cancel(Guid meetupEventId)
            => Client.DeleteAsync($"{BaseUrl}/{meetupEventId}");

        Task<MeetupEvent> Get(Guid meetupEventId)
            => Client.GetFromJsonAsync<MeetupEvent>($"{BaseUrl}/{meetupEventId}");

        async Task<bool> Exists(Guid meetupEventId)
        {
            var response = await Client.GetAsync($"{BaseUrl}/{meetupEventId}");
            return response.StatusCode == HttpStatusCode.OK;
        }
    }
}