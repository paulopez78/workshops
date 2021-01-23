using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Guid;
using static Meetup.EndToEndTest.UserProfileExtensions;
using static Meetup.EndToEndTest.NotificationsExtensions;

namespace Meetup.EndToEndTest
{
    public class EndToEndTest : IClassFixture<ClientsFixture>
    {
        readonly ClientsFixture Fixture;
        public EndToEndTest(ClientsFixture fixture) => Fixture = fixture;

        [Fact]
        public async Task Meetup_EndToEnd_Test()
        {
            await Fixture.UserProfile
                .CreateUserProfile(Pau, Joe, Carla, Alice, Bob);

            await Fixture.GroupManagementCommands
                .StartMeetupGroup(
                    NetCoreBcn,
                    "Barcelona .NET Core",
                    "This is an awesome group",
                    groupSlug: NetCoreBcnSlug,
                    location: "Barcelona",
                    organizer: Pau
                );

            // Add some members to the group
            await Fixture.GroupManagementCommands
                .AddGroupMember(NetCoreBcn, Joe, Carla, Alice, Bob);

            // create meetup
            await Fixture.MeetupSchedulingCommands
                .CreateMeetup(
                    MicroservicesMeetup,
                    NetCoreBcnSlug,
                    "Microservices failures",
                    "This is a talk about failures",
                    capacity: 2
                );

            await Fixture.MeetupSchedulingCommands
                .MakeOnline(MicroservicesMeetup, "https://zoom.us/netcorebcn");

            await Fixture.MeetupSchedulingCommands
                .Schedule(MicroservicesMeetup, StartTime, EndTime);

            await Fixture.MeetupSchedulingCommands
                .Publish(MicroservicesMeetup);

            await Task.Delay(1000);
            
            await Fixture.MeetupSchedulingCommands
                .Attend(MicroservicesMeetup,
                    Joe,
                    Carla,
                    Alice,
                    Bob
                );

            await Fixture.MeetupSchedulingCommands
                .DontAttend(MicroservicesMeetup, Joe);

            await Fixture.MeetupSchedulingCommands
                .ReduceCapacity(MicroservicesMeetup, byNumber: 1);

            await Fixture.MeetupSchedulingCommands
                .IncreaseCapacity(MicroservicesMeetup, byNumber: 1);

            await Fixture.GroupManagementCommands
                .LeaveGroup(NetCoreBcn, Alice);
            
            await Fixture.MeetupSchedulingCommands
                .Attend(MicroservicesMeetup, Joe);

            // assert
            var meetup = await Fixture.MeetupSchedulingQueries.Get(NetCoreBcnSlug, MicroservicesMeetup);

            meetup.Status.Should().Be("Finished");
            meetup.AttendantListStatus.Should().Be("Archived");

            meetup.Waiting(Joe).Should().BeTrue();
            meetup.Going(Carla).Should().BeTrue();
            meetup.Going(Bob).Should().BeTrue();

            (await Fixture.Notifications
                    .UserNotifications(Joe)
                    .OfType(NotificationType.Attending))
                .Should().BeTrue();

            (await Fixture.Notifications
                    .UserNotifications(Carla)
                    .OfType(NotificationType.Attending))
                .Should().BeTrue();

            (await Fixture.Notifications
                    .UserNotifications(Alice)
                    .OfType(NotificationType.Waiting))
                .Should().BeTrue();

            (await Fixture.Notifications
                    .UserNotifications(Pau)
                    .OfType(NotificationType.MemberJoined, NotificationType.MemberLeft))
                .Should().BeTrue();
        }

        string NetCoreBcnSlug      = "netcorebcn";
        string NetCoreBcn          = NewGuid().ToString();
        Guid   MicroservicesMeetup = NewGuid();

        DateTimeOffset StartTime = DateTimeOffset.UtcNow.AddSeconds(10);
        DateTimeOffset EndTime   = DateTimeOffset.UtcNow.AddSeconds(20);

        User Pau   = new(NewGuid(), "Pau", "Lopez", "pau.lopez@meetup.com");
        User Joe   = new(NewGuid(), "Joe", "Smith", "joe.smith@meetup.com");
        User Carla = new(NewGuid(), "Carla", "Garcia", "carla.garcia@meetup.com");
        User Alice = new(NewGuid(), "Alice", "Joplin", "alice.joplin@meetup.com");
        User Bob   = new(NewGuid(), "Bob", "Dylan", "bob.dylan@meetup.com");
    }
}