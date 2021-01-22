using System.Linq;
using System.Threading.Tasks;
using Marten;
using MassTransit;
using Meetup.Scheduling.Framework;
using Meetup.Scheduling.Queries;

namespace Meetup.Scheduling.AttendantList
{
    public class GroupMemberLeftEventHandler : IConsumer<GroupManagement.Contracts.Events.V1.MeetupGroupMemberLeft>
    {
        readonly HandleCommand<AttendantListAggregate> Handle;

        readonly IDocumentStore Store;


        public GroupMemberLeftEventHandler(HandleCommand<AttendantListAggregate> handle, IDocumentStore store)
        {
            Handle = handle;
            Store  = store;
        }

        public async Task Consume(ConsumeContext<GroupManagement.Contracts.Events.V1.MeetupGroupMemberLeft> context)
        {
            var commandHandler = Handle.WithContext(context);
            var memberLeft     = context.Message;

            var meetupEvents = await Store.Handle(
                new V1.GetByGroup(memberLeft.GroupSlug)
            );

            await Task.WhenAll(
                meetupEvents
                    .Where(x => x.AttendantListId is not null)
                    .Select(x =>
                        commandHandler(
                            x.AttendantListId.Value,
                            new Commands.V1.DontAttend(x.AttendantListId.Value, memberLeft.UserId)
                        )
                    )
            );
        }
    }
}