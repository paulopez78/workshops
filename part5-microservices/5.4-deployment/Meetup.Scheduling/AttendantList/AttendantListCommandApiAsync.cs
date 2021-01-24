using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Framework;
using static Meetup.Scheduling.Contracts.AttendantListCommands.V1;

namespace Meetup.Scheduling.AttendantList
{
    public class AttendantListCommandApiAsync :
        IConsumer<CreateAttendantList>,
        IConsumer<Open>,
        IConsumer<Close>,
        IConsumer<Archive>,
        IConsumer<ReduceCapacity>,
        IConsumer<IncreaseCapacity>,
        IConsumer<Attend>,
        IConsumer<DontAttend>
    {
        readonly HandleCommand<AttendantListAggregate> Handle;

        public AttendantListCommandApiAsync(HandleCommand<AttendantListAggregate> handle)
            => Handle = handle;

        public Task Consume(ConsumeContext<CreateAttendantList> context)
            => Handle.WithContext(context)
                (context.Message.Id, context.Message);

        public Task Consume(ConsumeContext<Open> context)
            => Handle.WithContext(context)
                (context.Message.MeetupEventId, context.Message);

        public Task Consume(ConsumeContext<Close> context)
            => Handle.WithContext(context)
                (context.Message.MeetupEventId, context.Message);

        public Task Consume(ConsumeContext<Archive> context)
            => Handle.WithContext(context)
                (context.Message.MeetupEventId, context.Message);

        public Task Consume(ConsumeContext<ReduceCapacity> context)
            => Handle.WithContext(context)
                (context.Message.MeetupEventId, context.Message);

        public Task Consume(ConsumeContext<IncreaseCapacity> context)
            => Handle.WithContext(context)
                (context.Message.MeetupEventId, context.Message);

        public Task Consume(ConsumeContext<Attend> context)
            => Handle.WithContext(context)
                (context.Message.MeetupEventId, context.Message);

        public Task Consume(ConsumeContext<DontAttend> context)
            => Handle.WithContext(context)
                (context.Message.MeetupEventId, context.Message);
    }
}