using System;
using System.Threading.Tasks;
using Marten;
using MassTransit;
using Meetup.Scheduling.Framework;
using Meetup.Scheduling.Queries;

namespace Meetup.Scheduling.AttendantList
{
    public class MeetupCanceledMassTransitDomainEventHandler : IConsumer<MeetupDetails.Events.V1.Cancelled>
    {
        readonly HandleCommand<AttendantListAggregate> Handle;
        readonly IDocumentStore                        Store;

        public MeetupCanceledMassTransitDomainEventHandler(
            HandleCommand<AttendantListAggregate> handle,
            IDocumentStore store)
        {
            Handle = handle;
            Store  = store;
        }

        public async Task Consume(ConsumeContext<MeetupDetails.Events.V1.Cancelled> context)
        {
            var id = await Store.GetAttendantListId(context.Message.Id);
            if (id is null)
                throw new InvalidOperationException("AttendantList Id not found");

            await Handle
                .WithContext(context)
                .Invoke(id.Value, new Commands.V1.Close(id.Value));
        }
    }
}