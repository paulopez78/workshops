using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.AttendantList;
using Meetup.Scheduling.Framework;
using static Meetup.Scheduling.Contracts.MeetupDetailsCommands.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    public class MeetupDetailsCommandApiAsync :
        IConsumer<CreateMeetup>,
        IConsumer<UpdateDetails>,
        IConsumer<MakeOnline>,
        IConsumer<MakeOnsite>,
        IConsumer<Schedule>,
        IConsumer<Publish>,
        IConsumer<Cancel>,
        IConsumer<Start>,
        IConsumer<Finish>
    {
        readonly HandleCommand<AttendantListAggregate> Handle;

        public MeetupDetailsCommandApiAsync(HandleCommand<AttendantListAggregate> handle)
            => Handle = handle;

        public Task Consume(ConsumeContext<CreateMeetup> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<UpdateDetails> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<MakeOnline> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<MakeOnsite> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Schedule> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Publish> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Cancel> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Start> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Finish> context)
            => Handle.WithContext(context).Invoke(context.Message.EventId, context.Message);
    }
}