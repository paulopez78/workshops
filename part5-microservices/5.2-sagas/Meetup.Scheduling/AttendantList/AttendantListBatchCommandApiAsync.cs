using System.Linq;
using System.Threading.Tasks;
using Marten;
using MassTransit;
using static Meetup.Scheduling.Contracts.AttendantListCommands.V1;

namespace Meetup.Scheduling.AttendantList
{
    public class AttendantListBatchCommandApiAsync : IConsumer<RemoveAttendantFromMeetups>
    {
        readonly IDocumentStore DocumentStore;

        public AttendantListBatchCommandApiAsync(IDocumentStore store) => DocumentStore = store;

        public async Task Consume(ConsumeContext<RemoveAttendantFromMeetups> context)
        {
            var opened = await DocumentStore.GetOpenedAttendantLists(context.Message.GroupSlug);
            await Task.WhenAll(
                opened.Select(list =>
                    context.Send(new DontAttend(list, context.Message.UserId))
                )
            );
        }
    }
}