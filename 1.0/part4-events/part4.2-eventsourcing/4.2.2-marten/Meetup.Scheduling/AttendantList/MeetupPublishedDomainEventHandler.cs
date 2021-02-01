using System;
using System.Threading.Tasks;
using Marten;
using MassTransit;
using Microsoft.Extensions.Logging;
using Meetup.Scheduling.Framework;
using Meetup.Scheduling.Queries;

namespace Meetup.Scheduling.AttendantList
{
    public class MeetupPublishedMassTransitDomainEventHandler : IConsumer<MeetupDetails.Events.V1.Published>
    {
        readonly HandleCommand<AttendantListAggregate> Handle;

        readonly IDocumentStore Store;

        readonly ILogger<MeetupCreatedMassTransitDomainEventHandler> Logger;

        public MeetupPublishedMassTransitDomainEventHandler(
            HandleCommand<AttendantListAggregate> handle,
            IDocumentStore store,
            ILogger<MeetupCreatedMassTransitDomainEventHandler> logger
        )
        {
            Handle = handle;
            Store  = store;
            Logger = logger;
        }

        public async Task Consume(ConsumeContext<MeetupDetails.Events.V1.Published> context)
        {
            var id = await Store.GetAttendantListId(context.Message.Id);
            if (id is null)
                throw new InvalidOperationException("AttendantList Id not found");

            await Handle.WithContext(context)
                .Invoke(id.Value, new Commands.V1.Open(id.Value));
        }
    }
}